namespace LI4.Data.Models
{
    public class Utilizador
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public int Tipo { get; set; } // 1 - Cliente, 2 - Funcionário
        public string Username { get; set; }
        public string Senha { get; set; }
        public string ContactoTel { get; set; }
        public int NIF { get; set; }
    }
}