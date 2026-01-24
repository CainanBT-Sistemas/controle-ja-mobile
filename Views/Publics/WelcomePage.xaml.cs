namespace controle_ja_mobile.Views.Publics;

using Microsoft.Maui.Controls;

public partial class WelcomePage : ContentPage
{
	public WelcomePage()
	{
		InitializeComponent();
	}
    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        var registerPage = IPlatformApplication.Current.Services.GetService<RegisterPage>();
        await Navigation.PushAsync(registerPage);
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        var loginPage = IPlatformApplication.Current.Services.GetService<LoginPage>();
        await Navigation.PushAsync(loginPage);
    }
}