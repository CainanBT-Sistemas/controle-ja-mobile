using controle_ja_mobile.ViewModels;

namespace controle_ja_mobile.Views.Privates.Management;

public partial class ManageAccountsPage : ContentPage
{
    public ManageAccountsPage(AccountsViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        // Carrega as contas ao abrir a tela
        if (BindingContext is AccountsViewModel vm)
        {
            await vm.LoadAccountsCommand.ExecuteAsync(null);
        }
    }
}