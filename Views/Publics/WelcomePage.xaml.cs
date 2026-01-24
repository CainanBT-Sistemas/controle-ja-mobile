namespace controle_ja_mobile.Views.Publics;

using controle_ja_mobile.Services;
using Microsoft.Maui.Controls;

public partial class WelcomePage : ContentPage
{
    public WelcomePage()
    {
        InitializeComponent();
    }

    private async void OnGoogleLoginClicked(object sender, EventArgs e)
    {
        LoadingOverlay.IsVisible = true;

        try
        {
            var authService = IPlatformApplication.Current.Services.GetService<AuthService>();

            if (authService != null)
            {
                // Chama o login
                bool success = await authService.LoginWithGoogleAsync();

                if (success)
                {
                    // Dispatcher para garantir a troca na thread principal
                    Dispatcher.Dispatch(() =>
                    {
                        Application.Current.MainPage = new AppShell();
                    });
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", ex.Message, "OK");
        }
        finally
        {
            // Libera a tela escondendo o Overlay
            LoadingOverlay.IsVisible = false;
        }
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        var loginPage = IPlatformApplication.Current.Services.GetService<LoginPage>();
        await Navigation.PushAsync(loginPage);
    }
}