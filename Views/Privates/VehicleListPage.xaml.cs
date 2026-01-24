using controle_ja_mobile.ViewModels;

namespace controle_ja_mobile.Views.Privates;

public partial class VehicleListPage : ContentPage
{
    private readonly VehiclesViewModel _vm;

    public VehicleListPage(VehiclesViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = _vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadVehicles();
    }
}