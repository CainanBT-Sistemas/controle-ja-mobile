using controle_ja_mobile.ViewModels;

namespace controle_ja_mobile.Views.Privates;

public partial class InvoiceDetailsPage : ContentPage
{
    public InvoiceDetailsPage(InvoiceDetailsViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}