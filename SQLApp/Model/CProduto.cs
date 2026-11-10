using SQLite;

namespace SQLApp.Model
{
    [Table("Produto")] 
    public class CProduto
    {
        [PrimaryKey, AutoIncrement]
        public int CodigoProduto { get; set; }

        [MaxLength(50)]
        public string NomeProduto { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Tipo { get; set; } = string.Empty;

        public decimal Preco { get; set; }

        public int QtdEstoque { get; set; }

        [Ignore]
        public string Descricao => $"{CodigoProduto} - {NomeProduto}";
    }
}
