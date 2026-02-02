using controle_ja_mobile.ViewModels;
using controle_ja_mobile.Views.Components;
using controle_ja_mobile.Views.Privates.Tabs;

namespace controle_ja_mobile.Views.Privates;

public partial class DashboardPage : ContentPage
{
    public DashboardPage(DashboardViewModel homeVm, CreditCardsViewModel cardsVm, VehiclesViewModel vehiclesVm)
    {
        InitializeComponent();

        // 1. Instancia as Views e injeta os ViewModels
        var homeView = new HomeView { BindingContext = homeVm };
        var cardsView = new CreditCardsView { BindingContext = cardsVm };
        var vehiclesView = new VehicleListView { BindingContext = vehiclesVm };

        // 2. Adiciona ao Carrossel (Agora ele acha o MainCarousel por causa do x:Name)
        MainCarousel.ItemsSource = new List<ContentView>
        {
            homeView,
            cardsView,
            vehiclesView
        };

        // 3. Inscreve no MessagingCenter
        MessagingCenter.Subscribe<BottomMenu, string>(this, "NavigateTo", (sender, target) =>
        {
            switch (target)
            {
                case "Home": MainCarousel.Position = 0; break;
                case "Cards": MainCarousel.Position = 1; break;
                case "Vehicles": MainCarousel.Position = 2; break;
            }
        });
    }

    private void OnPositionChanged(object sender, PositionChangedEventArgs e)
    {
        // Agora ele acha o MyBottomMenu por causa do x:Name
        if (MyBottomMenu != null)
        {
            switch (e.CurrentPosition)
            {
                case 0: MyBottomMenu.ActivePage = "Home"; break;
                case 1: MyBottomMenu.ActivePage = "Cards"; break;
                case 2: MyBottomMenu.ActivePage = "Vehicles"; break;
            }
        }
    }
}