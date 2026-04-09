using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using controle_ja_mobile.Models;
using controle_ja_mobile.Services;
using System.Collections.ObjectModel;
using System.Globalization;

namespace controle_ja_mobile.ViewModels
{
    public partial class TransactionsViewModel : BaseViewModel
    {
        private readonly TransactionService _transactionService;
        private readonly CultureInfo _culture = new CultureInfo("pt-BR");

        // Lista agrupada que o XAML consome
        public ObservableCollection<TransactionGroup> GroupedTransactions { get; } = new();

        [ObservableProperty] private DateTime currentDate = DateTime.Now;
        [ObservableProperty] private string currentMonthYear;

        // Totais formatados para o cabeçalho
        [ObservableProperty] private string totalInFormatted = "R$ 0,00";
        [ObservableProperty] private string totalOutFormatted = "R$ 0,00";
        [ObservableProperty] private string balanceFormatted = "R$ 0,00";
        [ObservableProperty] private Color balanceColor = Color.FromArgb("#3B82F6");

        // Controle de estados da tela
        [ObservableProperty] private bool isEmpty = true;
        [ObservableProperty] private bool hasTransactions = false;

        public TransactionsViewModel(TransactionService transactionService)
        {
            _transactionService = transactionService;
            UpdateMonthLabel();
            _ = LoadTransactions();
        }

        // --- COMANDOS DE NAVEGAÇÃO DE MÊS ---

        [RelayCommand]
        public async Task PreviousMonth()
        {
            CurrentDate = CurrentDate.AddMonths(-1);
            UpdateMonthLabel();
            await LoadTransactions();
        }

        [RelayCommand]
        public async Task NextMonth()
        {
            CurrentDate = CurrentDate.AddMonths(1);
            UpdateMonthLabel();
            await LoadTransactions();
        }

        private void UpdateMonthLabel()
        {
            // Deixa o mês em CapsLock elegante como no design
            CurrentMonthYear = CurrentDate.ToString("MMMM 'de' yyyy", _culture);
        }

        // --- CARREGAMENTO DE DADOS ---

        [RelayCommand]
        public async Task LoadTransactions()
        {
            IsLoading = true;
            try
            {
                var firstDay = new DateTime(CurrentDate.Year, CurrentDate.Month, 1);
                var lastDay = firstDay.AddMonths(1).AddSeconds(-1);

                long startUnix = new DateTimeOffset(firstDay).ToUnixTimeMilliseconds();
                long endUnix = new DateTimeOffset(lastDay).ToUnixTimeMilliseconds();

                var monthTransactions = await _transactionService.GetTransactionsAsync(startUnix, endUnix);

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    GroupedTransactions.Clear();

                    if (monthTransactions != null && monthTransactions.Count > 0)
                    {
                        // Ordena por data decrescente
                        var sortedTransactions = monthTransactions.OrderByDescending(t => t.Date).ToList();

                        // Agrupa por dia para o visual de "Timeline"
                        var grouped = sortedTransactions.GroupBy(t => t.DateTimeObject.Date)
                            .Select(g =>
                            {
                                string dayName = _culture.DateTimeFormat.GetAbbreviatedDayName(g.Key.DayOfWeek);
                                dayName = char.ToUpper(dayName[0]) + dayName.Substring(1);

                                // O segredo para remover o ponto da abreviação (Sáb. -> Sáb)
                                dayName = dayName.Replace(".", "");

                                string header = $"{dayName}, {g.Key:dd/MM/yyyy}";
                                return new TransactionGroup(header, g.ToList());
                            }).ToList();

                        foreach (var group in grouped)
                            GroupedTransactions.Add(group);
                    }

                    IsEmpty = GroupedTransactions.Count == 0;
                    HasTransactions = GroupedTransactions.Count > 0;
                    CalculateTotals(monthTransactions ?? new List<Transaction>());
                });
            }
            catch (Exception ex)
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                    await App.Current.MainPage.DisplayAlert("Erro", "Falha ao carregar: " + ex.Message, "OK"));
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void CalculateTotals(List<Transaction> flatTransactions)
        {
            decimal totalIn = flatTransactions.Where(t => t.Type == TransactionType.RECEITA || t.Type == TransactionType.TRANSFERENCIA_ENTRADA).Sum(t => t.Amount);
            decimal totalOut = flatTransactions.Where(t => t.Type == TransactionType.DESPESA || t.Type == TransactionType.TRANSFERENCIA_SAIDA).Sum(t => t.Amount);
            decimal balance = totalIn - totalOut;

            TotalInFormatted = totalIn.ToString("C", _culture);
            TotalOutFormatted = totalOut.ToString("C", _culture);
            BalanceFormatted = balance.ToString("C", _culture);

            // Muda a cor do balanço dinamicamente
            if (balance < 0) BalanceColor = Color.FromArgb("#FF5252"); // Vermelho
            else if (balance > 0) BalanceColor = Color.FromArgb("#00E676"); // Verde
            else BalanceColor = Color.FromArgb("#3B82F6"); // Azul
        }

        // --- COMANDOS FALTANTES QUE CAUSARAM O ERRO ---

        [RelayCommand]
        public async Task GoToFilters()
        {
            // Resolve o erro XFC0045
            await App.Current.MainPage.DisplayAlert("Filtros", "A tela de filtros será implementada na próxima etapa!", "OK");
        }

        [RelayCommand]
        public async Task EditTransaction(Transaction transactionToEdit)
        {
            var navParams = new Dictionary<string, object> { { "TransactionToEdit", transactionToEdit } };
            await Shell.Current.GoToAsync(nameof(Views.Privates.TransactionAddPage), navParams);
        }

        [RelayCommand]
        public async Task DeleteTransaction(Transaction transaction)
        {
            bool confirm = await App.Current.MainPage.DisplayAlert("Excluir", $"Deseja apagar '{transaction.Name}'?", "Sim", "Não");
            if (!confirm) return;

            var success = await _transactionService.DeleteTransactionAsync(transaction.Id.Value, false);
            if (success) await LoadTransactions();
        }
    }
}