using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using controle_ja_mobile.Services;
using controle_ja_mobile.Views.Publics;
using controle_ja_mobile.Models;
using Microcharts;
using SkiaSharp;
using System.Globalization;
using controle_ja_mobile.Views.Privates;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.Messaging;
using controle_ja_mobile.Helpers;

namespace controle_ja_mobile.ViewModels
{
    public partial class DashboardViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;
        private readonly DashboardService _dashboardService;
        private readonly TransactionService _transactionService;
        private readonly CultureInfo _culture = new CultureInfo("pt-BR");

        public ObservableCollection<ChartSlide> GeneralCharts { get; } = new();

        [ObservableProperty] private double generalChartHeight = 280;

        [ObservableProperty] private Chart creditExpenseChart;
        [ObservableProperty] private bool hasCreditChartData;
        [ObservableProperty] private string userName;
        [ObservableProperty] private DateTime currentDate;
        [ObservableProperty] private string currentMonthDisplay;

        [ObservableProperty] private bool isMenuOpen;
        [ObservableProperty] private bool isSettingsMenuVisible;
        [ObservableProperty] private string availableBalance;
        [ObservableProperty] private string projectedBalance;
        [ObservableProperty] private string projectedPayables;
        [ObservableProperty] private string projectedVariables;
        public ObservableCollection<Account> Accounts { get; } = new();
        public ObservableCollection<CreditCard> CreditCards { get; } = new();
        public ObservableCollection<DashboardAlert> OverduePayables { get; } = new();
        public ObservableCollection<DashboardAlert> OverdueInvoices { get; } = new();
        public ObservableCollection<DashboardAlert> PendingPayables { get; } = new();
        public ObservableCollection<DashboardAlert> PendingReceivables { get; } = new();
        public ObservableCollection<DashboardAlert> PendingInvoices { get; } = new();

        [ObservableProperty] private bool hasOverduePayables;
        [ObservableProperty] private bool hasOverdueInvoices;
        [ObservableProperty] private bool hasPendingPayables;
        [ObservableProperty] private bool hasPendingReceivables;
        [ObservableProperty] private bool hasPendingInvoices;

        [ObservableProperty] private string totalOverduePayables;
        [ObservableProperty] private string totalOverdueInvoices;
        [ObservableProperty] private string totalPendingPayables;
        [ObservableProperty] private string totalPendingReceivables;
        [ObservableProperty] private string totalPendingInvoices;

        [ObservableProperty] private bool hasAccounts;
        [ObservableProperty] private bool hasCreditCards;

        [ObservableProperty] private bool isAlertsPopupVisible;
        [ObservableProperty] private string alertsPopupTitle;
        public ObservableCollection<DashboardAlert> CurrentAlertsList { get; } = new();

        public DashboardViewModel(ApiService apiService, DashboardService dashboardService, TransactionService transactionService)
        {
            _apiService = apiService;
            _dashboardService = dashboardService;
            _transactionService = transactionService;

            UserName = Preferences.Get("UserName", "Usuário");
            CurrentDate = DateTime.Now;
            UpdateMonthDisplay();

            WeakReferenceMessenger.Default.Register<GlobalRefreshMessage>(this, (r, m) => {
                MainThread.BeginInvokeOnMainThread(() => {
                    _ = GoToHomeCommand.ExecuteAsync(null);
                });
            });
        }

