using SQLApp.Model;

namespace SQLApp.ModelView
{
    public class CCompraService
    {
        public async Task InsertCompra(CCompra p)
        {
            var db = await CBancoService.GetConexao();
            await db.RunInTransactionAsync(conexao =>
            {
                if (conexao.Find<CCliente>(p.CodigoCliente) == null)
                    throw new InvalidOperationException("Cliente não encontrado.");
                p.ValorTotal = 0;
                conexao.Insert(p);
            });
        }

        public async Task UpdateCompra(CCompra p)
        {
            var db = await CBancoService.GetConexao();
            await db.RunInTransactionAsync(conexao =>
            {
                var atual = conexao.Find<CCompra>(p.CodigoCompra)
                    ?? throw new InvalidOperationException("Compra não encontrada.");
                if (conexao.Find<CCliente>(p.CodigoCliente) == null)
                    throw new InvalidOperationException("Cliente não encontrado.");
                atual.CodigoCliente = p.CodigoCliente;
                atual.DataCompra = p.DataCompra;
                conexao.Update(atual);
            });
        }

        public async Task DeleteCompra(int id)
        {
            var db = await CBancoService.GetConexao();
            await db.RunInTransactionAsync(conexao =>
            {
                if (conexao.Find<CCompra>(id) == null)
                    throw new InvalidOperationException("Compra não encontrada.");
                var itens = conexao.Table<CItemCompra>().Where(i => i.CodigoCompra == id).ToList();
                foreach (var item in itens)
                {
                    CItemCompraService.DevolverEstoque(conexao, item);
                    conexao.Delete(item);
                }
                conexao.Delete<CCompra>(id);
            });
        }

        public async Task<List<CCompra>> GetCompras()
        {
            var db = await CBancoService.GetConexao();
            return await db.Table<CCompra>().OrderByDescending(c => c.CodigoCompra).ToListAsync();
        }

        public async Task<CCompra?> ConsultarID(int id)
        {
            var db = await CBancoService.GetConexao();
            return await db.FindAsync<CCompra>(id);
        }
    }
}
