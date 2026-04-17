using controle_ja_mobile.ViewModels;

namespace controle_ja_mobile.Views.Privates.Tabs;

public partial class TransactionsListView : ContentView
{
    public TransactionsListView()
    {
        InitializeComponent();
        this.Loaded += OnViewLoaded;
    }

    private async void OnViewLoaded(object sender, EventArgs e)
    {
        var services = IPlatformApplication.Current?.Services;
        if (services != null)
        {
            var vm = services.GetService<TransactionsViewModel>();
            if (vm != null)
            {
                vm.IsLoading = true;
                this.Content.BindingContext = vm;

                await Task.Delay(150);

                await vm.LoadTransactionsCommand.ExecuteAsync(null);
            }
        }
    }
}