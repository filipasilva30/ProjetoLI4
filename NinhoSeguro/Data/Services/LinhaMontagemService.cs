using LI4.Data.Models;
using LI4.Data;

public class LinhaMontagemService
{
    private readonly SqlDataAccess _db;

    public LinhaMontagemService(SqlDataAccess db)
    {
        _db = db;
    }

    // Consultar o progresso da linha de montagem
    public async Task<List<LinhaMontagem>> ConsultarLinhaMontagemAsync(int produtoId)
    {
        var sql = "SELECT * FROM LinhaMontagem WHERE ProdutoId = @ProdutoId";
        var parametros = new { ProdutoId = produtoId };

        var etapas = await _db.LoadData<LinhaMontagem, dynamic>(sql, parametros);
        return etapas;
    }
}