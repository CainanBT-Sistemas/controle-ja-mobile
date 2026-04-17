using CommunityToolkit.Maui.Views;
using controle_ja_mobile.ViewModels;

namespace controle_ja_mobile.Views.Popups;

public partial class CreditCardAddPopup : Popup
{
    public CreditCardAddPopup(CreditCardAddViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
        vm.PopupInstance = this;
    }
}