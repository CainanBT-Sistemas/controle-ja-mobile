using controle_ja_mobile.Configs;
using controle_ja_mobile.Services;
using controle_ja_mobile.Views.Publics;

namespace controle_ja_mobile
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new NavigationPage(new WelcomePage(IPlatformApplication.Current.Services.GetService<AuthService>()));

        }

    }
}
