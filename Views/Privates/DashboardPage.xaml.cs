using controle_ja_mobile.ViewModels;

namespace controle_ja_mobile.Views.Privates;

public partial class DashboardPage : ContentPage
{
    private readonly DashboardViewModel _viewModel;
    public DashboardPage(DashboardViewModel vm)
	{
		InitializeComponent();
        _viewModel = vm;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_viewModel != null)
        {
            await _viewModel.LoadData();
        }
    }
}