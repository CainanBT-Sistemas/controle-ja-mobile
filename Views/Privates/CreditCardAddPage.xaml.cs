using controle_ja_mobile.ViewModels;

namespace controle_ja_mobile.Views.Privates;

public partial class CreditCardAddPage : ContentPage
{
    public CreditCardAddPage(CreditCardsViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}