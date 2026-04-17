using CommunityToolkit.Maui.Views;
using controle_ja_mobile.ViewModels;

namespace controle_ja_mobile.Views.Popups;

public partial class ManageCreditCardsPopup : Popup
{
    public ManageCreditCardsPopup(CreditCardsViewModel vm)
    {
        vm.IsLoading = true;
        InitializeComponent();
        BindingContext = vm;
        vm.PopupInstance = this;

        this.Opened += async (s, e) =>
        {
            await Task.Delay(600);
            await vm.LoadCardsCommand.ExecuteAsync(null);
        };
    }
}