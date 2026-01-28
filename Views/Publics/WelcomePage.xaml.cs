namespace controle_ja_mobile.Views.Publics;

using controle_ja_mobile.Services;
using Microsoft.Maui.Controls;
using Plugin.Fingerprint.Abstractions;
using Plugin.Fingerprint;
using controle_ja_mobile.Views.Privates;

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

        // Chama a autenticação biométrica
        await AuthenticateWithBiometricsAsync();
    }

    private async Task AuthenticateWithBiometricsAsync()
    {
        // Tenta autenticar com biometria
        bool isAuthenticated = await _authService.AuthenticateWithBiometricsAsync();

        if (isAuthenticated)
        {
            // Redireciona para a Dashboard após autenticação biométrica bem-sucedida
            var dashboardPage = IPlatformApplication.Current.Services.GetService<DashboardPage>();
            await Navigation.PushAsync(dashboardPage);
        }
        else
        {
            // Exibe uma mensagem de erro ou mantém na WelcomePage
            await DisplayAlert("Erro", "Não foi possível realizar a autenticação biométrica.", "OK");
        }
    }
}