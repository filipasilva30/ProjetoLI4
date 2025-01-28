namespace LI4.Data.Models
{
    public class Notificacao
    {
        public int Id { get; set; }
        public int NumEncomenda { get; set; }
        public int IdCliente { get; set; }
        public string Descricao { get; set; }
        public DateTime DataHora { get; set; }
    }
}
