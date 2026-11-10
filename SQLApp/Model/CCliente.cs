using SQLite;

namespace SQLApp.Model
{
    [Table("Cliente")] 
    public class CCliente
    {
        [PrimaryKey, AutoIncrement]
        public int CodigoCliente { get; set; }

        [MaxLength(50)]
        public string Nome { get; set; } = string.Empty;

        public long RG { get; set; }

        [MaxLength(14)]
        public string Telefone { get; set; } = string.Empty;

        [Ignore]
        public string Descricao => $"{CodigoCliente} - {Nome}";
    }
}
