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

            // Inserir a encomenda no banco
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
                Custo = custoTotal,
                Data = DateTime.Now,
                DataPrevEntrega = DateTime.Now.AddDays(5),
                PagamentoEfetuado = false,
                Estado = "Em espera",
                IdCliente = clienteId
            };

            var queries = new Dictionary<string, object>();
            foreach (var item in produtosEncomendados)
            {
                var sqlProduto = @"INSERT INTO Encomenda_tem_Produto (Quantidade, IdEncomenda, IdProduto) 
                           VALUES (@Quantidade, @IdEncomenda, @IdProduto)";

                var parametrosProduto = new
                {
                    Quantidade = item.quantidade,
                    IdEncomenda = encomendaId,
                    IdProduto = item.produto.Id
                };

                queries[sqlProduto] = parametrosProduto;
            }
            await _db.ExecuteTransaction(queries);
            return encomenda;
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

        public async Task<string> ConsultarEstadoEncomendaAsync(int encomendaId)
        {
            var encomenda = await _db.LoadData<Encomenda, dynamic>("SELECT Estado FROM Encomenda WHERE Numero = @IdEncomenda",
                                                                   new { IdEncomenda = encomendaId });

            if (encomenda == null || encomenda.Count == 0)
            {
                return "Encomenda não encontrada.";
            }

            return encomenda[0].Estado;
        }

        public async Task<string> AtualizarEstadoEncomendaAsync(int encomendaId, string novoEstado)
        {
            var encomenda = await _db.LoadData<Encomenda, dynamic>("SELECT * FROM Encomenda WHERE Numero = @IdEncomenda",
                                                                   new { IdEncomenda = encomendaId });

            if (encomenda == null || encomenda.Count == 0)
            {
                return "Encomenda não encontrada.";
            }

            var sql = "UPDATE Encomenda SET Estado = @Estado WHERE Numero = @IdEncomenda";
            var parametros = new { Estado = novoEstado, IdEncomenda = encomendaId };

            await _db.SaveData(sql, parametros);

            return "Estado da encomenda atualizado com sucesso!";
        }
    }
}