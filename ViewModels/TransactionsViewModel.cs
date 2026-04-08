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

        public ObservableCollection<Transaction> Transactions { get; } = new();

        [ObservableProperty] private DateTime currentDate = DateTime.Now;
        [ObservableProperty] private string currentMonthYear;

        [ObservableProperty] private string totalInFormatted = "R$ 0,00";
        [ObservableProperty] private string totalOutFormatted = "R$ 0,00";
        [ObservableProperty] private string balanceFormatted = "R$ 0,00";
        [ObservableProperty] private Color balanceColor = Color.FromArgb("#3B82F6");

        [ObservableProperty] private bool isEmpty = true;
        [ObservableProperty] private bool hasTransactions = false;

        public TransactionsViewModel(TransactionService transactionService)
        {
            _transactionService = transactionService;
            UpdateMonthLabel();
            _ = LoadTransactions();
        }

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
            CurrentMonthYear = CurrentDate.ToString("MMMM 'de' yyyy", _culture);
        }

        [RelayCommand]
        public async Task LoadTransactions()
        {
            IsLoading = true;
            try
            {
                // 1. Calcula o início e o fim do mês selecionado na tela
                var firstDay = new DateTime(CurrentDate.Year, CurrentDate.Month, 1);
                var lastDay = firstDay.AddMonths(1).AddSeconds(-1);

                long startUnix = new DateTimeOffset(firstDay).ToUnixTimeMilliseconds();
                long endUnix = new DateTimeOffset(lastDay).ToUnixTimeMilliseconds();

                // 2. Manda a API buscar SÓ o que importa
                var monthTransactions = await _transactionService.GetTransactionsAsync(startUnix, endUnix);

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Transactions.Clear();

                    if (monthTransactions != null && monthTransactions.Count > 0)
                    {
                        foreach (var t in monthTransactions)
                        {
                            Transactions.Add(t);
                        }
                    }

                    IsEmpty = Transactions.Count == 0;
                    HasTransactions = Transactions.Count > 0;

                    CalculateTotals();
                });
            }
            catch (Exception ex)
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await App.Current.MainPage.DisplayAlert("Erro", "Erro ao carregar: " + ex.Message, "OK");
                });
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void CalculateTotals()
        {
            decimal totalIn = Transactions.Where(t => t.Type == TransactionType.RECEITA).Sum(t => t.Amount);
            decimal totalOut = Transactions.Where(t => t.Type == TransactionType.DESPESA).Sum(t => t.Amount);
            decimal balance = totalIn - totalOut;

            TotalInFormatted = totalIn.ToString("C", _culture);
            TotalOutFormatted = totalOut.ToString("C", _culture);
            BalanceFormatted = balance.ToString("C", _culture);

            if (balance < 0) BalanceColor = Color.FromArgb("#FF5252");
            else if (balance > 0) BalanceColor = Color.FromArgb("#00E676");
            else BalanceColor = Color.FromArgb("#3B82F6");
        }

        [RelayCommand]
        public async Task GoToFilters()
        {
            await App.Current.MainPage.DisplayAlert("Filtros", "Página de filtros em breve!", "OK");
        }

        // COMANDO DE EDIÇÃO ADICIONADO AQUI
        [RelayCommand]
        public async Task EditTransaction(Transaction transactionToEdit)
        {
            var navParams = new Dictionary<string, object>
            {
                { "TransactionToEdit", transactionToEdit }
            };
            await Shell.Current.GoToAsync(nameof(Views.Privates.TransactionAddPage), navParams);
        }
    }
}