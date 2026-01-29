using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using controle_ja_mobile.Services;
using controle_ja_mobile.Views.Publics;

namespace controle_ja_mobile.ViewModels
{
    public partial class RegisterViewModel : BaseViewModel
    {
        private readonly AuthService _authService;

        [ObservableProperty] private string name;
        [ObservableProperty] private string email;
        [ObservableProperty] private string password;
        [ObservableProperty] private string confirmPassword;

        public RegisterViewModel(AuthService authService)
        {
            _authService = authService;
        }

        [RelayCommand]
        public async Task Register()
        {
            // Validações continuam fora para serem rápidas
            if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                await App.Current.MainPage.DisplayAlert("Atenção", "Preencha todos os campos.", "OK");
                return;
            }

            if (Password != ConfirmPassword)
            {
                await App.Current.MainPage.DisplayAlert("Erro", "As senhas não coincidem.", "OK");
                return;
            }

            bool sucesso = await _authService.RegisterAsync(Name, Email, Password);

            if (sucesso)
            {
                await App.Current.MainPage.DisplayAlert("Sucesso", "Conta criada com sucesso! Faça login.", "OK");
                var loginPage = IPlatformApplication.Current.Services.GetService<LoginPage>();
                Application.Current.MainPage = loginPage;
            }
            else
            {
                await App.Current.MainPage.DisplayAlert("Erro", "Falha ao criar conta. Verifique os dados ou tente outro e-mail.", "OK");
            }
        }

        [RelayCommand]
        public async Task GoBack()
        {
            await Application.Current.MainPage.Navigation.PopAsync();
        }
    }
}