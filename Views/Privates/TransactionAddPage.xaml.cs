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
}