using SQLite;

namespace SQLApp.Model
{
    [Table("ItemCompra")] 
    public class CItemCompra
    {
        [PrimaryKey, AutoIncrement]
        public int CodigoItem { get; set; }

        [Indexed]
        public int CodigoProduto { get; set; }

        [Indexed]
        public int CodigoCompra { get; set; }

        public int Quantidade { get; set; }

        public decimal ValorParcial { get; set; }

    }
}
