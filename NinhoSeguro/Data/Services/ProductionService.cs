using LI4.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LI4.Data.Services
{
    public class ProductionService
    {
        private readonly SqlDataAccess _db;

        public ProductionService(SqlDataAccess db)
        {
            _db = db;
        }

        public async Task<string> IniciarProducaoAsync(int encomendaId)
        {
            var sqlEnc = @"SELECT * FROM Encomenda WHERE Numero = @EncId AND PagamentoEfetuado = 1 AND Estado = 'Em espera'";
            var enc = await _db.LoadData<Encomenda, dynamic>(sqlEnc, new { EncId = encomendaId });
            if (enc == null || enc.Count == 0)
                return "Encomenda não disponível para produção.";

            var sqlUpdate = @"UPDATE Encomenda SET Estado = 'Em produção' WHERE Numero = @EncId";
            await _db.SaveData(sqlUpdate, new { EncId = encomendaId });
            return "Produção iniciada com sucesso.";
        }

        public async Task<string> EnviarEncomendaAsync(int encomendaId)
        {
            var sqlEnc = @"SELECT * FROM Encomenda WHERE Numero = @EncId AND Estado = 'Em produção'";
            var enc = await _db.LoadData<Encomenda, dynamic>(sqlEnc, new { EncId = encomendaId });
            if (enc == null || enc.Count == 0)
                return "Encomenda não está pronta para envio.";

            var sqlUpdate = @"UPDATE Encomenda SET Estado = 'Enviada' WHERE Numero = @EncId";
            await _db.SaveData(sqlUpdate, new { EncId = encomendaId });
            return "Encomenda enviada para o cliente.";
        }
    }
}