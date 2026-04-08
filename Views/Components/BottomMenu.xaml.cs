using controle_ja_mobile.Views.Privates;
using Microsoft.Maui.Graphics;
using CommunityToolkit.Mvvm.Messaging;
using controle_ja_mobile.Helpers;

namespace controle_ja_mobile.Views.Components;

public partial class BottomMenu : ContentView
{
    private readonly Color ActiveColor = Color.FromArgb("#00E676");
    private readonly Color InactiveColor = Color.FromArgb("#64748B");

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

    private async void OnMainFabClicked(object sender, EventArgs e)
    {
        try
        {
            // Abre a tela de Transação com o parâmetro 'mode=new' para iniciar neutro
            await Shell.Current.GoToAsync($"{nameof(TransactionAddPage)}?mode=new");
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Ops!", $"Erro ao abrir tela: {ex.Message}", "OK");
        }
    }
}