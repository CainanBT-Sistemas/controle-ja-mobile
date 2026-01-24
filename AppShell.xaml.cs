using controle_ja_mobile.Views.Privates;

namespace controle_ja_mobile
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(CreditCardAddPage), typeof(CreditCardAddPage));
            Routing.RegisterRoute(nameof(Views.Privates.VehicleAddPage), typeof(Views.Privates.VehicleAddPage));
            Routing.RegisterRoute(nameof(TransactionAddPage), typeof(TransactionAddPage));
        }
    }
}
