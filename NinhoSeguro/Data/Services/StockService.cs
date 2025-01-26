using LI4.Data.Models;  

namespace LI4.Data.Services
{
    public class StockService
    {
        private readonly SqlDataAccess _db;

        public StockService(SqlDataAccess db)
        {
            _db = db;
        }
        public async Task<List<Produto>> ListarProdutosEmStockAsync()
        {
            var sql = @"
                        SELECT Id, Nome, Descricao, Quantidade, Preco 
                        FROM Produto";

            var produtosData = await _db.LoadData<dynamic, object>(sql, new { });
            var produtos = produtosData.Select(p => new Produto
            {
                Id = p.Id,
                Nome = p.Nome,
                Descricao = p.Descricao,
                Quantidade = p.Quantidade,
                Preco = p.Preco
            }).ToList();

            return produtos;
        }

        public async Task<List<Material>> ListarMateriaisEmStockAsync()
        {
            var sql = @"SELECT * FROM Material";
            return await _db.LoadData<Material, dynamic>(sql, new { });
        }

        public async Task<string> AtualizarStockProdutoAsync(int produtoId, int novaQuantidade)
        {
            var sql = @"UPDATE Produto SET Quantidade = @Quantidade WHERE Id = @ProdId";
            await _db.SaveData(sql, new { Quantidade = novaQuantidade, ProdId = produtoId });
            return "Stock do produto atualizado com sucesso.";
        }

        public async Task<string> AtualizarStockMaterialAsync(int materialId, int novaQuantidade)
        {
            var sql = @"UPDATE Material SET Quantidade = @Quantidade WHERE Id = @MatId";
            await _db.SaveData(sql, new { Quantidade = novaQuantidade, MatId = materialId });
            return "Stock do material atualizado com sucesso.";
        }

        private async Task<Produto> GetProdutoPorIdAsync(int produtoId)
        {
            var sql = @"SELECT Id, Nome, Descricao, Quantidade, Preco FROM Produto WHERE Id = @ProdutoId";
            var produtoData = await _db.LoadData<dynamic, object>(sql, new { ProdutoId = produtoId });

            var produto = produtoData.Select(p => new Produto
            {
                Id = p.Id,
                Nome = p.Nome,
                Descricao = p.Descricao,
                Quantidade = p.Quantidade,
                Preco = p.Preco
            }).FirstOrDefault();

            return produto;
        }

        public async Task<string> AtualizarStockApósEnvioAsync(List<Encomenda_tem_Produto> produtosEncomendados)
        {
            foreach (var item in produtosEncomendados)
            {
                // Busca o produto correspondente ao IdProduto da encomenda
                var produto = await GetProdutoPorIdAsync(item.IdProduto);

                if (produto == null)
                {
                    return $"Produto com Id {item.IdProduto} não encontrado.";
                }

                int novaQuantidade = produto.Quantidade - item.Quantidade;
                if (novaQuantidade < 0)
                {
                    return $"Quantidade insuficiente no estoque para o produto {produto.Nome}.";
                }
                await AtualizarStockProdutoAsync(produto.Id, novaQuantidade);
            }
            return "Estoque atualizado com sucesso.";
        }
    }
}