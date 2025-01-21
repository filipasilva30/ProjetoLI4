namespace LI4.Data.Models
{
    public class LinhaMontagem
    {
        public int IdEtapa { get; set; }
        public string Descricao { get; set; }
        public TimeSpan Duracao { get; set; }
        public int PassoSeguinte { get; set; }
        public Produto Produto { get; set; }
    }
}
