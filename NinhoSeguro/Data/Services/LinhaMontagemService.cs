using LI4.Data.Models;
using LI4.Data;

namespace LI4.Data.Services
{
    public class LinhaMontagemService
    {
        private readonly SqlDataAccess _db;

        public LinhaMontagemService(SqlDataAccess db)
        {
            _db = db;
        }
        public async Task<List<LinhaMontagem>> ConsultarLinhaMontagemAsync(int produtoId)
        {
            var sql = @"
                WITH Etapas AS (
                    SELECT m.*
                    FROM Montagem m
                    INNER JOIN Produto p ON p.Montagem = m.IdEtapa
                    WHERE p.Id = @ProdutoId
                    UNION ALL
                    SELECT m.*
                    FROM Montagem m
                    INNER JOIN Etapas e ON m.IdEtapa = e.PassoSeguinte
                )
                SELECT * FROM Etapas";

            var parametros = new { ProdutoId = produtoId };

            var etapas = await _db.LoadData<LinhaMontagem, dynamic>(sql, parametros);
            return etapas;
        }

        public async Task<LinhaMontagem> ObterEtapaInicialAsync(int produtoId)
        {
            var sql = @"
                SELECT m.*
                FROM Montagem m
                INNER JOIN Produto p ON p.Montagem = m.IdEtapa
                WHERE p.Id = @ProdutoId";

            var parametros = new { ProdutoId = produtoId };

            var etapas = await _db.LoadData<LinhaMontagem, dynamic>(sql, parametros);
            return etapas.FirstOrDefault(); // Retorna apenas a etapa inicial
        }

        public async Task<string> VerificarProgressoAsync(int produtoId, int etapaAtualId)
        {
            var sql = @"
                SELECT Descricao
                FROM Montagem
                WHERE IdEtapa = @EtapaAtualId";

            var parametros = new { EtapaAtualId = etapaAtualId };

            var etapas = await _db.LoadData<LinhaMontagem, dynamic>(sql, parametros);
            var etapaAtual = etapas.FirstOrDefault();

            if (etapaAtual != null)
            {
                return $"Etapa atual: {etapaAtual.Descricao}";
            }
            return "Nenhuma etapa em progresso.";
        }
    }
}
