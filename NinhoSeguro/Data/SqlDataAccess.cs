using Dapper;
using Microsoft.Data.SqlClient;

namespace LI4.Data
{
    public class SqlDataAccess
    {
        private readonly IConfiguration _config;
        private readonly SqlConnection _connection;

        public SqlDataAccess(IConfiguration config)
        {
            _config = config;
            _connection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
        }

        public SqlConnection Connection => _connection;

        // Carregar dados com parâmetros
        public async Task<List<T>> LoadData<T, U>(string sql, U parameters)
        {
            var data = await _connection.QueryAsync<T>(sql, parameters);
            return data.ToList();
        }

        // Salvar dados (INSERT, UPDATE, DELETE)
        public Task SaveData<T>(string sql, T parameters)
        {
            return _connection.ExecuteAsync(sql, parameters);
        }

        // Executar múltiplas queries em uma transação
        public async Task ExecuteTransaction<T>(Dictionary<string, T> queries)
        {
            await _connection.OpenAsync();

            var transaction = _connection.BeginTransaction();
            try
            {
                foreach (var query in queries)
                {
                    await _connection.ExecuteAsync(query.Key, query.Value, transaction);
                }

                transaction.Commit();
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
            finally
            {
                await _connection.CloseAsync();
            }
        }
    }
}