using CommunityToolkit.Maui.Views;
using controle_ja_mobile.ViewModels;

namespace controle_ja_mobile.Views.Popups;

public partial class ManageAccountsPopup : Popup
{
    public ManageAccountsPopup(AccountsViewModel vm)
    {
        // Seta TRUE ANTES de ler o XAML. Assim o MAUI ignora o CollectionView na abertura!
        vm.IsLoading = true;

        InitializeComponent();
        BindingContext = vm;
        vm.PopupInstance = this;

        this.Opened += async (s, e) =>
        {
            // O Falso Delay: O Popup já abriu lisinho. Deixa o esqueleto piscar por 600ms
            await Task.Delay(600);

            // Agora sim, busca os dados. Ao terminar, IsLoading vira false e a lista renderiza
            await vm.LoadAccountsCommand.ExecuteAsync(null);
        };
    }
}