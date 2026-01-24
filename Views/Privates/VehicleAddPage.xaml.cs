using controle_ja_mobile.ViewModels;

namespace controle_ja_mobile.Views.Privates;

public partial class VehicleAddPage : ContentPage
{
    public VehicleAddPage(VehiclesViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}