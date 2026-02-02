using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using controle_ja_mobile.Models;
using controle_ja_mobile.Services;
using System.Collections.ObjectModel;

namespace controle_ja_mobile.ViewModels
{
    public partial class TransactionAddViewModel : BaseViewModel, IQueryAttributable
    {
        private readonly TransactionService _transactionService;
        private readonly CategoryService _categoryService;
        private readonly AccountService _accountService;
        private readonly CreditCardService _creditCardService;
        public ObservableCollection<Category> Categories { get; } = new();
        public ObservableCollection<PaymentSource> PaymentSources { get; } = new();

        [ObservableProperty] private string pageTitle;
        [ObservableProperty] private string themeColor;
        [ObservableProperty] private string description;
        [ObservableProperty] private string amount;
        [ObservableProperty] private DateTime date = DateTime.Now;
        [ObservableProperty] private bool isPaid = true;

        [ObservableProperty] private Category selectedCategory;
        [ObservableProperty] private PaymentSource selectedSource;

        [ObservableProperty] private bool showInstallmentOption;
        [ObservableProperty] private bool isInstallment;
        [ObservableProperty] private string installmentCount = "1";
        [ObservableProperty] private bool isRecurring;

        private TransactionType _currentType;

        public TransactionAddViewModel(TransactionService transactionService)
        {
            _transactionService = transactionService;
        }


        [RelayCommand]
        public async Task SaveAndContinue()
        {
            bool success = await SaveInternal();
            if (success)
            {
                await App.Current.MainPage.DisplayAlert("Salvo!", "Lançamento registrado. Pode inserir o próximo.", "OK");
                Amount = string.Empty;
                Description = string.Empty;
            }
        }

        [RelayCommand]
        public async Task SaveAndClose()
        {
            bool success = await SaveInternal();
            if (success)
            {
                await Shell.Current.GoToAsync("..");
            }
        }

        private async Task<bool> SaveInternal()
        {
            // Validações continuam fora para feedback rápido
            if (string.IsNullOrWhiteSpace(Description) || string.IsNullOrWhiteSpace(Amount))
            {
                await App.Current.MainPage.DisplayAlert("Atenção", "Preencha a Descrição e o Valor.", "OK");
                return false;
            }

            if (SelectedCategory == null || SelectedSource == null)
            {
                await App.Current.MainPage.DisplayAlert("Atenção", "Selecione a Categoria e a Conta/Cartão.", "OK");
                return false;
            }

            string cleanAmount = Amount.Replace("R$", "").Trim();
            if (!decimal.TryParse(cleanAmount, out decimal decimalAmount))
            {
                await App.Current.MainPage.DisplayAlert("Erro", "Valor inválido. Digite apenas números.", "OK");
                return false;
            }

            bool savedSuccessfully = false;

            // Envolve a chamada da API com nossa proteção de erros
            await ExecuteWithErrorHandlingAsync(async () =>
            {
                Transaction transaction = new Transaction();
                transaction.Name = description;
                transaction.Type = _currentType;
                transaction.Amount = decimalAmount;
                transaction.Date = new DateTimeOffset(Date).ToUnixTimeMilliseconds();
                transaction.Paid = IsPaid;
                transaction.CategoryId = SelectedCategory.Id;
                transaction.AccountId = SelectedSource.Type == "ACCOUNT" ? SelectedSource.Id : Guid.Empty;
                transaction.CreditCardId = SelectedSource.Type == "CREDIT_CARD" ? SelectedSource.Id : (Guid?)null;
                transaction.Installments = IsInstallment && int.TryParse(InstallmentCount, out int i) ? i : 1;
                transaction.IsRecurring = IsRecurring;

                bool success = await _transactionService.SaveTransactionAsync(transaction);

                if (success == null)
                {
                    // Lançamos exceção para o ExecuteAsync pegar e mostrar a mensagem amigável de erro genérico
                    throw new Exception("O servidor não retornou dados.");
                }

                savedSuccessfully = true;
            });

            return savedSuccessfully;
        }

        [RelayCommand]
        public async Task Cancel() => await Shell.Current.GoToAsync("..");

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("type"))
            {
                var typeString = query["type"].ToString();
                if (Enum.TryParse(typeString, out TransactionType parsedType))
                {
                    _currentType = parsedType;
                    SetupScreen();
                }
            }
        }

        private async void SetupScreen()
        {
            if (_currentType == TransactionType.DESPESA)
            {
                PageTitle = "Nova Despesa";
                ThemeColor = "#FF5252";
            }
            else
            {
                PageTitle = "Nova Receita";
                ThemeColor = "#00E676";
            }
            await LoadDependencies();
        }

        private async Task LoadDependencies()
        {
            await ExecuteWithErrorHandlingAsync(async () =>
            {
                var tCats =  _categoryService.GetCategoriesAsync();
                var tAccs =  _accountService.GetAccountsAsync();
                var tCards = _creditCardService.GetCreditCardsAsync();

                await Task.WhenAll(tCats, tAccs, tCards);

                var cats = await tCats;
                var accs = await tAccs;
                var cards = await tCards;

                Categories.Clear();
                if (cats != null)
                {
                    foreach (var c in cats.Where(x => x.Type == _currentType)) Categories.Add(c);
                }

                PaymentSources.Clear();
                if (accs != null)
                {
                    foreach (var a in accs)
                        PaymentSources.Add(new PaymentSource { Id = a.Id, Name = a.Name, Type = "ACCOUNT" });
                }

                if (cards != null && _currentType == TransactionType.DESPESA)
                {
                    foreach (var c in cards)
                        PaymentSources.Add(new PaymentSource { Id = c.Id, Name = $"Cartão: {c.Name}", Type = "CREDIT_CARD" });
                }
            });
        }

        [RelayCommand]
        public async Task AddNewCategory()
        {
            string result = await App.Current.MainPage.DisplayPromptAsync("Nova Categoria", "Nome da categoria:");
            if (!string.IsNullOrWhiteSpace(result))
            {
                await ExecuteWithErrorHandlingAsync(async () =>
                {
                    var newCat = new Category { Name = result, Type = _currentType };
                    var savedCat = await _categoryService.SaveCategoryAsync(newCat);
                    if (savedCat != null)
                    {
                        Categories.Add(savedCat);
                        SelectedCategory = savedCat;
                    }
                });
            }
        }

        partial void OnSelectedCategoryChanged(Category value)
        {
            if (value != null && !string.IsNullOrEmpty(value.Color))
            {
                ThemeColor = value.Color;
            }
        }

        partial void OnSelectedSourceChanged(PaymentSource value)
        {
            if (value != null && value.Type == "CREDIT_CARD" && _currentType == TransactionType.DESPESA)
            {
                ShowInstallmentOption = true;
                IsPaid = false;
            }
            else
            {
                ShowInstallmentOption = false;
                IsInstallment = false;
            }
        }
    }

    public class PaymentSource
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; } // "ACCOUNT" ou "CREDIT_CARD"
    }
}