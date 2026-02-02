namespace controle_ja_mobile.Views.Publics;

using controle_ja_mobile.Services;
using Microsoft.Maui.Controls;
using controle_ja_mobile.Views.Privates;
using controle_ja_mobile.ViewModels;
using System.Net.Http.Headers;

public partial class WelcomePage : ContentPage
{
    private readonly AuthService _authService;
    public WelcomePage(AuthService authService)
    {
        InitializeComponent();
        _authService = authService;
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

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        LoadingOverlay.IsVisible = true;
        var token = await SecureStorage.GetAsync("refresh_token");
        if (!string.IsNullOrEmpty(token))
        {
            bool success = await _authService.loginWithTokenAsync(token);
            if (success)
            {
                Application.Current.MainPage = new AppShell();
            }
        }
        LoadingOverlay.IsVisible = false;
    }
}