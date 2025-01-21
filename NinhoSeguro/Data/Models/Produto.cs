﻿namespace LI4.Data.Models
{
    public class Produto
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public int Quantidade { get; set; }
        public LinhaMontagem Montagem { get; set; }
        public List<Produto_tem_Material> Materiais { get; set; }
    }
}