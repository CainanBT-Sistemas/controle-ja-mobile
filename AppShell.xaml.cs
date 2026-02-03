using controle_ja_mobile.Views.Privates;
using controle_ja_mobile.Views.Privates.Management;
using System.Windows.Input;

namespace controle_ja_mobile
{
    public partial class AppShell : Shell
    {
        // Propriedade Bindable para controlar a visibilidade da barra no XAML
        public static readonly BindableProperty IsTabBarVisibleProperty =
            BindableProperty.Create(nameof(IsTabBarVisible), typeof(bool), typeof(AppShell), true);

        public bool IsTabBarVisible
        {
            get => (bool)GetValue(IsTabBarVisibleProperty);
            set => SetValue(IsTabBarVisibleProperty, value);
        }

        public AppShell()
        {
            InitializeComponent();
            RegisterRoutes();
            BindingContext = this; // Permite que o XAML enxergue a propriedade IsTabBarVisible
        }

        private void RegisterRoutes()
        {
            // Registra as páginas que não estão nas abas principais (páginas de navegação interna)
            Routing.RegisterRoute(nameof(CreditCardAddPage), typeof(CreditCardAddPage));
            Routing.RegisterRoute(nameof(VehicleAddPage), typeof(VehicleAddPage));
            Routing.RegisterRoute(nameof(TransactionAddPage), typeof(TransactionAddPage));            
            Routing.RegisterRoute(nameof(ManageCreditCardsPage), typeof(ManageCreditCardsPage));
            Routing.RegisterRoute(nameof(ManageVehiclesPage), typeof(ManageVehiclesPage));
            Routing.RegisterRoute(nameof(ManageAccountsPage), typeof(ManageAccountsPage));
            Routing.RegisterRoute(nameof(ManageCategoriesPage), typeof(ManageCategoriesPage));
        }

        // Esse método é chamado toda vez que você troca de tela
        protected override void OnNavigated(ShellNavigatedEventArgs args)
        {
            base.OnNavigated(args);

            // Lógica Inteligente:
            // Se a página atual for uma página de "Adicionar" (AddPage), ESCONDE a barra.
            // Se for uma página principal (Dashboard, Lista), MOSTRA a barra.

            var currentRoute = args.Current?.Location?.ToString();

            if (currentRoute != null)
            {
                // Se a rota contiver "AddPage", definimos visibilidade como FALSE
                if (currentRoute.Contains("AddPage"))
                {
                    IsTabBarVisible = false;
                }
                else
                {
                    IsTabBarVisible = true;
                }
            }
        }
    }
}
