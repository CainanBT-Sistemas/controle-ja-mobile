using controle_ja_mobile.ViewModels;

namespace controle_ja_mobile.Views.Privates;

public partial class ProfilePage : ContentPage
{
    public ProfilePage(ProfileViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}