        [RelayCommand]
        public void OpenAlerts(string type)
        {
            CurrentAlertsList.Clear();
            if (type == "OVERDUE_PAYABLES" && OverduePayables.Any()) { AlertsPopupTitle = "Contas Atrasadas"; foreach (var item in OverduePayables) CurrentAlertsList.Add(item); IsAlertsPopupVisible = true; }
            else if (type == "OVERDUE_INVOICES" && OverdueInvoices.Any()) { AlertsPopupTitle = "Faturas Vencidas"; foreach (var item in OverdueInvoices) CurrentAlertsList.Add(item); IsAlertsPopupVisible = true; }
            else if (type == "PAYABLES" && PendingPayables.Any()) { AlertsPopupTitle = "A Pagar (Mês)"; foreach (var item in PendingPayables) CurrentAlertsList.Add(item); IsAlertsPopupVisible = true; }
            else if (type == "RECEIVABLES" && PendingReceivables.Any()) { AlertsPopupTitle = "A Receber (Mês)"; foreach (var item in PendingReceivables) CurrentAlertsList.Add(item); IsAlertsPopupVisible = true; }
            else if (type == "INVOICES" && PendingInvoices.Any()) { AlertsPopupTitle = "Faturas Abertas"; foreach (var item in PendingInvoices) CurrentAlertsList.Add(item); IsAlertsPopupVisible = true; }
        }

        [RelayCommand] public void CloseAlerts() => IsAlertsPopupVisible = false;
        [RelayCommand] public void ToggleSettingsMenu() => IsSettingsMenuVisible = !IsSettingsMenuVisible;
        [RelayCommand] public async Task CloseSettingsMenu() { if (IsSettingsMenuVisible) IsSettingsMenuVisible = false; }
        [RelayCommand] public async Task PerformLogout() { IsSettingsMenuVisible = false; bool confirm = await Shell.Current.DisplayAlert("Sair", "Tem certeza que deseja desconectar?", "Sim", "Não"); if (!confirm) return; Preferences.Remove("AuthToken"); Preferences.Remove("UserName"); var loginPage = IPlatformApplication.Current.Services.GetService<LoginPage>(); Application.Current.MainPage = new NavigationPage(loginPage); }
        [RelayCommand] public async Task GoToProfile() { IsSettingsMenuVisible = false; await Shell.Current.DisplayAlert("Meu Perfil", "A edição de perfil estará disponível em breve.", "OK"); }

        [RelayCommand]
        public async Task GoToHome()
        {
            IsLoading = true;
            try
            {
                await Task.WhenAll(LoadSummaryAsync(), LoadChartAsync());
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void UpdateMonthDisplay()
        {
            CurrentMonthDisplay = CurrentDate.ToString("MMMM 'de' yyyy", _culture);
        }

        [RelayCommand]
        public async Task NextMonth()
        {
            CurrentDate = CurrentDate.AddMonths(1);
            UpdateMonthDisplay();
            await GoToHomeCommand.ExecuteAsync(null);
        }

        [RelayCommand]
        public async Task PreviousMonth()
        {
            CurrentDate = CurrentDate.AddMonths(-1);
            UpdateMonthDisplay();
            await GoToHomeCommand.ExecuteAsync(null);
        }

        private async Task LoadSummaryAsync()
        {
            var realNow = DateTime.Now;
            var firstDay = new DateTime(realNow.Year, realNow.Month, 1);
            var lastDay = firstDay.AddMonths(1).AddSeconds(-1);

            long start = new DateTimeOffset(firstDay).ToUnixTimeMilliseconds();
            long end = new DateTimeOffset(lastDay).ToUnixTimeMilliseconds();

            var summary = await _dashboardService.GetFullSummaryAsync(start, end);
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (summary != null)
                {
                    AvailableBalance = summary.AvailableBalance.ToString("C", _culture);
                    ProjectedBalance = summary.ProjectedBalance.ToString("C", _culture);
                    ProjectedPayables = summary.ProjectedPayables.ToString("C", _culture);
                    ProjectedVariables = summary.ProjectedVariables.ToString("C", _culture);

                    Accounts.Clear();
                    if (summary.Accounts != null) foreach (var acc in summary.Accounts) Accounts.Add(acc);
                    HasAccounts = Accounts.Any();

                    CreditCards.Clear();
                    if (summary.CreditCards != null) foreach (var card in summary.CreditCards) CreditCards.Add(card);
                    HasCreditCards = CreditCards.Any();

                    OverduePayables.Clear(); foreach (var item in summary.OverduePayables ?? new()) OverduePayables.Add(item);
                    OverdueInvoices.Clear(); foreach (var item in summary.OverdueInvoices ?? new()) OverdueInvoices.Add(item);
                    PendingPayables.Clear(); foreach (var item in summary.PendingPayables ?? new()) PendingPayables.Add(item);
                    PendingReceivables.Clear(); foreach (var item in summary.PendingReceivables ?? new()) PendingReceivables.Add(item);
                    PendingInvoices.Clear(); foreach (var item in summary.PendingInvoices ?? new()) PendingInvoices.Add(item);

                    HasOverduePayables = OverduePayables.Any(); TotalOverduePayables = OverduePayables.Sum(x => x.Amount).ToString("C", _culture);
                    HasOverdueInvoices = OverdueInvoices.Any(); TotalOverdueInvoices = OverdueInvoices.Sum(x => x.Amount).ToString("C", _culture);
                    HasPendingPayables = PendingPayables.Any(); TotalPendingPayables = PendingPayables.Sum(x => x.Amount).ToString("C", _culture);
                    HasPendingReceivables = PendingReceivables.Any(); TotalPendingReceivables = PendingReceivables.Sum(x => x.Amount).ToString("C", _culture);
                    HasPendingInvoices = PendingInvoices.Any(); TotalPendingInvoices = PendingInvoices.Sum(x => x.Amount).ToString("C", _culture);
                }
            });
        }

