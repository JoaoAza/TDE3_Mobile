namespace SQLApp.View;

public partial class MenuPage : ContentPage
{
    public MenuPage()
    {
        InitializeComponent();
    }

    private async void btClientes_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ClientePage());
    }

    private async void btProdutos_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ProdutoPage());
    }

    private async void btCompras_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new CompraPage());
    }
}
