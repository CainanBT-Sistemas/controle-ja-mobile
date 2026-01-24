using controle_ja_mobile.Configs;
using controle_ja_mobile.Views.Publics;

namespace controle_ja_mobile
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            bool hasSeenHome = Preferences.Get("HasSeenHome", false);

            if (hasSeenHome)
            {
                var loginPage = IPlatformApplication.Current.Services.GetService<LoginPage>();
                MainPage = new NavigationPage(loginPage);
            }
            else
            {
                MainPage = new NavigationPage(new WelcomePage());
            }

        }

    }
}
