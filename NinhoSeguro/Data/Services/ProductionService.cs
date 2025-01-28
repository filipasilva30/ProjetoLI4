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
            var sqlProdutosEncomenda = @"SELECT ep.IdProduto, ep.Quantidade, pm.IdMaterial, pm.Quantidade AS QtdNecessaria
                                        FROM Encomenda_tem_Produto ep
                                        JOIN Produto_tem_Material pm ON ep.IdProduto = pm.IdProduto
                                        WHERE ep.NumEncomenda = @EncId";

            var produtosEncomenda = await _db.LoadData<dynamic, dynamic>(sqlProdutosEncomenda, new { EncId = encomendaId });

            if (produtosEncomenda == null || produtosEncomenda.Count == 0)
            {
                return (false, "Nenhum produto encontrado para esta encomenda.", null);
            }

            // Obter stock atual de materiais e produtos
            var sqlMateriaisStock = "SELECT Id, Quantidade FROM Material";
            var materiaisStock = await _db.LoadData<Material, dynamic>(sqlMateriaisStock, new { });
            var materiaisDisponiveis = materiaisStock.ToDictionary(m => m.Id, m => m.Quantidade);

            var sqlProdutosStock = "SELECT Id, Quantidade FROM Produto";
            var produtosStock = await _db.LoadData<Produto, dynamic>(sqlProdutosStock, new { });
            var produtosDisponiveis = produtosStock.ToDictionary(p => p.Id, p => p.Quantidade);

            // Calcular quantidades que faltam
            var produtosFaltantes = new Dictionary<int, int>();
            var produtosQuantidadeTotal = new Dictionary<int, int>();

            foreach (var grupo in produtosEncomenda.GroupBy(p => p.IdProduto))
            {
                int idProduto = grupo.Key;
                int qtdSolicitada = grupo.First().Quantidade;
                produtosQuantidadeTotal[idProduto] = qtdSolicitada;

                int qtdEmStock = produtosDisponiveis.ContainsKey(idProduto) ? produtosDisponiveis[idProduto] : 0;
                int qtdFaltante = Math.Max(0, qtdSolicitada - qtdEmStock);

                if (qtdFaltante > 0)
                {
                    produtosFaltantes[idProduto] = qtdFaltante;
                }
            }

            if (!produtosFaltantes.Any())
            {
                return (true, "Todos os produtos estão disponíveis em stock.", new Dictionary<int, int>());
            }

            // Para cada produto que falta, calcula a quantidade máxima produzível
            var produtosProduziveis = new Dictionary<int, int>();
            foreach (var produto in produtosFaltantes)
            {
                int idProduto = produto.Key;
                int qtdNecessaria = produto.Value;

                var materiaisNecessarios = produtosEncomenda
                    .Where(p => (int)p.IdProduto == idProduto)
                    .Select(p => new { IdMaterial = (int)p.IdMaterial, QtdNecessaria = (int)p.QtdNecessaria })
                    .ToList();

                // Calcular o máximo produzível para este produto
                int maxProduzivel = int.MaxValue;
                foreach (var material in materiaisNecessarios)
                {
                    if (!materiaisDisponiveis.ContainsKey(material.IdMaterial))
                    {
                        return (false, $"Material {material.IdMaterial} não encontrado no stock.", null);
                    }

                    int materialDisponivel = materiaisDisponiveis[material.IdMaterial];
                    int maxPorMaterial = materialDisponivel / material.QtdNecessaria;
                    maxProduzivel = Math.Min(maxProduzivel, maxPorMaterial);
                }

                if (maxProduzivel < qtdNecessaria)
                {
                    return (false, $"Material insuficiente para produzir a quantidade necessária do produto {idProduto}.", null);
                }

                // Retorna a quantidade máxima que pode ser produzida
                produtosProduziveis[idProduto] = maxProduzivel;

                // Atualiza a quantidade de materiais disponíveis baseado na quantidade máxima
                foreach (var material in materiaisNecessarios)
                {
                    materiaisDisponiveis[material.IdMaterial] -= material.QtdNecessaria * maxProduzivel;
                }
            }

            return (true, "Produção possível com os materiais disponíveis.", produtosProduziveis);
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