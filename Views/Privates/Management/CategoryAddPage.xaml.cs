using controle_ja_mobile.ViewModels;

namespace controle_ja_mobile.Views.Privates.Management;

public partial class CategoryAddPage : ContentPage
{
    public CategoryAddPage(CategoryAddViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is CategoryAddViewModel vm)
        {
            await vm.ReloadSubCategoriesAsync();
        }
    }
}