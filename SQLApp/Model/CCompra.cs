using SQLite;

namespace SQLApp.Model
{
    [Table("Compra")] 
    public class CCompra
    {
        [PrimaryKey, AutoIncrement]
        public int CodigoCompra { get; set; }

        public DateTime DataCompra { get; set; }

        public decimal ValorTotal { get; set; }

        [Indexed]
        public int CodigoCliente { get; set; }

    }
}
