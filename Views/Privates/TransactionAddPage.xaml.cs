using controle_ja_mobile.ViewModels;

namespace controle_ja_mobile.Views.Privates;

public partial class TransactionAddPage : ContentPage
{
    public TransactionAddPage(TransactionAddViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}