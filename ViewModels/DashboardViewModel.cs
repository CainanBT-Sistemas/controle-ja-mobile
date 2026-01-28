using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using controle_ja_mobile.Models;
using controle_ja_mobile.Services;
using controle_ja_mobile.Views.Privates;
using Microcharts;
using SkiaSharp;
using System.Globalization;

namespace controle_ja_mobile.ViewModels
{
    public partial class DashboardViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;
        private readonly CultureInfo _culture = new CultureInfo("pt-BR");

        [ObservableProperty] private DateTime currentDate;
        [ObservableProperty] private string currentMonthDisplay;
        [ObservableProperty] private string userName;

        [ObservableProperty] private string balance;
        [ObservableProperty] private string income;
        [ObservableProperty] private string expense;

        [ObservableProperty] private Chart expenseChart;
        [ObservableProperty] private bool hasChartData;

        [ObservableProperty] private bool isMenuOpen;

        public DashboardViewModel(ApiService apiService)
        {
            _apiService = apiService;
            UserName = Preferences.Get("UserName", "Usuário");
            CurrentDate = DateTime.Now;
            UpdateMonthDisplay();
        }

        [RelayCommand]
        public void ToggleMenu() => IsMenuOpen = !IsMenuOpen;

        [RelayCommand]
        public async Task GoToNewIncome()
        {
            IsMenuOpen = false;
            await Shell.Current.GoToAsync($"{nameof(TransactionAddPage)}?type=RECEITA");
        }

        [RelayCommand]
        public async Task GoToNewExpense()
        {
            IsMenuOpen = false;
            await Shell.Current.GoToAsync($"{nameof(TransactionAddPage)}?type=DESPESA");
        }

        [RelayCommand]
        public async Task GoToHome() => await LoadData();

        [RelayCommand]
        public async Task GoToTransactions() => await Shell.Current.DisplayAlert("Em Breve", "Extrato será implementado aqui!", "OK");

        [RelayCommand]
        public async Task GoToCards() => await Shell.Current.DisplayAlert("Em Breve", "Gestão de Cartões será aqui!", "OK");

        [RelayCommand]
        public async Task GoToCar() => await Shell.Current.DisplayAlert("Em Breve", "Gestão de Veículos será aqui!", "OK");

        [RelayCommand]
        public async Task GoToSettings() => await Shell.Current.DisplayAlert("Configurações", "Tela de perfil e conta.", "OK");

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
            await ExecuteAsync(async () =>
            {
                var now = CurrentDate;
                var firstDay = new DateTime(now.Year, now.Month, 1);
                var lastDay = firstDay.AddMonths(1).AddSeconds(-1);

                long start = new DateTimeOffset(firstDay).ToUnixTimeMilliseconds();
                long end = new DateTimeOffset(lastDay).ToUnixTimeMilliseconds();

                // Dispara as duas tarefas em paralelo
                var tSummary = _apiService.GetAsync<FinancialSummary>($"dashboard/summary?start={start}&end={end}");
                var tChart = _apiService.GetAsync<List<ChartData>>($"dashboard/expenses-category?start={start}&end={end}");

                await Task.WhenAll(tSummary, tChart);

                var summary = await tSummary;
                var chartData = await tChart;

                // Atualiza tela com dados
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