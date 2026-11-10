using SQLApp.Model;
using SQLApp.ModelView;
using SQLApp.View;

int checks = 0;
void Check(bool value, string name) { if (!value) throw new Exception(name); checks++; Console.WriteLine("OK " + name); }
async Task Fails(Func<Task> act, string name) { try { await act(); } catch { Check(true, name); return; } throw new Exception("Não rejeitou: " + name); }
void Invalid(Action act, string name) { try { act(); } catch { Check(true,name); return; } throw new Exception("Não rejeitou: " + name); }
var clientes = new CClienteService(); var produtos = new CProdutoService();
var compras = new CCompraService(); var itens = new CItemCompraService();
await Task.WhenAll(clientes.GetClientes(), clientes.GetClientes());
var c = new CCliente {Nome="Ana",RG=1234567890,Telefone="11999998888"};
await clientes.InsertCliente(c);
Check((await clientes.ConsultarID(c.CodigoCliente))!.Nome=="Ana","Incluir e consultar cliente");
c.Nome="Ana Silva";await clientes.UpdateCliente(c);
Check((await clientes.ConsultarNome("Silva")).Count==1,"Alterar e buscar cliente");
Check((await clientes.ConsultarNome("' OR 1=1 --")).Count==0,"Busca parametrizada");
var p = new CProduto {NomeProduto="Caderno",Tipo="Papelaria",Preco=12.50m,QtdEstoque=10};
await produtos.InsertProduto(p);
var compra=new CCompra {CodigoCliente=c.CodigoCliente,DataCompra=DateTime.Today}; await compras.InsertCompra(compra);
var item=new CItemCompra {CodigoCompra=compra.CodigoCompra,CodigoProduto=p.CodigoProduto,Quantidade=2};await itens.InsertItemCompra(item);
Check((await compras.ConsultarID(compra.CodigoCompra))!.ValorTotal==25m,"Total da compra após inclusão");
Check((await produtos.ConsultarID(p.CodigoProduto))!.QtdEstoque==8,"Baixa no estoque");
item.Quantidade=3;await itens.UpdateItemCompra(item);
Check((await compras.ConsultarID(compra.CodigoCompra))!.ValorTotal==37.50m,"Alteração de quantidade recalcula total");
item.Quantidade=20;await Fails(()=>itens.UpdateItemCompra(item),"Rejeita estoque insuficiente");
Check((await produtos.ConsultarID(p.CodigoProduto))!.QtdEstoque==7 && (await itens.ConsultarID(item.CodigoItem,compra.CodigoCompra))!.Quantidade==3,"Rollback mantém estoque e item");
p=(await produtos.ConsultarID(p.CodigoProduto))!;p.Preco=15;await produtos.UpdateProduto(p);
item.Quantidade=4;await itens.UpdateItemCompra(item);
Check((await itens.ConsultarID(item.CodigoItem,compra.CodigoCompra))!.ValorParcial==50m,"Preserva preço histórico");
var p2=new CProduto {NomeProduto="Caneta",Tipo="Papelaria",Preco=3m,QtdEstoque=20};await produtos.InsertProduto(p2);
item.CodigoProduto=p2.CodigoProduto;item.Quantidade=2;await itens.UpdateItemCompra(item);
Check((await produtos.ConsultarID(p.CodigoProduto))!.QtdEstoque==10 && (await produtos.ConsultarID(p2.CodigoProduto))!.QtdEstoque==18,"Troca de produto ajusta dois estoques");
Check((await compras.ConsultarID(compra.CodigoCompra))!.ValorTotal==6m,"Troca usa preço do novo produto");
var item2=new CItemCompra {CodigoCompra=compra.CodigoCompra,CodigoProduto=p.CodigoProduto,Quantidade=1};await itens.InsertItemCompra(item2);
Check((await compras.ConsultarID(compra.CodigoCompra))!.ValorTotal==21m,"Soma de múltiplos itens");
await Fails(()=>clientes.DeleteCliente(c.CodigoCliente),"Bloqueia exclusão de cliente vinculado");
await Fails(()=>produtos.DeleteProduto(p.CodigoProduto),"Bloqueia exclusão de produto vinculado");
await Fails(()=>compras.InsertCompra(new CCompra {CodigoCliente=999,DataCompra=DateTime.Today}),"Bloqueia cliente inexistente");
await Fails(()=>itens.DeleteItemCompra(item.CodigoItem,999),"Bloqueia item de outra compra");
await itens.DeleteItemCompra(item.CodigoItem,compra.CodigoCompra);
Check((await produtos.ConsultarID(p2.CodigoProduto))!.QtdEstoque==20 && (await compras.ConsultarID(compra.CodigoCompra))!.ValorTotal==15m,"Exclusão de item devolve estoque e ajusta total");
compra.DataCompra=DateTime.Today.AddDays(-1);await compras.UpdateCompra(compra);
Check((await compras.ConsultarID(compra.CodigoCompra))!.DataCompra==compra.DataCompra && (await compras.ConsultarID(compra.CodigoCompra))!.ValorTotal==15m,"Alteração de data preserva total");
await compras.DeleteCompra(compra.CodigoCompra);
Check((await itens.GetItens(compra.CodigoCompra)).Count==0 && (await produtos.ConsultarID(p.CodigoProduto))!.QtdEstoque==10,"Excluir compra remove itens e devolve estoque");
Check(await clientes.DeleteCliente(c.CodigoCliente)==1,"Excluir cliente sem vínculo");
Check(await produtos.DeleteProduto(p.CodigoProduto)==1,"Excluir produto sem vínculo");
Check(CValidacao.Preco("12,50")==12.50m && CValidacao.Preco("12.50")==12.50m,"Preço aceita ponto e vírgula");
Check(CValidacao.RG("9999999999")==9999999999L,"RG de 10 dígitos");
Invalid(()=>CValidacao.Preco("0"),"Preço zero");Invalid(()=>CValidacao.Preco("1,234"),"Preço com três decimais");
Invalid(()=>CValidacao.Preco("-1"),"Preço negativo");Invalid(()=>CValidacao.Estoque("-1"),"Estoque negativo");
Invalid(()=>CValidacao.Codigo("0"),"Código/quantidade zero");Invalid(()=>CValidacao.RG("abc"),"RG inválido");
Invalid(()=>CValidacao.Telefone("123"),"Telefone curto");Invalid(()=>CValidacao.Texto("  ","Nome",50),"Nome vazio");
Invalid(()=>CValidacao.Texto(new string('a',51),"Nome",50),"Nome longo");
Console.WriteLine($"PASSOU: {checks} verificações com os serviços C# reais e SQLite.");

public static class FileSystem
{
    public static string AppDataDirectory { get; } = Path.Combine(Path.GetTempPath(), "sqlapp-tests-" + Guid.NewGuid());
    static FileSystem() { Directory.CreateDirectory(AppDataDirectory); }
}
