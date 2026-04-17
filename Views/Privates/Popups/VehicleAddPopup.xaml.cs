using CommunityToolkit.Maui.Views;
using controle_ja_mobile.ViewModels;

namespace controle_ja_mobile.Views.Popups;

public partial class VehicleAddPopup : Popup
{
    public VehicleAddPopup(VehicleAddViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
        
        // Avisa o ViewModel que este é o Popup atual para ele conseguir fechar depois
        vm.PopupInstance = this;
    }
}