using CommunityToolkit.Maui.Views;
using controle_ja_mobile.ViewModels;

namespace controle_ja_mobile.Views.Popups;

public partial class ManageCategoriesPopup : Popup
{
    public ManageCategoriesPopup(CategoriesViewModel vm)
    {
        vm.IsLoading = true;
        InitializeComponent();
        BindingContext = vm;
        vm.PopupInstance = this;

        this.Opened += async (s, e) =>
        {
            await Task.Delay(600);
            await vm.LoadCategoriesCommand.ExecuteAsync(null);
        };
    }
}