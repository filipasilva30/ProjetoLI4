using LI4.Data.Models;

namespace LI4.Data.Services
{
    public class RegisterService
    {
        private readonly SqlDataAccess _db;

        public RegisterService(SqlDataAccess db)
        {
            _db = db;
        }

        public async Task<string> RegistarClienteAsync(RegisterModel modelo)
        {
            if (string.IsNullOrWhiteSpace(modelo.FullName) ||
                string.IsNullOrWhiteSpace(modelo.Username) ||
                string.IsNullOrWhiteSpace(modelo.Email) ||
                string.IsNullOrWhiteSpace(modelo.Password) ||
                string.IsNullOrWhiteSpace(modelo.Phone) ||
                string.IsNullOrWhiteSpace(modelo.NIF))
            {
                return "Dados inválidos ou incompletos.";
            }
            // Verificar se o e-mail ou o username já estão registados
            var clienteExistente = await _db.LoadData<Utilizador, dynamic>("SELECT * FROM Utilizador WHERE Email = @Email OR Username = @Username",
                                                                           new { Email = modelo.Email, Username = modelo.Username });

            if (clienteExistente != null && clienteExistente.Count > 0)
            {
                if (clienteExistente.Any(u => u.Email == modelo.Email))
                    return "Já existe uma conta associada ao email.";
                if (clienteExistente.Any(u => u.Username == modelo.Username))
                    return "O username inserido já está a ser utilizado.";
            }

            if (!modelo.Username.StartsWith("cl"))
            {
                return "O username deve começar com 'cl'.";
            }

            // Preparar a inserção do novo cliente
            var sqlInsert = @"INSERT INTO Utilizador (Nome, Email, Tipo, Username, Senha, ContactoTel, NIF) 
                              VALUES (@Nome, @Email,@Tipo, @Username, @Senha, @ContactoTel, @NIF)";

            var parametros = new
            {
                Nome = modelo.FullName,
                Email = modelo.Email,
                Tipo = 1, // Cliente
                Username = modelo.Username,
                Senha = modelo.Password, 
                ContactoTel = modelo.Phone,
                NIF = modelo.NIF
            };

            try
            {
                await _db.SaveData(sqlInsert, parametros);
                return "Cliente registado com sucesso!";
            }
            catch (Exception)
            {
                return "Erro ao registar cliente.";
            }
        }
    }
}