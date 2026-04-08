using controle_ja_mobile.ViewModels;

namespace controle_ja_mobile.Views.Privates;

public partial class ChangePasswordPage : ContentPage
{
    public ChangePasswordPage(ChangePasswordViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}