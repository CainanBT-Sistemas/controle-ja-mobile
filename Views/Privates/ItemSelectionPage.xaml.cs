using controle_ja_mobile.ViewModels;

namespace controle_ja_mobile.Views.Privates;

public partial class ItemSelectionPage : ContentPage
{
    public ItemSelectionPage(ItemSelectionViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is ItemSelectionViewModel vm)
        {
            if (vm.ReloadCategoriesCommand.CanExecute(null))
            {
                await vm.ReloadCategoriesCommand.ExecuteAsync(null);
            }
        }
    }
}