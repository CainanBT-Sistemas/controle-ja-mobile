using controle_ja_mobile.ViewModels;

namespace controle_ja_mobile.Views.Privates.Management;

public partial class ManageCategoriesPage : ContentPage
{
    public ManageCategoriesPage(CategoriesViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is CategoriesViewModel vm)
        {
            await vm.LoadCategoriesCommand.ExecuteAsync(null);
        }
    }
}