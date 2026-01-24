using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using controle_ja_mobile.Services;
using controle_ja_mobile.Views.Publics;

namespace controle_ja_mobile.ViewModels
{
    public partial class RegisterViewModel : ObservableObject
    {
        private readonly AuthService _authService;

        [ObservableProperty]
        private string name;

        [ObservableProperty]
        private string email;

        [ObservableProperty]
        private string password;

        [ObservableProperty]
        private string confirmPassword;

        [ObservableProperty]
        private bool isLoading;

        public RegisterViewModel(AuthService authService)
        {
            _authService = authService;
        }

        [RelayCommand]
        public async Task Register()
        {
            // 1. Validações Básicas
            if (string.IsNullOrWhiteSpace(Name) ||
                string.IsNullOrWhiteSpace(Email) ||
                string.IsNullOrWhiteSpace(Password))
            {
                await App.Current.MainPage.DisplayAlert("Atenção", "Preencha todos os campos.", "OK");
                return;
            }

            if (Password != ConfirmPassword)
            {
                await App.Current.MainPage.DisplayAlert("Erro", "As senhas não coincidem.", "OK");
                return;
            }

            // 2. Inicia o Loading (Trava a tela e mostra o spinner)
            IsLoading = true;

            // 3. Chama a API
            bool sucesso = await _authService.RegisterAsync(Name, Email, Password);

            // 4. Para o Loading
            IsLoading = false;

            // 5. Verifica o resultado
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

        // Ação do Botão "Voltar para Login"
        [RelayCommand]
        public async Task GoBack()
        {
            // Volta para a tela anterior na pilha de navegação
            await Application.Current.MainPage.Navigation.PopAsync();
        }
    }
}
