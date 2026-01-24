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

        // Escuta mudanças na ViewModel para disparar animações
        _viewModel.PropertyChanged += ViewModel_PropertyChanged;
    }

    private async void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(DashboardViewModel.IsMenuOpen))
        {
            if (_viewModel.IsMenuOpen)
            {
                // --- ABRIR MENU (Explosão) ---

                // 1. Habilita visibilidade e clique
                BtnIncome.Opacity = 1; IconIncome.Opacity = 1;
                BtnExpense.Opacity = 1; IconExpense.Opacity = 1;
                BtnIncome.InputTransparent = false;
                BtnExpense.InputTransparent = false;

                // 2. Animação em V (TranslateTo X, Y)
                // Receita: Esquerda (-70) e Cima (-70)
                var animIncome = BtnIncome.TranslateTo(-70, -70, 250, Easing.CubicOut);
                var animIconIncome = IconIncome.TranslateTo(-70, -70, 250, Easing.CubicOut);

                // Despesa: Direita (70) e Cima (-70)
                var animExpense = BtnExpense.TranslateTo(70, -70, 250, Easing.CubicOut);
                var animIconExpense = IconExpense.TranslateTo(70, -70, 250, Easing.CubicOut);

                // Gira o botão +
                var animFab = FabButton.RotateTo(45, 250, Easing.CubicOut);

                // Aguarda todos terminarem
                await Task.WhenAll(animIncome, animIconIncome, animExpense, animIconExpense, animFab);
            }
            else
            {
                // --- FECHAR MENU (Recolher) ---

                // 1. Volta pro centro (0, 0)
                var animIncome = BtnIncome.TranslateTo(0, 0, 250, Easing.CubicIn);
                var animIconIncome = IconIncome.TranslateTo(0, 0, 250, Easing.CubicIn);

                var animExpense = BtnExpense.TranslateTo(0, 0, 250, Easing.CubicIn);
                var animIconExpense = IconExpense.TranslateTo(0, 0, 250, Easing.CubicIn);

                var animFab = FabButton.RotateTo(0, 250, Easing.CubicIn);

                await Task.WhenAll(animIncome, animIconIncome, animExpense, animIconExpense, animFab);

                // 2. Esconde e desabilita clique
                BtnIncome.Opacity = 0; IconIncome.Opacity = 0;
                BtnExpense.Opacity = 0; IconExpense.Opacity = 0;
                BtnIncome.InputTransparent = true;
                BtnExpense.InputTransparent = true;
            }
        }
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