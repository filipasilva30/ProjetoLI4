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

        public async Task<string> CriarEncomendaAsync(int clienteId, Dictionary<Produto, int> itens, bool pagamentoEfetuado)
        {
            if (!pagamentoEfetuado)
                return "O pagamento não foi efetuado. A encomenda não pode ser criada.";

            // Verificar se o cliente existe
            var cliente = await _db.LoadData<Utilizador, dynamic>("SELECT * FROM Utilizador WHERE Id = @ClienteId", new { ClienteId = clienteId });
            if (cliente == null || cliente.Count == 0)
            {
                return "Cliente não encontrado.";
            }

            // Calcular custo total
            decimal custoTotal = itens.Sum(item => item.Key.Quantidade * item.Value);

            // Inserir a encomenda no banco
            var sqlEncomenda = @"INSERT INTO Encomenda (Custo, Data, DataPrevEntrega, PagamentoEfetuado, Estado, IdCliente) 
                                 OUTPUT INSERTED.Numero 
                                 VALUES (@Custo, @Data, @DataPrevEntrega, @PagamentoEfetuado, @Estado, @IdCliente)";

            var parametrosEncomenda = new
            {
                Custo = custoTotal,
                Data = DateTime.Now,
                DataPrevEntrega = DateTime.Now.AddDays(5),
                PagamentoEfetuado = true,
                Estado = "Em espera",
                IdCliente = clienteId
            };

            var encomendaId = await _db.LoadData<int, dynamic>(sqlEncomenda, parametrosEncomenda);

            // Inserir os itens da encomenda
            var queries = new Dictionary<string, object>();

            foreach (var item in itens)
            {
                var sqlProduto = @"INSERT INTO Encomenda_tem_Produto (Quantidade, IdEncomenda, IdProduto) 
                                   VALUES (@Quantidade, @IdEncomenda, @IdProduto)";

                var parametrosProduto = new
                {
                    Quantidade = item.Value,
                    IdEncomenda = encomendaId,
                    IdProduto = item.Key.Id
                };

                queries[sqlProduto] = parametrosProduto;
            }

            await _db.ExecuteTransaction(queries);

            return "Encomenda criada com sucesso!";
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