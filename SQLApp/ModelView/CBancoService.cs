using SQLite;

namespace SQLApp.ModelView
{
    public static class CBancoService
    {
        // Uma única inicialização, aguardada antes de qualquer consulta.
        private static readonly Lazy<Task<SQLiteAsyncConnection>> _conexao =
            new Lazy<Task<SQLiteAsyncConnection>>(Init);

        public static Task<SQLiteAsyncConnection> GetConexao() => _conexao.Value;

        private static async Task<SQLiteAsyncConnection> Init()
        {
            const string DatabaseFilename = "compras.db3";
            const SQLiteOpenFlags Flags = SQLiteOpenFlags.ReadWrite |
                SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache;
            string DatabasePath = Path.Combine(FileSystem.AppDataDirectory, DatabaseFilename);
            var db = new SQLiteAsyncConnection(DatabasePath, Flags);

           
            await db.ExecuteAsync("PRAGMA foreign_keys = ON");
            await db.ExecuteAsync(@"CREATE TABLE IF NOT EXISTS Cliente (
                CodigoCliente INTEGER PRIMARY KEY AUTOINCREMENT,
                Nome TEXT NOT NULL CHECK(length(trim(Nome)) BETWEEN 1 AND 50),
                RG INTEGER NOT NULL CHECK(RG BETWEEN 1 AND 9999999999),
                Telefone TEXT NOT NULL CHECK(length(Telefone) BETWEEN 8 AND 14))");
            await db.ExecuteAsync(@"CREATE TABLE IF NOT EXISTS Produto (
                CodigoProduto INTEGER PRIMARY KEY AUTOINCREMENT,
                NomeProduto TEXT NOT NULL CHECK(length(trim(NomeProduto)) BETWEEN 1 AND 50),
                Tipo TEXT NOT NULL CHECK(length(trim(Tipo)) BETWEEN 1 AND 50),
                Preco NUMERIC NOT NULL CHECK(Preco > 0 AND Preco <= 999999999.99),
                QtdEstoque INTEGER NOT NULL CHECK(QtdEstoque >= 0))");
            await db.ExecuteAsync(@"CREATE TABLE IF NOT EXISTS Compra (
                CodigoCompra INTEGER PRIMARY KEY AUTOINCREMENT,
                DataCompra BIGINT NOT NULL,
                ValorTotal NUMERIC NOT NULL DEFAULT 0 CHECK(ValorTotal >= 0),
                CodigoCliente INTEGER NOT NULL,
                FOREIGN KEY(CodigoCliente) REFERENCES Cliente(CodigoCliente) ON DELETE RESTRICT)");
            await db.ExecuteAsync(@"CREATE TABLE IF NOT EXISTS ItemCompra (
                CodigoItem INTEGER PRIMARY KEY AUTOINCREMENT,
                CodigoProduto INTEGER NOT NULL,
                CodigoCompra INTEGER NOT NULL,
                Quantidade INTEGER NOT NULL CHECK(Quantidade > 0),
                ValorParcial NUMERIC NOT NULL CHECK(ValorParcial > 0),
                FOREIGN KEY(CodigoProduto) REFERENCES Produto(CodigoProduto) ON DELETE RESTRICT,
                FOREIGN KEY(CodigoCompra) REFERENCES Compra(CodigoCompra) ON DELETE RESTRICT)");
            await db.ExecuteAsync("CREATE INDEX IF NOT EXISTS IX_Compra_Cliente ON Compra(CodigoCliente)");
            await db.ExecuteAsync("CREATE INDEX IF NOT EXISTS IX_Item_Compra ON ItemCompra(CodigoCompra)");
            await db.ExecuteAsync("CREATE INDEX IF NOT EXISTS IX_Item_Produto ON ItemCompra(CodigoProduto)");
            return db;
        }
    }
}
