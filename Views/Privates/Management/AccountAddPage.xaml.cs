using controle_ja_mobile.ViewModels;

namespace controle_ja_mobile.Views.Privates.Management;

public partial class AccountAddPage : ContentPage
{
    public AccountAddPage(AccountAddViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}