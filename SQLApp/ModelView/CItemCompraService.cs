using SQLite;
using SQLApp.Model;

namespace SQLApp.ModelView
{
    public class CItemCompraService
    {
        public async Task InsertItemCompra(CItemCompra item)
        {
            var db = await CBancoService.GetConexao();
            await db.RunInTransactionAsync(conexao =>
            {
                BaixarEstoque(conexao, item, null);
                conexao.Insert(item);
                AtualizarTotal(conexao, item.CodigoCompra);
            });
        }

        public async Task UpdateItemCompra(CItemCompra item)
        {
            var db = await CBancoService.GetConexao();
            await db.RunInTransactionAsync(conexao =>
            {
                var anterior = conexao.Find<CItemCompra>(item.CodigoItem)
                    ?? throw new InvalidOperationException("Item não encontrado.");
                if (anterior.CodigoCompra != item.CodigoCompra)
                    throw new InvalidOperationException("O item pertence a outra compra.");
                DevolverEstoque(conexao, anterior);
                BaixarEstoque(conexao, item, anterior);
                conexao.Update(item);
                AtualizarTotal(conexao, item.CodigoCompra);
            });
        }

        public async Task DeleteItemCompra(int id, int codigoCompra)
        {
            var db = await CBancoService.GetConexao();
            await db.RunInTransactionAsync(conexao =>
            {
                var item = conexao.Find<CItemCompra>(id)
                    ?? throw new InvalidOperationException("Item não encontrado.");
                if (item.CodigoCompra != codigoCompra)
                    throw new InvalidOperationException("O item pertence a outra compra.");
                DevolverEstoque(conexao, item);
                conexao.Delete(item);
                AtualizarTotal(conexao, codigoCompra);
            });
        }

        private static void BaixarEstoque(SQLiteConnection db, CItemCompra item, CItemCompra? anterior)
        {
            if (item.Quantidade <= 0)
                throw new InvalidOperationException("Quantidade deve ser maior que zero.");
            if (db.Find<CCompra>(item.CodigoCompra) == null)
                throw new InvalidOperationException("Compra não encontrada.");
            var produto = db.Find<CProduto>(item.CodigoProduto)
                ?? throw new InvalidOperationException("Produto não encontrado.");
            if (produto.QtdEstoque < item.Quantidade)
                throw new InvalidOperationException("Estoque insuficiente para esta quantidade.");

            decimal preco = anterior != null && anterior.CodigoProduto == item.CodigoProduto
                ? anterior.ValorParcial / anterior.Quantidade : produto.Preco;
            item.ValorParcial = decimal.Round(preco * item.Quantidade, 2, MidpointRounding.AwayFromZero);
            if (item.ValorParcial <= 0 || item.ValorParcial > 999999999.99m)
                throw new InvalidOperationException("Valor parcial fora do limite permitido.");
            produto.QtdEstoque -= item.Quantidade;
            db.Update(produto);
        }

        internal static void DevolverEstoque(SQLiteConnection db, CItemCompra item)
        {
            var produto = db.Find<CProduto>(item.CodigoProduto)
                ?? throw new InvalidOperationException("Produto não encontrado.");
            produto.QtdEstoque = checked(produto.QtdEstoque + item.Quantidade);
            db.Update(produto);
        }

        private static void AtualizarTotal(SQLiteConnection db, int codigoCompra)
        {
            var compra = db.Find<CCompra>(codigoCompra)
                ?? throw new InvalidOperationException("Compra não encontrada.");
            compra.ValorTotal = db.Table<CItemCompra>().Where(i => i.CodigoCompra == codigoCompra)
                .ToList().Sum(i => i.ValorParcial);
            if (compra.ValorTotal > 999999999.99m)
                throw new InvalidOperationException("Valor total acima do limite permitido.");
            db.Update(compra);
        }

        public async Task<List<CItemCompra>> GetItens(int codigoCompra)
        {
            var db = await CBancoService.GetConexao();
            return await db.Table<CItemCompra>().Where(i => i.CodigoCompra == codigoCompra).ToListAsync();
        }

        public async Task<CItemCompra?> ConsultarID(int id, int codigoCompra)
        {
            var db = await CBancoService.GetConexao();
            return await db.Table<CItemCompra>()
                .Where(i => i.CodigoItem == id && i.CodigoCompra == codigoCompra).FirstOrDefaultAsync();
        }
    }
}
