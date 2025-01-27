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
            return matData.FirstOrDefault(); 
        }

        /// <summary>
        /// Ao produzir casas, subtrai os materiais e adiciona ao stock o(s) produto(s).
        /// Retorna uma mensagem de erro ou "Stock atualizado com sucesso." se tudo correr bem.
        /// </summary>
        public async Task<string> AtualizarStockAposProducaoAsync(List<Encomenda_tem_Produto> produtosEncomendados)
        {
            foreach (var item in produtosEncomendados)
            {
                // 1) Obter quantos materiais são necessários para produzir 1 unidade do produto
                var materiaisNecessarios = ObterMateriaisNecessarios(item.IdProduto);

                // Se não houver definição (ex.: default case), podes retornar erro ou ignorar
                if (materiaisNecessarios.Count == 0)
                {
                    return $"Não há definição de materiais para o produto (ID={item.IdProduto}).";
                }

                // 2) Retirar cada material do stock
                foreach (var (idMaterial, qtdNecessariaPorUnidade) in materiaisNecessarios)
                {
                    int totalNecessario = qtdNecessariaPorUnidade * item.Quantidade;

                    var mat = await GetMaterialPorIdAsync(idMaterial);
                    if (mat == null)
                    {
                        return $"Material com ID {idMaterial} não encontrado.";
                    }

                    if (mat.Quantidade < totalNecessario)
                    {
                        return $"Estoque insuficiente do material '{mat.Nome}' (ID={mat.Id}).";
                    }

                    int novaQtdMaterial = mat.Quantidade - totalNecessario;
                    await AtualizarStockMaterialAsync(mat.Id, novaQtdMaterial);
                }

                // 3) Adicionar o produto (casa) ao stock de Produto
                var produto = await GetProdutoPorIdAsync(item.IdProduto);
                if (produto == null)
                {
                    return $"Produto com ID {item.IdProduto} não encontrado.";
                }

                int novaQuantidadeProduto = produto.Quantidade + item.Quantidade;
                await AtualizarStockProdutoAsync(produto.Id, novaQuantidadeProduto);
            }

            // Se tudo correu bem para todos os produtos
            return "Stock atualizado com sucesso.";
        }

        /// <summary>
        /// Exemplo “Bill of Materials” fixo. Ajusta consoante a tua BD real.
        /// </summary>
        private Dictionary<int, int> ObterMateriaisNecessarios(int idProduto)
        {
            // Exemplo: para Produto 1 precisas de 14 tábuas (ID=10), 5 cilindros (ID=11) e 3 tubos (ID=12)
            // Ajusta para os IDs/quantidades que realmente tens na tabela 'Material'.
            switch (idProduto)
            {
                case 1:
                    return new Dictionary<int, int>
                    {
                        { 10, 14 },
                        { 11, 5 },
                        { 12, 3 }
                    };
                case 2:
                    return new Dictionary<int, int>
                    {
                        { 10, 20 },
                        { 11, 4 }
                    };
                default:
                    return new Dictionary<int, int>();
            }
        }

        /// <summary>
        /// Ao enviar as casas, decrementa o stock de Produto. Retorna mensagem de sucesso ou erro.
        /// </summary>
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
