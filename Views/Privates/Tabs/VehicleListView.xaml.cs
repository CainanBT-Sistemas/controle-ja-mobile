using controle_ja_mobile.ViewModels;

namespace controle_ja_mobile.Views.Privates.Tabs;

public partial class VehicleListView : ContentView
{
    public VehicleListView()
    {
        InitializeComponent();
        this.Loaded += OnViewLoaded;
    }

    private async void OnViewLoaded(object sender, EventArgs e)
    {
        if (BindingContext is VehiclesViewModel vm)
        {
            vm.IsLoading = true;
            await Task.Delay(150);
            await vm.LoadVehiclesCommand.ExecuteAsync(null);
        }
    }
}