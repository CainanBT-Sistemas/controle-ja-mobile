using controle_ja_mobile.ViewModels;

namespace controle_ja_mobile.Views.Privates.Management;

public partial class VehicleAddPage : ContentPage
{
    public VehicleAddPage(VehicleAddViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}