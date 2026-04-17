using CommunityToolkit.Maui.Views;
using controle_ja_mobile.ViewModels;

namespace controle_ja_mobile.Views.Popups;

public partial class CategoryAddPopup : Popup
{
    public CategoryAddPopup(CategoryAddViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        viewModel.PopupInstance = this;
        this.Opened += OnPopupOpened;
    }

    private async void OnPopupOpened(object? sender, CommunityToolkit.Maui.Core.PopupOpenedEventArgs e)
    {
        if (BindingContext is CategoryAddViewModel vm)
        {
            await vm.ReloadSubCategoriesAsync();
        }
    }
}