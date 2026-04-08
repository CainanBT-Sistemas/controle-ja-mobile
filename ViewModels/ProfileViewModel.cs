using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using controle_ja_mobile.Services;
using controle_ja_mobile.Views.Privates;
using controle_ja_mobile.Views.Publics;

namespace controle_ja_mobile.ViewModels
{
    public partial class ProfileViewModel : BaseViewModel
    {
        private readonly AuthService _authService;
        private readonly BiometricAuthService _biometricAuthService;

        [ObservableProperty] private string userName;
        [ObservableProperty] private string userEmail;

        public ProfileViewModel(AuthService authService, BiometricAuthService biometricAuthService)
        {
            _authService = authService;
            _biometricAuthService = biometricAuthService;
            UserName = Preferences.Get("UserName", "");
            UserEmail = Preferences.Get("UserEmail", "");
        }

        [RelayCommand]
        private async Task UpdateProfile()
        {
            if (string.IsNullOrWhiteSpace(UserName))
            {
                await Shell.Current.DisplayAlert("Aviso", "Nome e E-mail não podem ficar vazios.", "OK");
                return;
            }

            // 1. Verifica se o aparelho TEM suporte a biometria/PIN configurado
            bool hasBiometrics = await _biometricAuthService.IsBiometricAvailableAsync();

            if (hasBiometrics)
            {
                // 2. Se tiver, exige a digital
                bool authenticated = await _biometricAuthService.AuthenticateAsync("Confirme sua identidade para alterar seus dados");

                // Se cancelou ou errou, para o processo
                if (!authenticated) return;
            }

            // 3. Manda para a API
            await ExecuteWithErrorHandlingAsync(async () =>
            {
                var success = await _authService.UpdateProfileAsync(UserName.Trim());

                if (success)
                {
                    await Shell.Current.DisplayAlert("Sucesso", "Perfil atualizado com sucesso!", "OK");

                    // AQUI ESTÁ O SEGREDO! Essa linha avisa a SettingsView para recarregar
                    WeakReferenceMessenger.Default.Send(new ProfileUpdatedMessage());
                }
            });
        }

        [RelayCommand]
        private async Task GoToChangePassword()
        {
            await Shell.Current.GoToAsync(nameof(ChangePasswordPage));
        }

        [RelayCommand]
        private async Task GoBack()
        {
            await Shell.Current.GoToAsync("..");
        }

        [RelayCommand]
        private async Task DeleteAccount()
        {
            //Confirmação visual
            bool confirm = await Shell.Current.DisplayAlert("Atenção",
                "Sua conta será desativada e você perderá o acesso aos dados. Deseja continuar?",
                "Sim, excluir", "Cancelar");

            if (!confirm) return;

            //Validação Biométrica (Segurança)
            bool hasBiometrics = await _biometricAuthService.IsBiometricAvailableAsync();
            if (hasBiometrics)
            {
                bool authenticated = await _biometricAuthService.AuthenticateAsync("Autentique-se para confirmar a exclusão");
                if (!authenticated) return;
            }
            //Execução
            await ExecuteWithErrorHandlingAsync(async () =>
            {
                bool success = await _authService.DeleteAccountAsync();

                if (success)
                {
                    await Shell.Current.DisplayAlert("Conta Excluída", "Sua conta foi desativada com sucesso.", "OK");

                    // Limpeza obrigatória de segurança no App
                    SecureStorage.RemoveAll();
                    Preferences.Clear();
                    var welcomePage = IPlatformApplication.Current.Services.GetService<WelcomePage>();
                    Application.Current.MainPage = new NavigationPage(welcomePage);
                }
                else
                {
                    await Shell.Current.DisplayAlert("Erro", "Não foi possível processar a exclusão no servidor.", "OK");
                }
            });
        }
    }
}