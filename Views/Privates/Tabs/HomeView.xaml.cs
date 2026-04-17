using controle_ja_mobile.ViewModels;

namespace controle_ja_mobile.Views.Privates.Tabs;

public partial class HomeView : ContentView
{
    private bool _isSkeletonAnimating = false;

    public HomeView()
    {
        InitializeComponent();
        this.Loaded += OnHomeLoaded;
        this.Unloaded += (s, e) => _isSkeletonAnimating = false;
    }

    private async void OnHomeLoaded(object sender, EventArgs e)
    {
        if (BindingContext is DashboardViewModel vm)
        {
            // Inicia a animação dos quadrados cinzas da Home
            _isSkeletonAnimating = true;
            AnimateDashboardSkeleton();

            // Dá fôlego pro MAUI desenhar
            await Task.Delay(150);

            // Inicia a busca dos dados (O ViewModel agora tira o IsLoading sozinho)
            await vm.GoToHomeCommand.ExecuteAsync(null);

            // Para a animação quando finalizar
            _isSkeletonAnimating = false;
        }
    }

    private async void AnimateDashboardSkeleton()
    {
        while (_isSkeletonAnimating && DashboardSkeletonView != null)
        {
            await DashboardSkeletonView.FadeTo(0.3, 500, Easing.Linear);
            if (!_isSkeletonAnimating) break;
            await DashboardSkeletonView.FadeTo(0.8, 500, Easing.Linear);
        }
    }
}