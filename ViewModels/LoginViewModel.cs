using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using controle_ja_mobile.Services;
using controle_ja_mobile.Views.Privates;
using controle_ja_mobile.Views.Publics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace controle_ja_mobile.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly AuthService _authService;

        [ObservableProperty] private string email;
        [ObservableProperty] private string password;
        [ObservableProperty] private bool isLoading;

        public LoginViewModel(AuthService authService)
        {
            _authService = authService;
        }

        [RelayCommand]
        public async Task Login()
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                await App.Current.MainPage.DisplayAlert("Ops", "Preencha e-mail e senha", "OK");
                return;
            }

            IsLoading = true;
            bool success = await _authService.loginAsync(Email, Password);
            IsLoading = false;

            if (success)
            {
                Application.Current.MainPage = new AppShell();
            }
            else
            {
                await App.Current.MainPage.DisplayAlert("Erro", "E-mail ou senha incorretos", "OK");
            }
        }

        [RelayCommand]
        public async Task GoToRegister()
        {
            var registerPage = IPlatformApplication.Current.Services.GetService<RegisterPage>();
            await Application.Current.MainPage.Navigation.PushAsync(registerPage);
        }

        [RelayCommand]
        public async Task ForgotPassword()
        {
            await App.Current.MainPage.DisplayAlert("Recuperar Senha", "Funcionalidade de recuperação será enviada para seu e-mail.", "OK");
        }
    }
}
