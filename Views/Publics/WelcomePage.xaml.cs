namespace controle_ja_mobile.Views.Publics;

using controle_ja_mobile.Services;
using Microsoft.Maui.Controls;
using controle_ja_mobile.Views.Privates;
using controle_ja_mobile.ViewModels;
using System.Net.Http.Headers;

public partial class WelcomePage : ContentPage
{
    private readonly AuthService _authService;
    private readonly BiometricAuthService _biometricAuthService;
    public WelcomePage(AuthService authService, BiometricAuthService biometricAuthService)
    {
        InitializeComponent();
        _authService = authService;
        _biometricAuthService = biometricAuthService;
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

        try
        {
            // Check if there's a saved refresh token
            var token = await SecureStorage.GetAsync("refresh_token");

            if (!string.IsNullOrEmpty(token))
            {
                // Check if biometric authentication is available
                bool biometricAvailable = await _biometricAuthService.IsBiometricAvailableAsync();

                if (biometricAvailable)
                {
                    // Request biometric authentication (includes PIN fallback)
                    bool authenticated = await _biometricAuthService.AuthenticateAsync(
                        "Autentique-se para acessar o Controle Já"
                    );

                    if (authenticated)
                    {
                        // User authenticated successfully, proceed with token login
                        bool success = await _authService.loginWithTokenAsync(token);

                        if (success)
                        {
                            // Navigate to main app
                            NavigateToMainApp();
                            return;
                        }
                    }
                    else
                    {
                        // Biometric authentication failed or was cancelled
                        // User stays on welcome page to choose another login method
                    }
                }
                else
                {
                    // Biometric not available, use automatic login with token
                    bool success = await _authService.loginWithTokenAsync(token);

                    if (success)
                    {
                        NavigateToMainApp();
                    }
                    else
                    {
                        // Token is invalid, clear it
                        ClearRefreshToken();
                    }
                }
            }
            // If no token exists, user stays on welcome page to choose login method
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"OnAppearing error: {ex.Message}");
            await DisplayAlert("Erro", "Ocorreu um erro ao iniciar o aplicativo.", "OK");
        }
        finally
        {
            LoadingOverlay.IsVisible = false;
        }
    }

    private void NavigateToMainApp()
    {
        Dispatcher.Dispatch(() =>
        {
            Application.Current.MainPage = new AppShell();
        });
    }

    private void ClearRefreshToken()
    {
        SecureStorage.Remove("refresh_token");
        SecureStorage.Remove("auth_token");
        Preferences.Remove("UserName");
    }
}