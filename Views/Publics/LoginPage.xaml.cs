using controle_ja_mobile.ViewModels;

namespace controle_ja_mobile.Views.Publics;

public partial class LoginPage : ContentPage
{
    public LoginPage(LoginViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm; 
    }
}