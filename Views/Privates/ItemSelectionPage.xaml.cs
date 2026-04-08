using controle_ja_mobile.ViewModels;

namespace controle_ja_mobile.Views.Privates;

public partial class ItemSelectionPage : ContentPage
{
    public ItemSelectionPage(ItemSelectionViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}