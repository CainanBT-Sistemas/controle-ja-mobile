using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using controle_ja_mobile.Services;
using controle_ja_mobile.Views.Publics;
using controle_ja_mobile.Models;
using Microcharts;
using SkiaSharp;
using System.Globalization;
using controle_ja_mobile.Views.Privates;

namespace controle_ja_mobile.ViewModels
{
    public partial class DashboardViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;
        private readonly DashboardService _dashboardService;
        private readonly CultureInfo _culture = new CultureInfo("pt-BR");

        [ObservableProperty] private string balance;
        [ObservableProperty] private string income;
        [ObservableProperty] private string expense;
        [ObservableProperty] private Chart expenseChart;
        [ObservableProperty] private bool hasChartData;
        [ObservableProperty] private string userName;
        [ObservableProperty] private DateTime currentDate;
        [ObservableProperty] private string currentMonthDisplay;
        [ObservableProperty] private bool isMenuOpen; // Menu inferior

        // === CONTROLE DO MENU SUPERIOR (POPUP) ===
        [ObservableProperty]
        private bool isSettingsMenuVisible;

        public DashboardViewModel(ApiService apiService, DashboardService dashboardService)
        {
            _apiService = apiService;
            _dashboardService = dashboardService;
            UserName = Preferences.Get("UserName", "Usuário");
            CurrentDate = DateTime.Now;
            UpdateMonthDisplay();
        }

        // 1. ABRIR/FECHAR PELO BOTÃO DA ENGRENAGEM
        [RelayCommand]
        public void ToggleSettingsMenu()
        {
            IsSettingsMenuVisible = !IsSettingsMenuVisible;
        }

        // 2. FECHAR AO CLICAR FORA (Com Feedback)
        [RelayCommand]
        public async Task CloseSettingsMenu()
        {
            if (IsSettingsMenuVisible)
            {
                IsSettingsMenuVisible = false;
                // Feedback visual que você pediu
                await Shell.Current.DisplayAlert("Feedback", "Você clicou fora! O menu foi fechado.", "OK");
            }
        }

        // 3. AÇÃO: LOGOUT (Com Confirmação)
        [RelayCommand]
        public async Task PerformLogout()
        {
            // Fecha o menu
            IsSettingsMenuVisible = false;

            // Pergunta de confirmação antes de sair
            bool confirm = await Shell.Current.DisplayAlert("Sair", "Tem certeza que deseja desconectar da sua conta?", "Sim", "Não");
            if (!confirm) return;

            // Limpa os dados de sessão
            Preferences.Remove("AuthToken");
            Preferences.Remove("UserName");

            // Redireciona para a tela de Login
            var loginPage = IPlatformApplication.Current.Services.GetService<LoginPage>();
            Application.Current.MainPage = new NavigationPage(loginPage);
        }

        // 4. AÇÃO: PERFIL (Com Feedback)
        [RelayCommand]
        public async Task GoToProfile()
        {
            IsSettingsMenuVisible = false;
            // Feedback do clique e ação
            await Shell.Current.DisplayAlert("Meu Perfil", "A edição de perfil estará disponível em breve.", "OK");
        }

        // --- OUTROS COMANDOS DO SISTEMA ---
        [RelayCommand] public void ToggleMenu() => IsMenuOpen = !IsMenuOpen;
        [RelayCommand] public async Task GoToNewIncome() { IsMenuOpen = false; await Shell.Current.GoToAsync($"{nameof(TransactionAddPage)}?type=RECEITA"); }
        [RelayCommand] public async Task GoToNewExpense() { IsMenuOpen = false; await Shell.Current.GoToAsync($"{nameof(TransactionAddPage)}?type=DESPESA"); }
        [RelayCommand] public async Task GoToHome() => await LoadData();
        [RelayCommand] public async Task GoToTransactions() => await Shell.Current.DisplayAlert("Em Breve", "Extrato será implementado aqui!", "OK");
        [RelayCommand] public async Task GoToCards() => await Shell.Current.DisplayAlert("Em Breve", "Gestão de Cartões será aqui!", "OK");
        [RelayCommand] public async Task GoToCar() => await Shell.Current.DisplayAlert("Em Breve", "Gestão de Veículos será aqui!", "OK");


        // --- DATA E LOAD DATA ---
        private void UpdateMonthDisplay()
        {
            CurrentMonthDisplay = CurrentDate.ToString("MMMM 'de' yyyy", _culture);
            Task.Run(LoadData);
        }

        [RelayCommand]
        public void NextMonth()
        {
            CurrentDate = CurrentDate.AddMonths(1);
            UpdateMonthDisplay();
        }

        [RelayCommand]
        public void PreviousMonth()
        {
            CurrentDate = CurrentDate.AddMonths(-1);
            UpdateMonthDisplay();
        }

        [RelayCommand]
        public async Task LoadData()
        {
            await ExecuteWithErrorHandlingAsync(async () =>
            {
                var now = CurrentDate;
                var firstDay = new DateTime(now.Year, now.Month, 1);
                var lastDay = firstDay.AddMonths(1).AddSeconds(-1);

                long start = new DateTimeOffset(firstDay).ToUnixTimeMilliseconds();
                long end = new DateTimeOffset(lastDay).ToUnixTimeMilliseconds();

                var tSummary = _dashboardService.getDashboardSummary(start, end);
                var tChart = _dashboardService.getExpensesByCategory(start, end);

                await Task.WhenAll(tSummary, tChart);

                var summary = await tSummary;
                var chartData = await tChart;

                if (summary != null)
                {
                    Balance = summary.Balance.ToString("C", _culture);
                    Income = summary.TotalIncome.ToString("C", _culture);
                    Expense = summary.TotalExpense.ToString("C", _culture);
                }
                else
                {
                    Balance = 0.ToString("C", _culture);
                    Income = 0.ToString("C", _culture);
                    Expense = 0.ToString("C", _culture);
                }

                if (chartData != null && chartData.Any(x => x.Value > 0))
                {
                    HasChartData = true;
                    var entries = new List<ChartEntry>();
                    var colors = new[] { "#00E676", "#2979FF", "#FFAB00", "#FF5252", "#E040FB" };
                    int i = 0;
                    foreach (var item in chartData)
                    {
                        entries.Add(new ChartEntry((float)item.Value)
                        {
                            Label = item.Label,
                            ValueLabel = item.Value.ToString("N0", _culture),
                            Color = SKColor.Parse(colors[i % colors.Length]),
                            ValueLabelColor = SKColor.Parse(colors[i % colors.Length])
                        });
                        i++;
                    }

                    ExpenseChart = new DonutChart
                    {
                        Entries = entries,
                        BackgroundColor = SKColors.Transparent,
                        LabelTextSize = 30,
                        HoleRadius = 0.70f,
                        LabelColor = SKColor.Parse("#94A3B8")
                    };
                }
                else
                {
                    HasChartData = false;
                    ExpenseChart = null;
                }
            });
        }
    }
}