using controle_ja_mobile.Configs;
using controle_ja_mobile.Services;
using controle_ja_mobile.Views.Privates.Management;
using controle_ja_mobile.Views.Publics;

namespace controle_ja_mobile
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new NavigationPage(new WelcomePage(
                IPlatformApplication.Current?.Services.GetService<AuthService>(),
                IPlatformApplication.Current?.Services.GetService<BiometricAuthService>()));
            Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("NoUnderline", (handler, view) =>
            {
#if ANDROID
                handler.PlatformView.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
#endif
            });
        }

    }
}