        private async Task LoadChartAsync()
        {
            var firstDay = new DateTime(CurrentDate.Year, CurrentDate.Month, 1);
            var lastDay = firstDay.AddMonths(1).AddSeconds(-1);

            long start = new DateTimeOffset(firstDay).ToUnixTimeMilliseconds();
            long end = new DateTimeOffset(lastDay).ToUnixTimeMilliseconds();

            var generalDataTask = _dashboardService.getExpensesByCategory(start, end);
            var incomeDataTask = _dashboardService.getIncomesByCategory(start, end);
            var creditDataTask = _dashboardService.getCreditExpensesByCategory(start, end);

            await Task.WhenAll(generalDataTask, incomeDataTask, creditDataTask);

            var generalData = await generalDataTask;
            var incomeData = await incomeDataTask;
            var creditData = await creditDataTask;

            MainThread.BeginInvokeOnMainThread(() =>
            {
                GeneralCharts.Clear();
                CreditExpenseChart = null;
            });

            await Task.Delay(100);

            MainThread.BeginInvokeOnMainThread(() =>
            {
                var backupColors = new[] { "#2979FF", "#FFAB00", "#00E676", "#E040FB", "#FF5252", "#00BCD4", "#FF9800" };

                bool hasExpense = generalData != null && generalData.Any(x => x.Value > 0);
                bool hasIncome = incomeData != null && incomeData.Any(x => x.Value > 0);

                GeneralChartHeight = (hasExpense || hasIncome) ? 280 : 120;

                SKColor labelColor = SKColor.Parse("#CBD5E1");
                SKColor valueLabelColor = SKColor.Parse("#FFFFFF");

                Chart expenseChart = null;
                if (hasExpense)
                {
                    int i = 0;
                    var entries = generalData.Select(item => {
                        string hexColor = !string.IsNullOrEmpty(item.Color) ? item.Color : backupColors[i % backupColors.Length];
                        i++;
                        return new ChartEntry((float)item.Value)
                        {
                            Label = item.Label,
                            ValueLabel = $"R$ {item.Value.ToString("N2", _culture)}",
                            Color = SKColor.Parse(hexColor),
                            ValueLabelColor = valueLabelColor
                        };
                    }).ToList();
                    expenseChart = new DonutChart { Entries = entries, BackgroundColor = SKColors.Transparent, LabelTextSize = 36, HoleRadius = 0.65f, LabelColor = labelColor };
                }

                Chart incomeChart = null;
                if (hasIncome)
                {
                    int i = 0;
                    var entries = incomeData.Select(item => {
                        string hexColor = !string.IsNullOrEmpty(item.Color) ? item.Color : backupColors[i % backupColors.Length];
                        i++;
                        return new ChartEntry((float)item.Value)
                        {
                            Label = item.Label,
                            ValueLabel = $"R$ {item.Value.ToString("N2", _culture)}",
                            Color = SKColor.Parse(hexColor),
                            ValueLabelColor = valueLabelColor
                        };
                    }).ToList();
                    incomeChart = new DonutChart { Entries = entries, BackgroundColor = SKColors.Transparent, LabelTextSize = 36, HoleRadius = 0.65f, LabelColor = labelColor };
                }

                GeneralCharts.Add(new ChartSlide { Title = "Despesas Gerais (Conta e Carteira)", TitleColor = Color.FromArgb("#94A3B8"), ChartObj = expenseChart, HasData = hasExpense, EmptyMessage = "Nenhum gasto neste mês." });
                GeneralCharts.Add(new ChartSlide { Title = "Receitas (Entradas)", TitleColor = Color.FromArgb("#94A3B8"), ChartObj = incomeChart, HasData = hasIncome, EmptyMessage = "Nenhuma receita neste mês." });

                if (creditData != null && creditData.Any(x => x.Value > 0))
                {
                    HasCreditChartData = true;
                    int i = 0;
                    var entries = creditData.Select(item => {
                        string hexColor = !string.IsNullOrEmpty(item.Color) ? item.Color : backupColors[i % backupColors.Length];
                        i++;
                        return new ChartEntry((float)item.Value)
                        {
                            Label = item.Label,
                            ValueLabel = $"R$ {item.Value.ToString("N2", _culture)}",
                            Color = SKColor.Parse(hexColor),
                            ValueLabelColor = valueLabelColor
                        };
                    }).ToList();
                    CreditExpenseChart = new DonutChart { Entries = entries, BackgroundColor = SKColors.Transparent, LabelTextSize = 36, HoleRadius = 0.65f, LabelColor = labelColor };
                }
                else { HasCreditChartData = false; CreditExpenseChart = null; }
            });
        }

