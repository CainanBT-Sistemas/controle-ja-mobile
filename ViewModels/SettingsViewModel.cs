using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using controle_ja_mobile.Services;
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
            await Shell.Current.DisplayAlert("Meu Perfil", "A edição de perfil estará disponível em breve.", "OK");
        }

        [RelayCommand]
        public async Task GoToAccounts()
        {
            await Shell.Current.DisplayAlert("Contas", "Lista de contas em breve.", "OK");
        }

        [RelayCommand]
        public async Task GoToCategories()
        {
            await Shell.Current.DisplayAlert("Categorias", "Lista de categorias em breve.", "OK");
        }

        [RelayCommand]
        public void GoToCreditCards()
        {
            // Navigate to credit cards list (already exists in carousel)
            MessagingCenter.Send(this, "NavigateTo", "Cards");
        }

        [RelayCommand]
        public void GoToVehicles()
        {
            // Navigate to vehicles list (already exists in carousel)
            MessagingCenter.Send(this, "NavigateTo", "Vehicles");
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
