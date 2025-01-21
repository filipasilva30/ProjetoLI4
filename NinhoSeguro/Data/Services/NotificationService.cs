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

        public async Task<string> EnviarNotificacaoAsync(int clienteId, string mensagem)
        {
            var sql = @"INSERT INTO Notificacao (ClienteId, Mensagem, DataHora) 
                        VALUES (@ClienteId, @Mensagem, @DataHora)";
            await _db.SaveData(sql, new 
            {
                ClienteId = clienteId,
                Mensagem = mensagem,
                DataHora = DateTime.Now
            });
            return "Notificação enviada ao cliente.";
        }

        public async Task<List<Notificacao>> ConsultarNotificacoesAsync(int clienteId)
        {
            var sql = @"SELECT * FROM Notificacao WHERE ClienteId = @ClienteId ORDER BY DataHora DESC";
            return await _db.LoadData<Notificacao, dynamic>(sql, new { ClienteId = clienteId });
        }
    }

    public class Notificacao
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public string Mensagem { get; set; }
        public DateTime DataHora { get; set; }
    }
}