        [RelayCommand]
        public async Task OpenInvoiceDetails(CreditCard card)
        {
            if (card == null) return;
            await NavigateToAsync($"{nameof(InvoiceDetailsPage)}?cardId={card.Id}");
        }

        [RelayCommand]
        public async Task QuickPay(DashboardAlert alert)
        {
            if (alert == null) return;
            if (alert.Type == "FATURA")
            {
                await App.Current.MainPage.DisplayAlert("Pagamento de Fatura", "Para pagar a fatura, feche este menu, clique no botão (+) verde e escolha 'Pagamento de Fatura' para informar de qual conta o dinheiro saiu.", "Entendi");
                return;
            }

            bool confirm = await App.Current.MainPage.DisplayAlert("Baixa Rápida", $"Deseja marcar '{alert.Description}' como pago?", "Sim", "Não");
            if (!confirm) return;

            await ExecuteWithErrorHandlingAsync(async () =>
            {
                var json = await _apiService.GetAsync<string>($"transactions/{alert.Id}");
                if (!string.IsNullOrWhiteSpace(json))
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    options.Converters.Add(new JsonStringEnumConverter());
                    var tx = JsonSerializer.Deserialize<Transaction>(json, options);

                    if (tx != null)
                    {
                        tx.Paid = true;
                        bool success = await _transactionService.UpdateTransactionAsync(tx.Id.Value, tx, false);
                        if (success)
                        {
                            CurrentAlertsList.Remove(alert);
                            if (CurrentAlertsList.Count == 0) CloseAlerts();

                            WeakReferenceMessenger.Default.Send(new GlobalRefreshMessage());
                            await App.Current.MainPage.DisplayAlert("Sucesso", "Transação marcada como paga!", "OK");
                        }
                    }
                }
            });
        }
    }

    public class ChartSlide
    {
        public string Title { get; set; }
        public Color TitleColor { get; set; }
        public Chart ChartObj { get; set; }
        public bool HasData { get; set; }
        public string EmptyMessage { get; set; }
    }
}