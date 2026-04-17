using CommunityToolkit.Maui.Views;
using controle_ja_mobile.ViewModels;

namespace controle_ja_mobile.Views.Popups;

public partial class AccountAddPopup : Popup
{
    public AccountAddPopup(AccountAddViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
        vm.PopupInstance = this;
    }
}