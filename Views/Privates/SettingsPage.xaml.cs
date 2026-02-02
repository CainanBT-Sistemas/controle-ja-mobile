using controle_ja_mobile.Views.Publics;

namespace controle_ja_mobile.Views.Privates
{
    public partial class SettingsPage : ContentPage
    {
        public SettingsPage()
        {
            InitializeComponent();
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("..");
        }

        private async void OnProfileClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Perfil", "Funcionalidade de perfil em desenvolvimento", "OK");
        }

        private async void OnSecurityClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Segurança", "Funcionalidade de segurança em desenvolvimento", "OK");
        }

        private async void OnNotificationsClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Notificações", "Funcionalidade de notificações em desenvolvimento", "OK");
        }

        private async void OnThemeClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Tema", "Funcionalidade de tema em desenvolvimento", "OK");
        }

        private async void OnHelpClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Ajuda", "Funcionalidade de ajuda em desenvolvimento", "OK");
        }

        private async void OnAboutClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Sobre o App", "Controle Já - Versão 1.0.0\n\nDesenvolvido por CainanBT Sistemas", "OK");
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlert("Sair", "Deseja realmente sair da sua conta?", "Sim", "Não");
            
            if (confirm)
            {
                // Limpar dados de autenticação se necessário
                // SecureStorage.Remove("auth_token");
                
                // Navegar para a página de login
                await Shell.Current.GoToAsync($"//{nameof(LoginPage)}");
            }
        }
    }
}
