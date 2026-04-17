using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using controle_ja_mobile.Services;
using controle_ja_mobile.Views.Privates.Management;
using controle_ja_mobile.Views.Publics;
using Microsoft.Maui.Storage;

namespace controle_ja_mobile.ViewModels
{
    public partial class SettingsViewModel : BaseViewModel
    {
        private readonly AuthService _authService;
        private readonly BiometricAuthService _biometricAuthService;

        [ObservableProperty] private string userName;
        [ObservableProperty] private string userEmail;
        [ObservableProperty] private string appVersion;

        public SettingsViewModel(AuthService authService, BiometricAuthService biometricAuthService)
        {
            _authService = authService;
            LoadUserData();
            _biometricAuthService = biometricAuthService;
            AppVersion = AppInfo.VersionString;

            WeakReferenceMessenger.Default.Register<ProfileUpdatedMessage>(this, (r, m) =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    LoadUserData();
                });
            });
        }

        public void LoadUserData()
        {
            UserName = Preferences.Get("UserName", "Usuário");
            UserEmail = Preferences.Get("UserEmail", "usuario@email.com");
        }

        [RelayCommand]
        public async Task GoToProfile()
        {
            await NavigateToAsync("ProfilePage");
        }

        [RelayCommand]
        public void GoToAccounts()
        {
            var vm = IPlatformApplication.Current?.Services.GetService<AccountsViewModel>();
            if (vm != null)
            {
                var popup = new Views.Popups.ManageAccountsPopup(vm);
                Shell.Current.ShowPopup(popup);
            }
        }

        [RelayCommand]
        public void GoToCategories()
        {
            var vm = IPlatformApplication.Current?.Services.GetService<CategoriesViewModel>();
            if (vm != null)
            {
                var popup = new Views.Popups.ManageCategoriesPopup(vm);
                Shell.Current.ShowPopup(popup);
            }
        }

        [RelayCommand]
        public void GoToCreditCards()
        {
            var vm = IPlatformApplication.Current?.Services.GetService<CreditCardsViewModel>();
            if (vm != null)
            {
                var popup = new Views.Popups.ManageCreditCardsPopup(vm);
                Shell.Current.ShowPopup(popup);
            }
        }

        [RelayCommand]
        public void GoToVehicles()
        {
            var vm = IPlatformApplication.Current?.Services.GetService<VehiclesViewModel>();
            if (vm != null)
            {
                var popup = new Views.Popups.ManageVehiclesPopup(vm);
                Shell.Current.ShowPopup(popup);
            }
        }

        [RelayCommand]
        public async Task ResetData()
        {
            bool confirm = await Shell.Current.DisplayAlert("Atenção", "Esta ação apagará todos os seus lançamentos e saldo. Deseja continuar?", "Sim, limpar tudo", "Cancelar");
            if (!confirm) return;

            bool hasBiometrics = await _biometricAuthService.IsBiometricAvailableAsync();
            if (hasBiometrics)
            {
                bool authenticated = await _biometricAuthService.AuthenticateAsync("Autentique-se para confirmar a exclusão");
                if (!authenticated) return;
            }
            await ExecuteWithErrorHandlingAsync(async () =>
            {
                bool success = await _authService.resetDataUser();

                if (success)
                {
                    await Shell.Current.DisplayAlert("Conta resetada", "Sua conta foi resetada com sucesso. Faça login novamente", "OK");

                    SecureStorage.RemoveAll();
                    Preferences.Clear();

                    var welcomePage = IPlatformApplication.Current.Services.GetService<WelcomePage>();
                    Application.Current.MainPage = new NavigationPage(welcomePage);
                }
            });
        }

        [RelayCommand]
        public async Task PerformLogout()
        {
            bool confirm = await Shell.Current.DisplayAlert("Sair", "Tem certeza que deseja desconectar da sua conta?", "Sim", "Não");
            if (!confirm) return;

            SecureStorage.RemoveAll();
            Preferences.Clear();

            var welcomePage = IPlatformApplication.Current.Services.GetService<WelcomePage>();
            Application.Current.MainPage = new NavigationPage(welcomePage);
        }

        [RelayCommand]
        public async Task ShowAbout()
        {
            await Shell.Current.DisplayAlert("Sobre", $"Controle Já\nVersão {AppVersion}\n\nDesenvolvido por CainanBT Sistemas", "OK");
        }
    }

    // CORREÇÃO: A classe responsável por carregar a mensagem na memória!
    public class ProfileUpdatedMessage { }
}