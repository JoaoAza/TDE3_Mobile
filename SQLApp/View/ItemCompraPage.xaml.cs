using SQLApp.Model;
using SQLApp.ModelView;
using System.Collections.ObjectModel;

namespace SQLApp.View;

public partial class ItemCompraPage : ContentPage
{
    private CItemCompraService _service = new CItemCompraService();
    private ObservableCollection<CItemCompra> _lista = new ObservableCollection<CItemCompra>();
    private bool _ocupado;
    private readonly int _codigoCompra;
    private List<CProduto> _produtos = new List<CProduto>();
    private CProdutoService _produtoService = new CProdutoService();

    public ItemCompraPage(int codigoCompra)
    {
        InitializeComponent();
        _codigoCompra = codigoCompra;
        lbCompra.Text = "Compra nº " + codigoCompra;
        lvLista.ItemsSource = _lista;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await Executar(CarregarLista);
    }

    // Centraliza tratamento de erros e impede duas gravações simultâneas pela tela.
    private async Task Executar(Func<Task> acao)
    {
        if (_ocupado) return;
        _ocupado = true;
        IsEnabled = false;
        try
        {
            await acao();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Erro", ex.Message, "OK");
        }
        finally
        {
            IsEnabled = true;
            _ocupado = false;
        }
    }

    private async Task CarregarLista()
    {
        int? selecionado = (pkProduto.SelectedItem as CProduto)?.CodigoProduto;
        _produtos = await _produtoService.GetProdutos();
        pkProduto.ItemsSource = _produtos;
        pkProduto.SelectedItem = _produtos.FirstOrDefault(c => c.CodigoProduto == selecionado);
        var lista = await _service.GetItens(_codigoCompra);
        _lista.Clear();
        lista.ForEach(p => _lista.Add(p));

    }

    private CItemCompra LerFormulario()
    {
        if (pkProduto.SelectedItem is not CProduto produto)
            throw new InvalidOperationException("Selecione um produto cadastrado.");
        return new CItemCompra
        {
            CodigoCompra = _codigoCompra,
            CodigoProduto = produto.CodigoProduto,
            Quantidade = CValidacao.Codigo(txQuantidade.Text, "Quantidade")
        };
    }

    private void Preencher(CItemCompra p)
    {
        txID.Text = p.CodigoItem.ToString();
        pkProduto.SelectedItem = _produtos.FirstOrDefault(c => c.CodigoProduto == p.CodigoProduto);
        txQuantidade.Text = p.Quantidade.ToString();
        lbParcial.Text = "Valor parcial: " + p.ValorParcial.ToString("C2", CValidacao.Cultura);
    }

    private void LimparFormulario()
    {
        txID.Text = string.Empty;
        txQuantidade.Text = string.Empty;
        pkProduto.SelectedItem = null;
        lbParcial.Text = "Valor parcial: calculado ao salvar";
        lvLista.SelectedItem = null;
    }

    private void btLimpar_Clicked(object sender, EventArgs e)
    {
        LimparFormulario();
    }

    private async void btConsultar_Clicked(object sender, EventArgs e)
    {
        await Executar(async () =>
        {
            int id = CValidacao.Codigo(txID.Text);
            var p = await _service.ConsultarID(id, _codigoCompra);
            if (p == null) throw new InvalidOperationException("Registro não encontrado.");
            Preencher(p);
        });
    }

    private async void btIncluir_Clicked(object sender, EventArgs e)
    {
        await Executar(async () =>
        {
            if (!string.IsNullOrWhiteSpace(txID.Text))
                throw new InvalidOperationException("Limpe o formulário para incluir um novo registro. Para editar, use Alterar.");
            var p = LerFormulario();
            await _service.InsertItemCompra(p);
            LimparFormulario();
            await CarregarLista();
            await DisplayAlertAsync("Sucesso", "Registro incluído com sucesso.", "OK");
        });
    }

    private async void btAlterar_Clicked(object sender, EventArgs e)
    {
        await Executar(async () =>
        {
            int id = CValidacao.Codigo(txID.Text);
            var p = LerFormulario();
            p.CodigoItem = id;
            await _service.UpdateItemCompra(p);
            LimparFormulario();
            await CarregarLista();
            await DisplayAlertAsync("Sucesso", "Registro alterado com sucesso.", "OK");
        });
    }

    private async void btExcluir_Clicked(object sender, EventArgs e)
    {
        await Executar(async () =>
        {
            int id = CValidacao.Codigo(txID.Text);
            bool confirmar = await DisplayAlertAsync("Confirmar", "Deseja excluir este registro?", "Sim", "Não");
            if (!confirmar) return;
            await _service.DeleteItemCompra(id, _codigoCompra);
            LimparFormulario();
            await CarregarLista();
            await DisplayAlertAsync("Sucesso", "Registro excluído com sucesso.", "OK");
        });
    }

    private async void btAtualizar_Clicked(object sender, EventArgs e)
    {
        await Executar(CarregarLista);
    }

    private void lvLista_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is CItemCompra p)
            Preencher(p);
    }

}
