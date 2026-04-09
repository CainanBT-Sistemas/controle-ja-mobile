using controle_ja_mobile.ViewModels;

namespace controle_ja_mobile.Views.Privates;

public partial class TransactionAddPage : ContentPage
{
    public TransactionAddPage(ViewModels.TransactionAddViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private void OnDateSelected(object sender, DateChangedEventArgs e)
    {
        if (BindingContext is ViewModels.TransactionAddViewModel vm)
        {
            vm.Date = e.NewDate;
        }
    }

    private void OnBackgroundTapped(object sender, TappedEventArgs e)
    {
#if ANDROID
        var activity = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity;
        var currentFocus = activity?.CurrentFocus;

        if (currentFocus != null)
        {
            currentFocus.ClearFocus();
            var inputMethodManager = activity.GetSystemService(Android.Content.Context.InputMethodService) as Android.Views.InputMethods.InputMethodManager;
            inputMethodManager?.HideSoftInputFromWindow(currentFocus.WindowToken, Android.Views.InputMethods.HideSoftInputFlags.None);
        }
#endif
    }
}