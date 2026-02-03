using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using controle_ja_mobile.Services;
using controle_ja_mobile.Views.Privates.Management; // Namespace sugerido
using controle_ja_mobile.Views.Publics;

namespace controle_ja_mobile.ViewModels
{
    public partial class SettingsViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;

        [ObservableProperty] private string userName;
        [ObservableProperty] private string userEmail;
        [ObservableProperty] private string appVersion;

        public SettingsViewModel(ApiService apiService)
        {
            _apiService = apiService;
            UserName = Preferences.Get("UserName", "Usuário");
            UserEmail = Preferences.Get("UserEmail", "usuario@email.com");
            AppVersion = AppInfo.VersionString;
        }

        [RelayCommand]
        public async Task GoToProfile()
        {
            // Em breve página de edição de perfil
            await Shell.Current.DisplayAlert("Meu Perfil", "A edição de perfil estará disponível em breve.", "OK");
        }

        [RelayCommand]
        public async Task GoToAccounts()
        {
            await Shell.Current.GoToAsync(nameof(ManageAccountsPage));
        }

        [RelayCommand]
        public async Task GoToCategories()
        {
            await Shell.Current.GoToAsync(nameof(ManageCategoriesPage));
        }

        [RelayCommand]
        public async Task GoToCreditCards()
        {
            // Navega para a página de gerenciamento dedicada
            await Shell.Current.GoToAsync(nameof(ManageCreditCardsPage));
        }

        [RelayCommand]
        public async Task GoToVehicles()
        {
            // Navega para a página de gerenciamento dedicada
            await Shell.Current.GoToAsync(nameof(ManageVehiclesPage));
        }

        [RelayCommand]
        public async Task ChangePassword()
        {
            await Shell.Current.DisplayAlert("Alterar Senha", "Funcionalidade disponível em breve.", "OK");
        }

        [RelayCommand]
        public async Task PerformLogout()
        {
            bool confirm = await Shell.Current.DisplayAlert("Sair", "Tem certeza que deseja desconectar da sua conta?", "Sim", "Não");
            if (!confirm) return;

            Preferences.Remove("AuthToken");
            Preferences.Remove("UserName");
            Preferences.Remove("UserEmail");

            var loginPage = IPlatformApplication.Current.Services.GetService<LoginPage>();
            Application.Current.MainPage = new NavigationPage(loginPage);
        }

        [RelayCommand]
        public async Task ShowAbout()
        {
            await Shell.Current.DisplayAlert("Sobre", $"Controle Já\nVersão {AppVersion}\n\nDesenvolvido por CainanBT Sistemas", "OK");
        }
    }
}