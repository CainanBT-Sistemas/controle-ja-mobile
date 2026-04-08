using controle_ja_mobile.ViewModels;

namespace controle_ja_mobile.Views.Privates.Tabs;

public partial class TransactionsListView : ContentView
{
    public TransactionsListView()
    {
        InitializeComponent();
        this.Loaded += OnViewLoaded;
    }

    private void OnViewLoaded(object sender, EventArgs e)
    {
        // Trava "_isLoaded" removida. Agora ele busca os dados toda vez que a aba é aberta.
        var services = IPlatformApplication.Current?.Services;

        if (services != null)
        {
            var vm = services.GetService<TransactionsViewModel>();
            if (vm != null)
            {
                this.Content.BindingContext = vm;

                // Força a recarga da API sempre
                vm.LoadTransactionsCommand.Execute(null);
            }
        }
    }
}
