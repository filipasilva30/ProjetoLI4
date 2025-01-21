namespace LI4.Data.Models
{
    public class Encomenda
    {
        public int Numero { get; set; }
        public decimal Custo { get; set; }
        public DateTime Data { get; set; }
        public DateTime DataPrevEntrega { get; set; }
        public bool PagamentoEfetuado { get; set; }
        public string Estado { get; set; }
        public int IdCliente { get; set; } 
        public List<Encomenda_tem_Produto> ProdutosEncomendados { get; set; }
    }
}