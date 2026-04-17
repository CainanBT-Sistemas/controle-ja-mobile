using CommunityToolkit.Mvvm.Messaging;
using controle_ja_mobile.Helpers;
using controle_ja_mobile.ViewModels;
using controle_ja_mobile.Views.Components;
using controle_ja_mobile.Views.Privates.Tabs;
using Microsoft.Maui.Controls;
using System.Collections.Generic;

namespace controle_ja_mobile.Views.Privates;

public partial class DashboardPage : ContentPage
{
    private readonly List<ContentView> _tabs;

    public DashboardPage(DashboardViewModel homeVm, CreditCardsViewModel cardsVm, VehiclesViewModel vehiclesVm, SettingsViewModel settingsVm)
    {
        InitializeComponent();

        // 1. Instancia as Views e injeta os ViewModels
        var homeView = new HomeView { BindingContext = homeVm };
        var transactionsView = new TransactionsListView();
        var cardsView = new CreditCardsView { BindingContext = cardsVm };
        var vehiclesView = new VehicleListView { BindingContext = vehiclesVm };
        var settingsView = new SettingsView { BindingContext = settingsVm };

        // 2. Guarda na lista
        _tabs = new List<ContentView>
        {
            homeView,
            transactionsView,
            cardsView,
            vehiclesView,
            settingsView
        };

        // 3. Adiciona todas no Grid (apenas a primeira visível)
        for (int i = 0; i < _tabs.Count; i++)
        {
            _tabs[i].IsVisible = (i == 0); // Só a Home (0) começa visível
            TabContainer.Children.Add(_tabs[i]);
        }

        // 4. Inscreve no Messenger UMA ÚNICA VEZ para trocar a aba instantaneamente
        WeakReferenceMessenger.Default.Register<NavigationMessage>(this, (recipient, message) =>
        {
            string targetPage = message.Value;

            switch (targetPage)
            {
                case "Home": ActivateTab(0, "Home"); break;
                case "Transactions": ActivateTab(1, "Transactions"); break;
                case "Cards": ActivateTab(2, "Cards"); break;
                case "Vehicles": ActivateTab(3, "Vehicles"); break;
                case "Settings": ActivateTab(4, "Settings"); break;
            }
        });
    }

    private void ActivateTab(int index, string menuName)
    {
        // 1. Esconde todas as abas
        foreach (var tab in _tabs)
        {
            tab.IsVisible = false;
        }

        // 2. Mostra só a aba clicada (Troca Instantânea, sem animação arrastada)
        if (index >= 0 && index < _tabs.Count)
        {
            _tabs[index].IsVisible = true;
        }

        // 3. Atualiza a cor do menu inferior
        if (MyBottomMenu != null)
        {
            MyBottomMenu.ActivePage = menuName;
        }
    }
}