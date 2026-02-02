using Android.App;
using Android.Content;
using Android.Content.PM;

namespace controle_ja_mobile
{
    // Essa Activity só acorda quando o Google chama o link "com.googleusercontent..."
    [Activity(NoHistory = true, LaunchMode = LaunchMode.SingleTop, Exported = true)]
    [IntentFilter(
        new[] { Intent.ActionView },
        Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
        DataScheme = "com.googleusercontent.apps.336338674705-v2hntbb10rnof4mrrkf8k03c2ot1opv9")] // SEU ID REVERSO
    public class GoogleAuthActivity : Microsoft.Maui.Authentication.WebAuthenticatorCallbackActivity
    {
        // Não precisa de código. A herança já faz o trabalho de avisar o App.
    }
}