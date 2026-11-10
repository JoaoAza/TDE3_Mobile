using SQLApp.Model;

namespace SQLApp.ModelView
{
    public class CClienteService
    {
        public async Task<int> InsertCliente(CCliente p)
        {
            var db = await CBancoService.GetConexao();
            return await db.InsertAsync(p);
        }

        public async Task<int> UpdateCliente(CCliente p)
        {
            var db = await CBancoService.GetConexao();
            return await db.UpdateAsync(p);
        }

        public async Task<int> DeleteCliente(int id)
        {
            var db = await CBancoService.GetConexao();
            int vinculos = await db.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM Compra WHERE CodigoCliente = ?", id);
            if (vinculos > 0)
                throw new InvalidOperationException("Este cliente possui compras. Exclua os vínculos primeiro.");
            return await db.DeleteAsync<CCliente>(id);
        }

        public async Task<List<CCliente>> GetClientes()
        {
            var db = await CBancoService.GetConexao();
            return await db.Table<CCliente>().ToListAsync();
        }

        public async Task<CCliente?> ConsultarID(int id)
        {
            var db = await CBancoService.GetConexao();
            return await db.FindAsync<CCliente>(id);
        }

        public async Task<List<CCliente>> ConsultarNome(string query)
        {
            var db = await CBancoService.GetConexao();
            return await db.QueryAsync<CCliente>(
                "SELECT * FROM Cliente WHERE Nome LIKE ? ORDER BY Nome", "%" + query + "%");
        }
    }
}
