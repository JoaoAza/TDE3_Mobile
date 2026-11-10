using SQLApp.Model;
using SQLApp.ModelView;
using System.Collections.ObjectModel;

namespace SQLApp.View;

public partial class ProdutoPage : ContentPage
{
    private CProdutoService _service = new CProdutoService();
    private ObservableCollection<CProduto> _lista = new ObservableCollection<CProduto>();
    private bool _ocupado;

    public ProdutoPage()
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
            ? lista.OrderByDescending(p => p.NomeProduto).ToList()
            : lista.OrderBy(p => p.NomeProduto).ToList();
        _lista.Clear();
        lista.ForEach(p => _lista.Add(p));

    }

    private CProduto LerFormulario()
    {
        return new CProduto
        {
            NomeProduto = CValidacao.Texto(txNomeProduto.Text, "Nome do produto", 50),
            Tipo = CValidacao.Texto(txTipo.Text, "Tipo", 50),
            Preco = CValidacao.Preco(txPreco.Text),
            QtdEstoque = CValidacao.Estoque(txQtdEstoque.Text)
        };
    }

    private void Preencher(CProduto p)
    {
        txID.Text = p.CodigoProduto.ToString();
        txNomeProduto.Text = p.NomeProduto;
        txTipo.Text = p.Tipo;
        txPreco.Text = p.Preco.ToString("F2", CValidacao.Cultura);
        txQtdEstoque.Text = p.QtdEstoque.ToString();
    }

    private void LimparFormulario()
    {
        txID.Text = string.Empty;
        txNomeProduto.Text = string.Empty;
        txTipo.Text = string.Empty;
        txPreco.Text = string.Empty;
        txQtdEstoque.Text = string.Empty;
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
            await _service.InsertProduto(p);
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
            p.CodigoProduto = id;
            if (await _service.UpdateProduto(p) == 0)
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
            if (await _service.DeleteProduto(id) == 0)
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
        if (e.CurrentSelection.FirstOrDefault() is CProduto p)
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
