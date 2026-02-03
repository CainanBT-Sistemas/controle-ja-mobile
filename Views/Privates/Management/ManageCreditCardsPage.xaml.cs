using controle_ja_mobile.ViewModels;

namespace controle_ja_mobile.Views.Privates.Management;

public partial class ManageCreditCardsPage : ContentPage
{
    public ManageCreditCardsPage(CreditCardsViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        // Garante que recarrega a lista e gráficos ao entrar
        if (BindingContext is CreditCardsViewModel vm)
        {
            await vm.LoadCardsCommand.ExecuteAsync(null);
        }
    }
}