namespace LI4.Auth
{
    public class Session
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public int Tipo { get; set; } // 1 - Cliente, 2 - Funcionário
    }
}