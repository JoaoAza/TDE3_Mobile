using SQLApp.Model;

namespace SQLApp.ModelView
{
    public class CProdutoService
    {
        public async Task<int> InsertProduto(CProduto p)
        {
            var db = await CBancoService.GetConexao();
            return await db.InsertAsync(p);
        }

        public async Task<int> UpdateProduto(CProduto p)
        {
            var db = await CBancoService.GetConexao();
            return await db.UpdateAsync(p);
        }

        public async Task<int> DeleteProduto(int id)
        {
            var db = await CBancoService.GetConexao();
            int vinculos = await db.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM ItemCompra WHERE CodigoProduto = ?", id);
            if (vinculos > 0)
                throw new InvalidOperationException("Este produto possui itens de compra. Exclua os vínculos primeiro.");
            return await db.DeleteAsync<CProduto>(id);
        }

        public async Task<List<CProduto>> GetProdutos()
        {
            var db = await CBancoService.GetConexao();
            return await db.Table<CProduto>().ToListAsync();
        }

        public async Task<CProduto?> ConsultarID(int id)
        {
            var db = await CBancoService.GetConexao();
            return await db.FindAsync<CProduto>(id);
        }

        public async Task<List<CProduto>> ConsultarNome(string query)
        {
            var db = await CBancoService.GetConexao();
            return await db.QueryAsync<CProduto>(
                "SELECT * FROM Produto WHERE NomeProduto LIKE ? ORDER BY NomeProduto", "%" + query + "%");
        }
    }
}
