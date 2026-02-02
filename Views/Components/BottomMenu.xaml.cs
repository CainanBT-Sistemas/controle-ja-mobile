using controle_ja_mobile.Views.Privates;
using Microsoft.Maui.Graphics;

namespace controle_ja_mobile.Views.Components;

public partial class BottomMenu : ContentView
{
    private bool _isMenuOpen = false;

    // Cores
    private readonly Color ActiveColor = Color.FromArgb("#00E676");
    private readonly Color InactiveColor = Color.FromArgb("#64748B");

    // Propriedade Bindable (Recebe aviso da Dashboard quando o usuário desliza o dedo)
    public static readonly BindableProperty ActivePageProperty =
        BindableProperty.Create(nameof(ActivePage), typeof(string), typeof(BottomMenu), "Home", propertyChanged: OnActivePageChanged);

    public string ActivePage
    {
        get => (string)GetValue(ActivePageProperty);
        set => SetValue(ActivePageProperty, value);
    }

    public BottomMenu()
    {
        InitializeComponent();
        UpdateVisualState();
    }

    private static void OnActivePageChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is BottomMenu menu) menu.UpdateVisualState();
    }

    // Pinta os ícones baseados na página atual
    private void UpdateVisualState()
    {
        if (PathHome == null) return; // Proteção

        // 1. Reseta tudo
        PathHome.Fill = InactiveColor; LblHome.TextColor = InactiveColor;
        PathTransactions.Fill = InactiveColor; LblTransactions.TextColor = InactiveColor;
        PathCards.Fill = InactiveColor; LblCards.TextColor = InactiveColor;
        PathVehicles.Fill = InactiveColor; LblVehicles.TextColor = InactiveColor;
        PathSettings.Fill = InactiveColor; LblSettings.TextColor = InactiveColor;

        // 2. Pinta o ativo
        switch (ActivePage)
        {
            case "Home":
                PathHome.Fill = ActiveColor; LblHome.TextColor = ActiveColor; break;
            case "Transactions":
                PathTransactions.Fill = ActiveColor; LblTransactions.TextColor = ActiveColor; break;
            case "Cards":
                PathCards.Fill = ActiveColor; LblCards.TextColor = ActiveColor; break;
            case "Vehicles":
                PathVehicles.Fill = ActiveColor; LblVehicles.TextColor = ActiveColor; break;
            case "Settings":
                PathSettings.Fill = ActiveColor; LblSettings.TextColor = ActiveColor; break;
        }
    }

    // --- NAVEGAÇÃO DAS TABS ---
    private void OnHomeClicked(object sender, EventArgs e)
    {
        MessagingCenter.Send(this, "NavigateTo", "Home");
    }

    private void OnTransactionsClicked(object sender, EventArgs e)
    {
        MessagingCenter.Send(this, "NavigateTo", "Transactions");
    }

    private void OnCardsClicked(object sender, EventArgs e)
    {
        MessagingCenter.Send(this, "NavigateTo", "Cards");
    }

    private void OnVehiclesClicked(object sender, EventArgs e)
    {
        MessagingCenter.Send(this, "NavigateTo", "Vehicles");
    }

    private void OnSettingsClicked(object sender, EventArgs e)
    {
        MessagingCenter.Send(this, "NavigateTo", "Settings");
    }

    // --- LÓGICA DO FAB (Botão +) ---
    private async void OnFabClicked(object sender, EventArgs e)
    {
        _isMenuOpen = !_isMenuOpen;
        await ToggleMenuAnimations();
    }

    private async void OnNewIncomeClicked(object sender, EventArgs e)
    {
        _isMenuOpen = false; await ToggleMenuAnimations();
        await Shell.Current.GoToAsync($"{nameof(TransactionAddPage)}?type=RECEITA");
    }

    private async void OnNewExpenseClicked(object sender, EventArgs e)
    {
        _isMenuOpen = false; await ToggleMenuAnimations();
        await Shell.Current.GoToAsync($"{nameof(TransactionAddPage)}?type=DESPESA");
    }

    // Animações do FAB - Agora mostra verticalmente acima
    private async Task ToggleMenuAnimations()
    {
        if (_isMenuOpen)
        {
            BtnIncome.InputTransparent = false; BtnExpense.InputTransparent = false;
            BtnIncome.Opacity = 1; GridIncome.Opacity = 1;
            BtnExpense.Opacity = 1; GridExpense.Opacity = 1;

            await Task.WhenAll(
                BtnIncome.TranslateTo(0, -70, 250, Easing.CubicOut),
                GridIncome.TranslateTo(0, -70, 250, Easing.CubicOut),
                BtnExpense.TranslateTo(0, -140, 250, Easing.CubicOut),
                GridExpense.TranslateTo(0, -140, 250, Easing.CubicOut),
                FabButton.RotateTo(45, 250, Easing.CubicOut)
            );
        }
        else
        {
            await Task.WhenAll(
                BtnIncome.TranslateTo(0, 0, 250, Easing.CubicIn),
                GridIncome.TranslateTo(0, 0, 250, Easing.CubicIn),
                BtnExpense.TranslateTo(0, 0, 250, Easing.CubicIn),
                GridExpense.TranslateTo(0, 0, 250, Easing.CubicIn),
                FabButton.RotateTo(0, 250, Easing.CubicIn)
            );
            BtnIncome.Opacity = 0; GridIncome.Opacity = 0;
            BtnExpense.Opacity = 0; GridExpense.Opacity = 0;
            BtnIncome.InputTransparent = true; BtnExpense.InputTransparent = true;
        }
    }
}
