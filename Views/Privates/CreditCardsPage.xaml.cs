using controle_ja_mobile.ViewModels;

namespace controle_ja_mobile.Views.Privates;

public partial class CreditCardsPage : ContentPage
{
    private readonly CreditCardsViewModel _vm;
    public CreditCardsPage(CreditCardsViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = _vm;
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadCards();
    }
}