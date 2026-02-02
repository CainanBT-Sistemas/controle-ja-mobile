using Android.App;
using Android.Content.PM;
using Android.OS;

namespace controle_ja_mobile
{
    [Activity(
        Theme = "@style/Maui.SplashTheme", 
        MainLauncher = true,
        LaunchMode = LaunchMode.SingleTop,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            
            // Initialize Plugin.Fingerprint
            Plugin.Fingerprint.CrossFingerprint.SetCurrentActivityResolver(() => this);
        }
    }
}