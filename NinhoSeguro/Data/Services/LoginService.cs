using LI4.Data.Models;

namespace LI4.Data.Services
{
    public class LoginService
    {
        private readonly SqlDataAccess _db;

        public LoginService(SqlDataAccess db)
        {
            _db = db;
        }

        public async Task<(bool Sucesso, string Mensagem, int? Tipo, int? ClienteId, Utilizador? Utilizador)> IniciarSessaoAsync(LoginModel modelo)
        {
            // Validações iniciais
            if (string.IsNullOrWhiteSpace(modelo.Username) ||
                string.IsNullOrWhiteSpace(modelo.Password))
            {
                return (false, "Credenciais inválidas", null, null, null);
            }

            // Verificar se o utilizador existe
            var sql = @"SELECT * FROM Utilizador WHERE Username = @Username";
            var resultado = await _db.LoadData<Utilizador, dynamic>(sql, new { Username = modelo.Username });

            if (resultado == null || resultado.Count == 0)
            {
                return (false, "Não existe conta com o username inserido", null, null, null);
            }

            var utilizador = resultado.FirstOrDefault();

            // Validar senha
            if (utilizador.Senha != modelo.Password)
            {
                return (false, "Senha inválida", null, null, null);
            }

            // Retornar o tipo do utilizador e os dados dele
            return (true, "Login efetuado com sucesso", utilizador.Tipo, utilizador.Id, utilizador);
        }
    }
}