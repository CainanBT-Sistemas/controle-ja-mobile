using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using controle_ja_mobile.Models;
using controle_ja_mobile.Services;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace controle_ja_mobile.ViewModels
{
    public partial class TransactionAddViewModel : ObservableObject, IQueryAttributable
    {
        private readonly ApiService _apiService;

        public ObservableCollection<Category> Categories { get; } = new();

        public ObservableCollection<PaymentSource> PaymentSources { get; } = new();

        [ObservableProperty] private string pageTitle;
        [ObservableProperty] private string themeColor;

        [ObservableProperty] private string description;
        [ObservableProperty] private string amount;
        [ObservableProperty] private DateTime date = DateTime.Now;
        [ObservableProperty] private bool isPaid = true; 

        [ObservableProperty] private Category selectedCategory;

        [ObservableProperty]
        private PaymentSource selectedSource;

        [ObservableProperty] private bool showInstallmentOption; // Controla visibilidade
        [ObservableProperty] private bool isInstallment;
        [ObservableProperty] private string installmentCount = "1";

        [ObservableProperty] private bool isRecurring;
        [ObservableProperty] private bool isLoading;

        private TransactionType _currentType;

        public TransactionAddViewModel(ApiService apiService)
        {
            _apiService = apiService;
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

        private async Task<bool> SaveInternal()
        {
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
            string cleanAmount = Amount.Replace("R$", "").Trim(); // Remove R$ se vier
            if (!decimal.TryParse(cleanAmount, out decimal decimalAmount))
            {
                await App.Current.MainPage.DisplayAlert("Erro", "Valor inválido. Digite apenas números.", "OK");
                return false;
            }

            IsLoading = true;
            try
            {
                var transaction = new
                {
                    name = Description,
                    type = _currentType.ToString(),
                    amount = decimalAmount,
                    date = new DateTimeOffset(Date).ToUnixTimeMilliseconds(),
                    paid = IsPaid,
                    categoryId = SelectedCategory.Id,
                    accountId = SelectedSource.Type == "ACCOUNT" ? SelectedSource.Id : (Guid?)null,
                    creditCardId = SelectedSource.Type == "CREDIT_CARD" ? SelectedSource.Id : (Guid?)null,
                    installments = IsInstallment && int.TryParse(InstallmentCount, out int i) ? i : 1,
                    isRecurring = IsRecurring
                };

                var result = await _apiService.PostAsync<TransactionModel>("transactions", transaction);

                if (result == null)
                {
                    await App.Current.MainPage.DisplayAlert("Erro", "O servidor recusou o salvamento.", "OK");
                    return false;
                }

                return true;
            }catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Erro Crítico", ex.Message, "OK");
                return false;
            }
            finally { IsLoading = false; }
        }


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
            IsLoading = true;
            try
            {
                var tCats = _apiService.GetAsync<List<Category>>("categories");
                var tAccs = _apiService.GetAsync<List<Account>>("accounts");
                var tCards = _apiService.GetAsync<List<CreditCard>>("cards"); // Pega cartões também

                await Task.WhenAll(tCats, tAccs, tCards);

                var cats = await tCats;
                var accs = await tAccs;
                var cards = await tCards;

                // 1. Categorias
                Categories.Clear();
                if (cats != null)
                {
                    foreach (var c in cats.Where(x => x.Type == _currentType)) Categories.Add(c);
                }

                // 2. Fontes de Pagamento (Junta Contas + Cartões)
                PaymentSources.Clear();
                if (accs != null)
                {
                    foreach (var a in accs)
                        PaymentSources.Add(new PaymentSource { Id = a.Id, Name = a.Name, Type = "ACCOUNT" });
                }
                // Só mostra cartões se for DESPESA
                if (cards != null && _currentType == TransactionType.DESPESA)
                {
                    foreach (var c in cards)
                        PaymentSources.Add(new PaymentSource { Id = c.Id, Name = $"Cartão: {c.Name}", Type = "CREDIT_CARD" });
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public async Task AddNewCategory()
        {
            string result = await App.Current.MainPage.DisplayPromptAsync("Nova Categoria", "Nome da categoria:");
            if (!string.IsNullOrWhiteSpace(result))
            {
                IsLoading = true;
                var newCat = new Category { Name = result, Type = _currentType };
                var savedCat = await _apiService.PostAsync<Category>("categories", newCat);

                if (savedCat != null)
                {
                    Categories.Add(savedCat);
                    SelectedCategory = savedCat; // Já seleciona a nova
                }
                IsLoading = false;
            }
        }


        // Botão "Salvar e Sair"
        [RelayCommand]
        public async Task SaveAndClose()
        {
            bool success = await SaveInternal();
            if (success)
            {
                await Shell.Current.GoToAsync("..");
            }
        }

        [RelayCommand]
        public async Task Cancel() => await Shell.Current.GoToAsync("..");
    }

    // Classe auxiliar para o Dropdown misto
    public class PaymentSource
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; } // "ACCOUNT" ou "CREDIT_CARD"
    }
}
