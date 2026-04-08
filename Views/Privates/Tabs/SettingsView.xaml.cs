using controle_ja_mobile.ViewModels;

namespace controle_ja_mobile.Views.Privates.Tabs;

public partial class SettingsView : ContentView
{
    public SettingsView()
    {
        InitializeComponent();
        this.Loaded += OnSettingsViewLoaded;
    }

    private void OnSettingsViewLoaded(object sender, EventArgs e)
    {
        if (BindingContext is SettingsViewModel vm)
        {
            vm.LoadUserData();
        }
    }
}