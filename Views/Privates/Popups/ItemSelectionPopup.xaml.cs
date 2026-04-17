using CommunityToolkit.Maui.Views;
using controle_ja_mobile.ViewModels;

namespace controle_ja_mobile.Views.Popups;

public partial class ItemSelectionPopup : Popup
{
    public ItemSelectionPopup(ItemSelectionViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
        vm.PopupInstance = this; // Avisa o ViewModel que este é o Popup atual
    }
}