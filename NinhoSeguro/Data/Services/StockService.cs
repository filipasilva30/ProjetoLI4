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
            var sql = @"SELECT Id, Nome, Quantidade FROM Material";
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

        public async Task<Produto> GetProdutoPorIdAsync(int produtoId)
        {
            var sql = @"SELECT Id, Nome, Descricao, Quantidade, Preco 
                        FROM Produto 
                        WHERE Id = @ProdutoId";

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

        // Modelo Material tem: Id, Nome, Quantidade
        public async Task<Material> GetMaterialPorIdAsync(int materialId)
        {
            var sql = @"SELECT Id, Nome, Quantidade
                        FROM Material
                        WHERE Id = @MaterialId";

            var matData = await _db.LoadData<Material, dynamic>(sql, new { MaterialId = materialId });
            var material = matData.Select(m => new Material
            {
                Id = m.Id,
                Nome = m.Nome,
                Quantidade = m.Quantidade
            }).FirstOrDefault();
            return material; 
        }

        public async Task<List<Produto_tem_Material>> ListarMateriaisProdutoAsync(int produtoId)
        {
            var sql = @"
                SELECT ptm.IdProduto, ptm.IdMaterial, ptm.Quantidade AS QuantidadeNecessaria
                FROM Produto_tem_Material ptm
                INNER JOIN Material m ON m.Id = ptm.IdMaterial
                WHERE ptm.IdProduto = @ProdutoId";

            var produtosTemMateriaisData = await _db.LoadData<dynamic, dynamic>(sql, new { ProdutoId = produtoId });

            var listaProdutoTemMaterial = produtosTemMateriaisData.Select(ptm => new Produto_tem_Material
            {
                IdProduto = ptm.IdProduto,
                IdMaterial = ptm.IdMaterial,
                Quantidade = ptm.QuantidadeNecessaria
            }).ToList();

            return listaProdutoTemMaterial;
        }

        public async Task<string> AtualizarStockApósEnvioAsync(List<Encomenda_tem_Produto> produtosEncomendados)
        {
            foreach (var item in produtosEncomendados)
            {
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
            return "Stock atualizado com sucesso.";
        }
    }
}
