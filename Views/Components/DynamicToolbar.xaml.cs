using controle_ja_mobile.Configs;
using controle_ja_mobile.Views.Publics;

namespace controle_ja_mobile.Views.Components;

public partial class DynamicToolbar : ContentView
{
    // Bindable property for page title
    public static readonly BindableProperty PageTitleProperty =
        BindableProperty.Create(nameof(PageTitle), typeof(string), typeof(DynamicToolbar), "Dashboard", propertyChanged: OnPageTitleChanged);

    // Bindable property for user name
    public static readonly BindableProperty UserNameProperty =
        BindableProperty.Create(nameof(UserName), typeof(string), typeof(DynamicToolbar), "Usuário");

    public string PageTitle
    {
        get => (string)GetValue(PageTitleProperty);
        set => SetValue(PageTitleProperty, value);
    }

    public string UserName
    {
        get => (string)GetValue(UserNameProperty);
        set => SetValue(UserNameProperty, value);
    }

    public DynamicToolbar()
    {
        InitializeComponent();
    }

    private static void OnPageTitleChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is DynamicToolbar toolbar && toolbar.LblPageTitle != null)
        {
            toolbar.LblPageTitle.Text = (string)newValue;
        }
    }

    private void OnMenuTapped(object sender, EventArgs e)
    {
        MenuOverlay.IsVisible = true;
    }

    private void OnOverlayTapped(object sender, EventArgs e)
    {
        MenuOverlay.IsVisible = false;
    }

    private async void OnProfileTapped(object sender, EventArgs e)
    {
        MenuOverlay.IsVisible = false;
        await App.Current.MainPage.DisplayAlert("Meu Perfil", "A edição de perfil estará disponível em breve.", "OK");
    }

    private async void OnLogoutTapped(object sender, EventArgs e)
    {
        MenuOverlay.IsVisible = false;

        bool confirm = await App.Current.MainPage.DisplayAlert("Sair", "Tem certeza que deseja desconectar da sua conta?", "Sim", "Não");
        if (!confirm) return;

        // Clear session data
        Preferences.Remove(AppConstants.AuthStorageKey);
        Preferences.Remove(AppConstants.UserNameStorageKey);

        // Navigate to login page
        var loginPageInstance = IPlatformApplication.Current.Services.GetService<LoginPage>();
        Application.Current.MainPage = new NavigationPage(loginPageInstance);
    }
}
