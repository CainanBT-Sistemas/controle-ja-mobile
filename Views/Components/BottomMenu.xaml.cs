using controle_ja_mobile.Views.Privates;
// NOVO: Adicionado import da sua nova pasta Popups!
using controle_ja_mobile.Views.Popups;
using Microsoft.Maui.Graphics;
using CommunityToolkit.Mvvm.Messaging;
using controle_ja_mobile.Helpers;
using controle_ja_mobile.ViewModels;
using CommunityToolkit.Maui.Views;

namespace controle_ja_mobile.Views.Components;

public partial class BottomMenu : ContentView
{
    private readonly Color ActiveColor = Color.FromArgb("#00E676");
    private readonly Color InactiveColor = Color.FromArgb("#64748B");

    private bool _isFabMenuOpen = false;

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

    private void UpdateVisualState()
    {
        if (IconHome == null) return;

        IconHome.TextColor = InactiveColor; LblHome.TextColor = InactiveColor;
        IconTransactions.TextColor = InactiveColor; LblTransactions.TextColor = InactiveColor;
        IconVehicles.TextColor = InactiveColor; LblVehicles.TextColor = InactiveColor;
        IconSettings.TextColor = InactiveColor; LblSettings.TextColor = InactiveColor;

        switch (ActivePage)
        {
            case "Home": IconHome.TextColor = ActiveColor; LblHome.TextColor = ActiveColor; break;
            case "Transactions": IconTransactions.TextColor = ActiveColor; LblTransactions.TextColor = ActiveColor; break;
            case "Vehicles": IconVehicles.TextColor = ActiveColor; LblVehicles.TextColor = ActiveColor; break;
            case "Settings": IconSettings.TextColor = ActiveColor; LblSettings.TextColor = ActiveColor; break;
        }
    }

    private void OnHomeClicked(object sender, EventArgs e) => WeakReferenceMessenger.Default.Send(new NavigationMessage("Home"));
    private void OnTransactionsClicked(object sender, EventArgs e) => WeakReferenceMessenger.Default.Send(new NavigationMessage("Transactions"));
    private void OnVehiclesClicked(object sender, EventArgs e) => WeakReferenceMessenger.Default.Send(new NavigationMessage("Vehicles"));
    private void OnSettingsClicked(object sender, EventArgs e) => WeakReferenceMessenger.Default.Send(new NavigationMessage("Settings"));

    private async void OnMainFabClicked(object sender, EventArgs e) => await ToggleFabMenu();

    private async void OnCloseFabMenuTapped(object sender, TappedEventArgs e)
    {
        if (_isFabMenuOpen) await ToggleFabMenu();
    }

    private async Task ToggleFabMenu()
    {
        _isFabMenuOpen = !_isFabMenuOpen;

        if (_isFabMenuOpen)
        {
            FabOverlay.IsVisible = true;
            FabMenuContainer.IsVisible = true;

            await Task.WhenAll(
                MainFabIcon.RotateTo(45, 150, Easing.CubicOut),
                FabOverlay.FadeTo(0.6, 150),
                FabMenuContainer.FadeTo(1, 150),
                FabMenuContainer.TranslateTo(0, 0, 150, Easing.CubicOut)
            );
        }
        else
        {
            await Task.WhenAll(
                MainFabIcon.RotateTo(0, 150, Easing.CubicIn),
                FabOverlay.FadeTo(0, 150),
                FabMenuContainer.FadeTo(0, 150),
                FabMenuContainer.TranslateTo(0, 20, 150, Easing.CubicIn)
            );

            FabOverlay.IsVisible = false;
            FabMenuContainer.IsVisible = false;
        }
    }

    private void ShowAddPopup(string type)
    {
        var vm = IPlatformApplication.Current.Services.GetService<TransactionAddViewModel>();
        if (vm != null)
        {
            var reflection = typeof(TransactionAddViewModel).GetField("_isInitialized", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            reflection?.SetValue(vm, false);

            vm.ApplyQueryAttributes(new Dictionary<string, object> { { "mode", "new" }, { "type", type } });

            // Instancia o popup referenciando a nova pasta "Popups"
            var popup = new TransactionAddPopup(vm);
            Shell.Current.ShowPopup(popup);
        }
    }

    private async void OnNewExpenseClicked(object sender, TappedEventArgs e)
    {
        await ToggleFabMenu();
        ShowAddPopup("Despesa");
    }

    private async void OnNewCreditCardExpenseClicked(object sender, TappedEventArgs e)
    {
        await ToggleFabMenu();
        ShowAddPopup("Despesa no Cartão");
    }

    private async void OnNewIncomeClicked(object sender, TappedEventArgs e)
    {
        await ToggleFabMenu();
        ShowAddPopup("Receita");
    }

    private async void OnNewTransferClicked(object sender, TappedEventArgs e)
    {
        await ToggleFabMenu();
        ShowAddPopup("Transferência");
    }
}