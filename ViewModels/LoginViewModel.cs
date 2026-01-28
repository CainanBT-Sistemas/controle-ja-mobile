using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using controle_ja_mobile.Services;
using controle_ja_mobile.Views.Privates;
using controle_ja_mobile.Views.Publics;

namespace controle_ja_mobile.ViewModels
{
    public partial class LoginViewModel : BaseViewModel
    {
        private readonly AuthService _authService;

        [ObservableProperty] private string email = string.Empty;
        [ObservableProperty] private string password = string.Empty;

        public LoginViewModel(AuthService authService)
        {
            _authService = authService;
        }

        [RelayCommand]
        public async Task Login()
        {
            var mainPage = App.Current.MainPage;
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                if (mainPage != null)
                    await mainPage.DisplayAlert("Ops", "Preencha e-mail e senha", "OK");
                return;
            }

            await ExecuteAsync(async () =>
            {
                bool success = await _authService.loginAsync(Email, Password);

                if (success)
                {
                    Application.Current.MainPage = new AppShell();
                }
                else
                {
                    var currentPage = App.Current.MainPage;
                    if (currentPage != null)
                        await currentPage.DisplayAlert("Erro", "E-mail ou senha incorretos", "OK");
                }
            });
        }

        [RelayCommand]
        public async Task GoToRegister()
        {
            var registerPage = IPlatformApplication.Current?.Services.GetService<RegisterPage>();
            var mainPage = Application.Current.MainPage;
            if (registerPage != null && mainPage?.Navigation != null)
            {
                await mainPage.Navigation.PushAsync(registerPage);
            }
        }

        [RelayCommand]
        public async Task ForgotPassword()
        {
            var mainPage = App.Current.MainPage;
            if (mainPage != null)
                await mainPage.DisplayAlert("Recuperar Senha", "Funcionalidade de recuperação será enviada para seu e-mail.", "OK");
        }
    }
}
