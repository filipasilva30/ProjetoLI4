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

            // Mapeia os materiais disponíveis
            Dictionary<int, int> materiaisDisponiveis = materiaisStock.ToDictionary(m => m.Id, m => m.Quantidade);
            Dictionary<int, int> produtosProduziveis = new();

            // Para cada produto na encomenda, verifica a quantidade de produção
            foreach (var item in produtosEncomenda)
            {
                int idProduto = item.IdProduto;
                int qtdProduto = item.Quantidade;

                // Busca os materiais necessários para o produto
                var materiaisNecessarios = await _db.LoadData<dynamic, dynamic>(
                    @"
            SELECT pm.IdMaterial, pm.Quantidade AS QtdNecessaria
            FROM Produto_tem_Material pm
            WHERE pm.IdProduto = @ProdutoId", new { ProdutoId = idProduto });

                int qtdMaxProduzivel = int.MaxValue; // Começa com o valor máximo possível

                foreach (var material in materiaisNecessarios)
                {
                    int idMaterial = material.IdMaterial;
                    int qtdNecessaria = material.QtdNecessaria * qtdProduto;  // Total de material necessário para a quantidade de produtos

                    if (materiaisDisponiveis.ContainsKey(idMaterial))
                    {
                        int qtdDisponivel = materiaisDisponiveis[idMaterial];

                        // Se o material não tem quantidade suficiente, calcula a capacidade máxima de produção
                        if (qtdDisponivel < qtdNecessaria)
                        {
                            int maxProduzivelParaMaterial = qtdDisponivel / material.QtdNecessaria;
                            qtdMaxProduzivel = Math.Min(qtdMaxProduzivel, maxProduzivelParaMaterial);
                        }
                    }
                    else
                    {
                        return (false, $"Material necessário para o produto {idProduto} não encontrado no stock.", null);
                    }
                }

                // Agora, definimos a quantidade máxima que pode ser produzida com base no material mais restritivo
                if (qtdMaxProduzivel == int.MaxValue)
                {
                    qtdMaxProduzivel = qtdProduto;  // Se todos os materiais têm quantidade suficiente
                }

                produtosProduziveis[idProduto] = qtdMaxProduzivel;

                if (qtdMaxProduzivel == 0)
                {
                    return (false, $"Material insuficiente para produzir o produto {idProduto}.", produtosProduziveis);
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