using controle_ja_mobile.ViewModels;

namespace controle_ja_mobile.Views.Privates.Management;

public partial class ManageVehiclesPage : ContentPage
{
    public ManageVehiclesPage(VehiclesViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is VehiclesViewModel vm)
        {
            await vm.LoadVehiclesCommand.ExecuteAsync(null);
        }
    }
}