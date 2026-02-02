namespace controle_ja_mobile.Views.Publics;

using controle_ja_mobile.ViewModels;

public partial class RegisterPage : ContentPage
{
	public RegisterPage(RegisterViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;
    }
}