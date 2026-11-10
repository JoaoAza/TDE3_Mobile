using SQLApp.Model;
using SQLApp.ModelView;
using System.Collections.ObjectModel;

namespace SQLApp.View;

public partial class CompraPage : ContentPage
{
    private CCompraService _service = new CCompraService();
    private ObservableCollection<CCompra> _lista = new ObservableCollection<CCompra>();
    private bool _ocupado;
    private List<CCliente> _clientes = new List<CCliente>();
    private CClienteService _clienteService = new CClienteService();

    public CompraPage()
    {
        InitializeComponent();
        dpDataCompra.Date = DateTime.Today;
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
        int? selecionado = (pkCliente.SelectedItem as CCliente)?.CodigoCliente;
        _clientes = await _clienteService.GetClientes();
        pkCliente.ItemsSource = _clientes;
        pkCliente.SelectedItem = _clientes.FirstOrDefault(c => c.CodigoCliente == selecionado);
        var lista = await _service.GetCompras();
        _lista.Clear();
        lista.ForEach(p => _lista.Add(p));
        if (int.TryParse(txID.Text, out int id))
        {
            var atual = lista.FirstOrDefault(c => c.CodigoCompra == id);
            if (atual != null) lbTotal.Text = "Valor total: " + atual.ValorTotal.ToString("C2", CValidacao.Cultura);
        }
    }

    private CCompra LerFormulario()
    {
        if (pkCliente.SelectedItem is not CCliente cliente)
            throw new InvalidOperationException("Selecione um cliente cadastrado.");
        if (dpDataCompra.Date is not DateTime data)
            throw new InvalidOperationException("Informe a data da compra.");
        return new CCompra { CodigoCliente = cliente.CodigoCliente, DataCompra = data.Date };
    }

    private void Preencher(CCompra p)
    {
        txID.Text = p.CodigoCompra.ToString();
        pkCliente.SelectedItem = _clientes.FirstOrDefault(c => c.CodigoCliente == p.CodigoCliente);
        dpDataCompra.Date = p.DataCompra;
        lbTotal.Text = "Valor total: " + p.ValorTotal.ToString("C2", CValidacao.Cultura);
    }

    private void LimparFormulario()
    {
        txID.Text = string.Empty;

        pkCliente.SelectedItem = null;
        dpDataCompra.Date = DateTime.Today;
        lbTotal.Text = "Valor total: R$ 0,00";
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
            var p = await _service.ConsultarID(id);
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
            await _service.InsertCompra(p);
            LimparFormulario();
            await CarregarLista();
            Preencher(p);
            await DisplayAlertAsync("Sucesso", "Registro incluído com sucesso.", "OK");
        });
    }

    private async void btAlterar_Clicked(object sender, EventArgs e)
    {
        await Executar(async () =>
        {
            int id = CValidacao.Codigo(txID.Text);
            var p = LerFormulario();
            p.CodigoCompra = id;
            await _service.UpdateCompra(p);
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
            bool confirmar = await DisplayAlertAsync("Confirmar", "Excluir a compra, todos os seus itens e devolver as quantidades ao estoque?", "Sim", "Não");
            if (!confirmar) return;
            await _service.DeleteCompra(id);
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
        if (e.CurrentSelection.FirstOrDefault() is CCompra p)
            Preencher(p);
    }

    private async void btItens_Clicked(object sender, EventArgs e)
    {
        await Executar(async () =>
        {
            int id = CValidacao.Codigo(txID.Text);
            if (await _service.ConsultarID(id) == null)
                throw new InvalidOperationException("Compra não encontrada. Inclua a compra primeiro.");
            await Navigation.PushAsync(new ItemCompraPage(id));
        });
    }
}
