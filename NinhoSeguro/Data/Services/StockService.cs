using LI4.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

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
            var sql = @"SELECT * FROM Produto";
            return await _db.LoadData<Produto, dynamic>(sql, new { });
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
    }
}