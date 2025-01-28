using LI4.Data.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LI4.Data.Services
{
    public class NotificationService
    {
        private readonly SqlDataAccess _db;

        public NotificationService(SqlDataAccess db)
        {
            _db = db;
        }

        public async Task<string> EnviarNotificacaoAsync(int numEncomenda, int idCliente, string descricao)
        {
            var sql = @"INSERT INTO Notificacao (NumEncomenda, IdCliente, Descricao, DataHora) 
                        VALUES (@NumEncomenda, @IdCliente, @Descricao, @DataHora)";
            await _db.SaveData(sql, new 
            {
                NumEncomenda = numEncomenda,
                IdCliente = idCliente,
                Descricao = descricao,
                DataHora = DateTime.Now
            });
            return "Notificação enviada ao cliente.";
        }

        public async Task<List<Notificacao>> ConsultarNotificacoesAsync(int idCliente)
        {
            var sql = @"SELECT * FROM Notificacao WHERE IdCliente = @IdCliente ORDER BY DataHora DESC";
            return await _db.LoadData<Notificacao, dynamic>(sql, new { IdCliente = idCliente });
        }
    }
}
