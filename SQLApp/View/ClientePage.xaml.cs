using SQLApp.Model;
using SQLApp.ModelView;
using System.Collections.ObjectModel;

namespace SQLApp.View;

public partial class ClientePage : ContentPage
{
    private CClienteService _service = new CClienteService();
    private ObservableCollection<CCliente> _lista = new ObservableCollection<CCliente>();
    private bool _ocupado;

    public ClientePage()
    {
        InitializeComponent();
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
        var lista = await _service.ConsultarNome(sbBusca.Text ?? string.Empty);
        lista = pkOrdem.SelectedIndex == 1
            ? lista.OrderByDescending(p => p.Nome).ToList()
            : lista.OrderBy(p => p.Nome).ToList();
        _lista.Clear();
        lista.ForEach(p => _lista.Add(p));

    }

    private CCliente LerFormulario()
    {
        return new CCliente
        {
            Nome = CValidacao.Texto(txNome.Text, "Nome", 50),
            RG = CValidacao.RG(txRG.Text),
            Telefone = CValidacao.Telefone(txTelefone.Text)
        };
    }

    private void Preencher(CCliente p)
    {
        txID.Text = p.CodigoCliente.ToString();
        txNome.Text = p.Nome;
        txRG.Text = p.RG.ToString();
        txTelefone.Text = p.Telefone;
    }

    private void LimparFormulario()
    {
        txID.Text = string.Empty;
        txNome.Text = string.Empty;
        txRG.Text = string.Empty;
        txTelefone.Text = string.Empty;
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
            await _service.InsertCliente(p);
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
            p.CodigoCliente = id;
            if (await _service.UpdateCliente(p) == 0)
                throw new InvalidOperationException("Registro não encontrado.");
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
            if (await _service.DeleteCliente(id) == 0)
                throw new InvalidOperationException("Registro não encontrado.");
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
        if (e.CurrentSelection.FirstOrDefault() is CCliente p)
            Preencher(p);
    }

    private async void sbBusca_SearchButtonPressed(object sender, EventArgs e)
    {
        await Executar(CarregarLista);
    }

    private async void pkOrdem_SelectedIndexChanged(object sender, EventArgs e)
    {
        await Executar(CarregarLista);
    }
}
