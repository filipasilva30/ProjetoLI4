using LI4.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LI4.Data.Services
{
    public class ProductionService
    {
        private readonly SqlDataAccess _db;

        public ProductionService(SqlDataAccess db)
        {
            _db = db;
        }

        public async Task<string> IniciarProducaoAsync(int encomendaId)
        {
            var sqlEnc = @"SELECT * FROM Encomenda WHERE Numero = @EncId AND PagamentoEfetuado = 1 AND Estado = 'Em espera'";
            var enc = await _db.LoadData<Encomenda, dynamic>(sqlEnc, new { EncId = encomendaId });
            if (enc == null || enc.Count == 0)
                return "Encomenda não disponível para produção.";

            var sqlUpdate = @"UPDATE Encomenda SET Estado = 'Em produção' WHERE Numero = @EncId";
            await _db.SaveData(sqlUpdate, new { EncId = encomendaId });
            return "Produção iniciada com sucesso.";
        }

        public async Task<(bool PodeProduzir, string Mensagem, Dictionary<int, int> ProdutosProduziveis)> VerificarMateriaisParaProducaoAsync(int encomendaId)
        {
            var sqlProdutosEncomenda = @"
                SELECT ep.IdProduto, ep.Quantidade, pm.IdMaterial, pm.Quantidade AS QtdNecessaria
                FROM Encomenda_tem_Produto ep
                JOIN Produto_tem_Material pm ON ep.IdProduto = pm.IdProduto
                WHERE ep.NumEncomenda = @EncId";

            var produtosEncomenda = await _db.LoadData<dynamic, dynamic>(sqlProdutosEncomenda, new { EncId = encomendaId });

            if (produtosEncomenda == null || produtosEncomenda.Count == 0)
                return (false, "Nenhum produto encontrado para esta encomenda.", null);

            var sqlMateriaisStock = "SELECT Id, Quantidade FROM Material";
            var materiaisStock = await _db.LoadData<Material, dynamic>(sqlMateriaisStock, new { });

            Dictionary<int, int> materiaisDisponiveis = materiaisStock.ToDictionary(m => m.Id, m => m.Quantidade);
            Dictionary<int, int> produtosProduziveis = new();

            foreach (var item in produtosEncomenda)
            {
                int idProduto = item.IdProduto;
                int qtdProduto = item.Quantidade;
                int idMaterial = item.IdMaterial;
                int qtdNecessaria = item.QtdNecessaria * qtdProduto;

                if (materiaisDisponiveis.ContainsKey(idMaterial))
                {
                    int qtdDisponivel = materiaisDisponiveis[idMaterial];

                    if (qtdDisponivel < qtdNecessaria)
                    {
                        int maxProduzivel = qtdDisponivel / item.QtdNecessaria;
                        produtosProduziveis[idProduto] = maxProduzivel;

                        if (maxProduzivel == 0)
                            return (false, $"Material insuficiente para produzir o produto {idProduto}.", produtosProduziveis);
                    }
                    else
                    {
                        produtosProduziveis[idProduto] = qtdProduto;
                    }
                }
                else
                {
                    return (false, $"Material necessário para o produto {idProduto} não encontrado no stock.", null);
                }
            }

            return (true, "Materiais disponíveis para produção.", produtosProduziveis);
        }


        public async Task<string> EnviarEncomendaAsync(int encomendaId)
        {
            var sqlEnc = @"SELECT * FROM Encomenda WHERE Numero = @EncId AND Estado = 'Em produção'";
            var enc = await _db.LoadData<Encomenda, dynamic>(sqlEnc, new { EncId = encomendaId });
            if (enc == null || enc.Count == 0)
                return "Encomenda não está pronta para envio.";

            var sqlUpdate = @"UPDATE Encomenda SET Estado = 'Enviada' WHERE Numero = @EncId";
            await _db.SaveData(sqlUpdate, new { EncId = encomendaId });
            return "Encomenda enviada para o cliente.";
        }
    }
}