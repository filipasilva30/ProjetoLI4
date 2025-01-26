using LI4.Data;
using LI4.Data.Models;

namespace LI4.Data.Services
{
    public class OrderService
    {

        private readonly SqlDataAccess _db;

        public OrderService(SqlDataAccess db)
        {
            _db = db;
        }

        public async Task<bool> EliminarEncomendaAsync(int encomendaId)
        {
            try
            {
                // Eliminar os produtos associados à encomenda
                var sqlDeleteProdutos = "DELETE FROM Encomenda_tem_Produto WHERE NumEncomenda = @NumEncomenda";
                await _db.SaveData(sqlDeleteProdutos, new { NumEncomenda = encomendaId });

                // Eliminar a encomenda
                var sqlDeleteEncomenda = "DELETE FROM Encomenda WHERE Numero = @NumEncomenda";
                await _db.SaveData(sqlDeleteEncomenda, new { NumEncomenda = encomendaId });

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<Utilizador> ObterClientePorIdAsync(int clienteId)
        {
            var sql = "SELECT Nome, NIF FROM Utilizador WHERE Id = @ClienteId";
            var parametros = new { ClienteId = clienteId };

            var clientes = await _db.LoadData<Utilizador, dynamic>(sql, parametros);
            return clientes.FirstOrDefault();
        }
        public async Task<Encomenda> CriarEncomendaAsync(int clienteId, List<(Produto produto, int quantidade)> produtosEncomendados)
        {

            // Verificar se o cliente existe
            var cliente = await _db.LoadData<Utilizador, dynamic>("SELECT * FROM Utilizador WHERE Id = @ClienteId", new { ClienteId = clienteId });
            if (cliente == null || cliente.Count == 0)
            {
                throw new Exception("Cliente não encontrado.");
            }

            // Calcular custo total
            decimal custoTotal = produtosEncomendados.Sum(item => item.produto.Preco * item.quantidade);


            var sqlEncomenda = @"INSERT INTO Encomenda (Custo, Data, DataPrevEntrega, PagamentoEfetuado, Estado, IdCliente) 
                         OUTPUT INSERTED.Numero 
                         VALUES (@Custo, @Data, @DataPrevEntrega, @PagamentoEfetuado, @Estado, @IdCliente)";

            var parametrosEncomenda = new
            {
                Custo = custoTotal,
                Data = DateTime.Now,
                DataPrevEntrega = DateTime.Now.AddDays(7),
                PagamentoEfetuado = false,
                Estado = "Em espera",
                IdCliente = clienteId
            };

            var encomendaId = (await _db.LoadData<int, dynamic>(sqlEncomenda, parametrosEncomenda)).FirstOrDefault();

            if (encomendaId == 0)
            {
                throw new Exception("Erro ao criar encomenda.");
            }

            var encomenda = new Encomenda
            {
                Numero = encomendaId,
                Custo = custoTotal,
                Data = DateTime.Now,
                DataPrevEntrega = DateTime.Now.AddDays(7),
                PagamentoEfetuado = false,
                Estado = "Em espera",
                IdCliente = clienteId
            };

            // Inserir os produtos da encomenda um por um
            var sqlProduto = @"INSERT INTO Encomenda_tem_Produto (Quantidade, NumEncomenda, IdProduto) 
                       VALUES (@Quantidade, @NumEncomenda, @IdProduto)";

            foreach (var item in produtosEncomendados)
            {
                var parametrosProduto = new
                {
                    Quantidade = item.quantidade,
                    NumEncomenda = encomendaId,
                    IdProduto = item.produto.Id
                };

                await _db.SaveData(sqlProduto, parametrosProduto);
                Console.WriteLine($"Produto {item.produto.Nome} com quantidade {item.quantidade} foi inserido.");
            }


            return encomenda;

        }

        public async Task<int> GetQuantidadeEncomendadaAsync(int encomendaId, int produtoId)
        {
            var sql = @"
            SELECT Quantidade 
            FROM Encomenda_tem_Produto 
            WHERE NumEncomenda = @EncomendaId AND IdProduto = @ProdutoId";

            var quantidade = await _db.LoadData<int, dynamic>(
                sql,
                new { EncomendaId = encomendaId, ProdutoId = produtoId }
            );

            return quantidade.FirstOrDefault();
        }

        // lista de encomendas de um dado cliente
        public async Task<List<Encomenda>> ListarEncomendasClienteAsync(int clienteId)
        {
            var sql = "SELECT * FROM Encomenda WHERE IdCliente = @IdCliente";
            var parametros = new { IdCliente = clienteId };

            var encomendas = await _db.LoadData<Encomenda, dynamic>(sql, parametros);
            return encomendas;
        }

        // lista de todas as encomendas para o funcionário ver
        public async Task<List<Encomenda>> ListarTodasEncomendasAsync()
        {
            var sql = "SELECT * FROM Encomenda";
            var encomendas = await _db.LoadData<Encomenda, dynamic>(sql, new { });
            return encomendas;
        }

        public async Task<string> ObterNomeProdutoPorIdAsync(int produtoId)
        {
            var sql = "SELECT Nome FROM Produto WHERE Id = @ProdutoId";
            var parametros = new { ProdutoId = produtoId };

            var nomeProduto = await _db.LoadData<string, dynamic>(sql, parametros);

            return nomeProduto.FirstOrDefault();
        }

        public async Task<List<Produto>> ListarProdutosPorEncomendaAsync(int encomendaId)
        {
            var sql = @"
            SELECT p.Id, p.Nome, p.Descricao, ep.Quantidade, p.Preco
            FROM Encomenda_tem_Produto ep
            INNER JOIN Produto p ON ep.IdProduto = p.Id
            WHERE ep.NumEncomenda = @NumEncomenda";

            var parametros = new { NumEncomenda = encomendaId };

            var produtos = await _db.LoadData<Produto, dynamic>(sql, parametros);
            return produtos;
        }

        public async Task<string> ConsultarEstadoEncomendaAsync(int encomendaId)
        {
            var encomenda = await _db.LoadData<Encomenda, dynamic>("SELECT Estado FROM Encomenda WHERE Numero = @NumEncomenda",
                                                                   new { NumEncomenda = encomendaId });

            if (encomenda == null || encomenda.Count == 0)
            {
                return "Encomenda não encontrada.";
            }

            return encomenda[0].Estado;
        }

        public async Task<string> AtualizarEstadoEncomendaAsync(int encomendaId, string novoEstado)
        {
            var encomenda = await _db.LoadData<Encomenda, dynamic>("SELECT * FROM Encomenda WHERE Numero = @NumEncomenda",
                                                                   new { NumEncomenda = encomendaId });

            if (encomenda == null || encomenda.Count == 0)
            {
                return "Encomenda não encontrada.";
            }

            var sql = "UPDATE Encomenda SET Estado = @Estado WHERE Numero = @NumEncomenda";
            var parametros = new { Estado = novoEstado, NumEncomenda = encomendaId };

            await _db.SaveData(sql, parametros);

            return "Estado da encomenda atualizado com sucesso!";
        }
        public async Task<bool> AtualizarDataEncomendaAsync(int encomendaId, DateTime novaData)
        {
            var sql = "UPDATE Encomenda SET DataPrevEntrega = @NovaData WHERE Numero = @NumEncomenda";
            var parametros = new { NovaData = novaData, NumEncomenda = encomendaId };

            try
            {
                await _db.SaveData(sql, parametros);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> AtualizarPagamentoEfetuadoAsync(int encomendaId)
        {
            var sql = "UPDATE Encomenda SET PagamentoEfetuado = 1 WHERE Numero = @NumEncomenda";
            var parametros = new { NumEncomenda = encomendaId };

            try
            {
                await _db.SaveData(sql, parametros);
                return true; // Se não houver exceções, retorna verdadeiro.
            }
            catch (Exception)
            {
                return false; // Em caso de erro, retorna falso.
            }
        }
    }
}
