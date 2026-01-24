using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using controle_ja_mobile.Models;
using controle_ja_mobile.Services;
using controle_ja_mobile.Views.Privates;
using Microcharts;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace controle_ja_mobile.ViewModels
{
    public partial class DashboardViewModel : ObservableObject
    {
        private readonly ApiService _apiService;
        private readonly CultureInfo _culture = new CultureInfo("pt-BR"); // Força Brasil
        
        [ObservableProperty] private DateTime currentDate;
        [ObservableProperty] private string currentMonthDisplay;
        [ObservableProperty] private string userName;
        [ObservableProperty] private string balance;
        [ObservableProperty] private string income;
        [ObservableProperty] private string expense;
        [ObservableProperty] private Chart expenseChart;
        [ObservableProperty] private bool isLoading;
        [ObservableProperty] private bool hasChartData;


        public DashboardViewModel(ApiService apiService)
        {
            _apiService = apiService;
            UserName = Preferences.Get("UserName", "Usuário");

            currentDate = DateTime.Now;
            UpdateMonthDisplay();
        }

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
            if (IsLoading) return;
            IsLoading = true;

            try
            {
                // Datas (Início e Fim do Mês)
                var now = DateTime.Now;
                var firstDay = new DateTime(now.Year, now.Month, 1);
                var lastDay = firstDay.AddMonths(1).AddSeconds(-1);

                long start = new DateTimeOffset(firstDay).ToUnixTimeMilliseconds();
                long end = new DateTimeOffset(lastDay).ToUnixTimeMilliseconds();

                // Chamadas API
                var tSummary = _apiService.GetAsync<FinancialSummary>($"dashboard/summary?start={start}&end={end}");
                var tChart = _apiService.GetAsync<List<ChartData>>($"dashboard/expenses-category?start={start}&end={end}");

                await Task.WhenAll(tSummary, tChart);

                var summary = await tSummary;
                var chartData = await tChart;

                // 1. Atualiza Cards (Forçando R$)
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

                // 2. Lógica do Gráfico vs Estado Vazio
                if (chartData != null && chartData.Any(x => x.Value > 0))
                {
                    hasChartData = true; // Mostra gráfico

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
                    HasChartData = false; // Mostra mensagem "Sem dados"
                    ExpenseChart = null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public async Task ShowAddOptions()
        {
            string action = await App.Current.MainPage.DisplayActionSheet("Novo Lançamento", "Cancelar", null, "Nova Despesa", "Nova Receita");

            if (action == "Nova Despesa")
            {
                // Navega passando o tipo via QueryParameter
                await Shell.Current.GoToAsync($"{nameof(TransactionAddPage)}?type=DESPESA");
            }
            else if (action == "Nova Receita")
            {
                await Shell.Current.GoToAsync($"{nameof(TransactionAddPage)}?type=RECEITA");
            }
        }
    }
}
