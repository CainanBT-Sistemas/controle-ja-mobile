using controle_ja_mobile.ViewModels;
namespace controle_ja_mobile.Views.Privates.Management;

public partial class CreditCardAddPage : ContentPage
{
    public CreditCardAddPage(CreditCardAddViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}