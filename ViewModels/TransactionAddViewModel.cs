using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using controle_ja_mobile.Helpers;
using controle_ja_mobile.Models;
using controle_ja_mobile.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.Collections.Generic;

namespace controle_ja_mobile.ViewModels
{
    public partial class TransactionAddViewModel : BaseViewModel, IQueryAttributable
    {
        private readonly TransactionService _transactionService;
        private readonly CategoryService _categoryService;
        private readonly AccountService _accountService;
        private readonly CreditCardService _creditCardService;

        private Category _systemTransferCategory;

        // --- TRAVA DE NAVEGAÇÃO: Evita que a tela resete ao voltar de uma seleção ---
        private bool _isInitialized = false;

        public ObservableCollection<PaymentSource> PaymentSources { get; } = new();
        public ObservableCollection<Category> Categories { get; } = new();

        public ObservableCollection<string> TransactionTypes { get; } = new() { "Despesa", "Receita", "Transferência" };

        [ObservableProperty] private string selectedTransactionType;
        partial void OnSelectedTransactionTypeChanged(string value)
        {
            if (string.IsNullOrEmpty(value)) return;

            if (value == "Despesa") _currentType = TransactionType.DESPESA;
            else if (value == "Receita") _currentType = TransactionType.RECEITA;
            else if (value == "Transferência") _currentType = TransactionType.TRANSFERENCIA;

            // Recalcula o visual e revela o resto da tela
            SetupScreenVisuals();
            UpdatePaymentSourcesList();
            UpdateCategoriesList();
        }

        [ObservableProperty] private string amountTitle = "Novo Lançamento";
        [ObservableProperty] private string semanticColor = "#3F3F46";
        [ObservableProperty] private string sourceAccountTitle = "Conta / Cartão";
        [ObservableProperty] private string description;
        [ObservableProperty] private string amount;
        [ObservableProperty] private DateTime date = DateTime.Today;
        [ObservableProperty] private bool isPaid = true;
        [ObservableProperty] private string paidLabelText = "Status";
        [ObservableProperty] private bool showMoreDetails = false;

        [ObservableProperty] private bool isFixed;
        [ObservableProperty] private bool isRecurring;
        [ObservableProperty] private int installments = 2;

        public ObservableCollection<string> Periods { get; } = new() { "Mensal", "Semanal", "Anual" };
        [ObservableProperty] private string selectedPeriod = "Mensal";

        [ObservableProperty] private bool isTransfer;
        [ObservableProperty] private bool isCategoryVisible;

        // Propriedade para controlar a revelação da tela
        [ObservableProperty] private bool hasTypeSelected;

        [ObservableProperty] private bool isCreationMode = true;
        private Transaction _editingTransaction;

        [ObservableProperty] private Category selectedCategory;
        partial void OnSelectedCategoryChanged(Category value)
        {
            OnPropertyChanged(nameof(HasCategory));
            OnPropertyChanged(nameof(IsCategoryEmpty));
        }
        public bool HasCategory => SelectedCategory != null;
        public bool IsCategoryEmpty => SelectedCategory == null;

        [ObservableProperty] private PaymentSource selectedSource;
        partial void OnSelectedSourceChanged(PaymentSource value)
        {
            OnPropertyChanged(nameof(HasSource));
            OnPropertyChanged(nameof(IsSourceEmpty));
        }
        public bool HasSource => SelectedSource != null;
        public bool IsSourceEmpty => SelectedSource == null;

        [ObservableProperty] private PaymentSource selectedDestination;
        partial void OnSelectedDestinationChanged(PaymentSource value)
        {
            OnPropertyChanged(nameof(HasDestination));
            OnPropertyChanged(nameof(IsDestinationEmpty));
        }
        public bool HasDestination => SelectedDestination != null;
        public bool IsDestinationEmpty => SelectedDestination == null;

        public string FormattedDate
        {
            get
            {
                var today = DateTime.Today;
                if (Date.Date == today) return "Hoje";
                if (Date.Date == today.AddDays(-1)) return "Ontem";
                return Date.ToString("dd/MM/yyyy");
            }
        }

        private TransactionType? _currentType = null;
        private string _selectionTarget = "";

        private List<Category> _allCategoriesCache = new();
        private List<PaymentSource> _allAccountsCache = new();
        private List<PaymentSource> _allCardsCache = new();

        public TransactionAddViewModel(TransactionService transactionService, CategoryService categoryService, AccountService accountService, CreditCardService creditCardService)
        {
            _transactionService = transactionService;
            _categoryService = categoryService;
            _accountService = accountService;
            _creditCardService = creditCardService;

            WeakReferenceMessenger.Default.Register<ItemSelectedMessage>(this, (recipient, message) =>
            {
                var vm = (TransactionAddViewModel)recipient;
                if (string.IsNullOrEmpty(vm._selectionTarget)) return;

                string currentTarget = vm._selectionTarget;
                object data = message.Value;
                vm._selectionTarget = "";

                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    try
                    {
                        await Task.Delay(300);
                        if (data is SelectionItem selItem) data = selItem.OriginalObject;

                        if (currentTarget == "Category" && data is Category cat)
                        {
                            if (string.IsNullOrEmpty(cat.Color)) cat.Color = "#A1A1AA";
                            if (string.IsNullOrEmpty(cat.Icon)) cat.Icon = "bookmark";
                            vm.SelectedCategory = cat;
                        }
                        else if (currentTarget == "Source" && data is PaymentSource src) vm.SelectedSource = src;
                        else if (currentTarget == "Destination" && data is PaymentSource dest) vm.SelectedDestination = dest;
                    }
                    catch (Exception ex) { await App.Current.MainPage.DisplayAlert("Erro", ex.Message, "OK"); }
                });
            });
        }

        partial void OnDateChanged(DateTime value) => OnPropertyChanged(nameof(FormattedDate));

        [RelayCommand]
        public void ToggleMoreDetails() => ShowMoreDetails = !ShowMoreDetails;

        private async void SetupScreenVisuals()
        {
            if (_currentType == null)
            {
                HasTypeSelected = false; // Esconde o restante da tela
                AmountTitle = "Novo Lançamento";
                SemanticColor = "#3F3F46";
                IsCategoryVisible = true;
                IsTransfer = false;

                if (_allCategoriesCache.Count == 0)
                {
                    await LoadDependencies();
                }
                return;
            }

            HasTypeSelected = true; // Mostra o restante da tela
            IsTransfer = _currentType == TransactionType.TRANSFERENCIA;
            IsCategoryVisible = !IsTransfer;

            if (_currentType == TransactionType.DESPESA)
            {
                AmountTitle = IsCreationMode ? "Nova despesa" : "Editar despesa";
                SemanticColor = "#FF5252";
                PaidLabelText = "Pago";
                SourceAccountTitle = "Pagar com";
            }
            else if (_currentType == TransactionType.RECEITA)
            {
                AmountTitle = IsCreationMode ? "Nova receita" : "Editar receita";
                SemanticColor = "#00E676";
                PaidLabelText = "Recebido";
                SourceAccountTitle = "Receber em";
            }
            else if (_currentType == TransactionType.TRANSFERENCIA)
            {
                AmountTitle = "Transferência";
                SemanticColor = "#3B82F6";
                PaidLabelText = "Efetivado";
                SourceAccountTitle = "Conta de origem";
            }

            if (_allCategoriesCache.Count == 0 || _allAccountsCache.Count == 0)
            {
                await LoadDependencies();
            }
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            // SE A TELA JÁ INICIOU, IGNORA A ROTA PARA NÃO RESETAR OS DADOS
            if (_isInitialized)
                return;

            if (query.ContainsKey("mode") && query["mode"].ToString() == "new")
            {
                IsCreationMode = true;
                _editingTransaction = null;
                _currentType = null;
                SelectedTransactionType = null;
                SetupScreenVisuals();

                _isInitialized = true; // Marca como iniciada
            }
            else if (query.ContainsKey("TransactionToEdit"))
            {
                IsCreationMode = false;
                _editingTransaction = query["TransactionToEdit"] as Transaction;

                if (_editingTransaction != null)
                {
                    _currentType = _editingTransaction.Type;

                    if (_currentType == TransactionType.DESPESA) SelectedTransactionType = "Despesa";
                    else if (_currentType == TransactionType.RECEITA) SelectedTransactionType = "Receita";
                    else if (_currentType == TransactionType.TRANSFERENCIA) SelectedTransactionType = "Transferência";

                    Description = _editingTransaction.Name;
                    Amount = _editingTransaction.Amount.ToString("N2", new System.Globalization.CultureInfo("pt-BR"));
                    Date = DateTimeOffset.FromUnixTimeMilliseconds(_editingTransaction.Date).DateTime.ToLocalTime();
                    IsPaid = _editingTransaction.Paid;
                    isFixed = _editingTransaction.IsFixed;
                    IsRecurring = _editingTransaction.IsRecurring;
                    Installments = _editingTransaction.Installments > 0 ? _editingTransaction.Installments : 2;

                    SetupScreenVisuals();

                    _isInitialized = true; // Marca como iniciada
                }
            }
        }

        private async Task LoadDependencies()
        {
            await ExecuteWithErrorHandlingAsync(async () =>
            {
                var tCats = _categoryService.GetCategoriesAsync();
                var tAccs = _accountService.GetAccountsAsync();
                var tCards = _creditCardService.GetCreditCardsAsync();

                await Task.WhenAll(tCats, tAccs, tCards);

                var cats = await tCats;
                var accs = await tAccs;
                var cards = await tCards;

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    _allCategoriesCache.Clear();
                    if (cats != null)
                    {
                        _allCategoriesCache.AddRange(cats);
                        _systemTransferCategory = cats.FirstOrDefault(c => c.Type == TransactionType.TRANSFERENCIA || c.Name.Equals("Transferência", StringComparison.OrdinalIgnoreCase));
                    }

                    _allAccountsCache.Clear();
                    if (accs != null)
                    {
                        foreach (var a in accs.Where(x => x.Type != AccountType.CREDIT_CARD))
                            _allAccountsCache.Add(new PaymentSource { Id = a.Id, Name = a.Name, Type = "ACCOUNT", Icon = string.IsNullOrEmpty(a.Icon) ? "account_balance_wallet" : a.Icon, Color = string.IsNullOrEmpty(a.Color) ? "#42A5F5" : a.Color });
                    }

                    _allCardsCache.Clear();
                    if (cards != null)
                    {
                        foreach (var c in cards)
                        {
                            var accountIdToUse = c.AccountId != Guid.Empty ? c.AccountId : c.Id;
                            _allCardsCache.Add(new PaymentSource { Id = accountIdToUse, Name = c.Name, Type = "CREDIT_CARD", Icon = string.IsNullOrEmpty(c.Icon) ? "credit_card" : c.Icon, Color = string.IsNullOrEmpty(c.Color) ? "#9C27B0" : c.Color });
                        }
                    }

                    if (_currentType != null)
                    {
                        UpdateCategoriesList();
                        UpdatePaymentSourcesList();

                        if (!IsCreationMode && _editingTransaction != null)
                        {
                            SelectedSource = PaymentSources.FirstOrDefault(p => p.Id == _editingTransaction.AccountId);
                            if (IsTransfer && _editingTransaction.TargetAccountId.HasValue)
                            {
                                SelectedDestination = PaymentSources.FirstOrDefault(p => p.Id == _editingTransaction.TargetAccountId.Value);
                            }
                            SelectedCategory = Categories.SelectMany(c => c.SubCategories.Concat(new[] { c })).FirstOrDefault(c => c.Id == _editingTransaction.CategoryId);
                        }
                    }
                });
            });
        }

        private void UpdateCategoriesList()
        {
            Categories.Clear();
            SelectedCategory = null;

            if (_currentType == null || IsTransfer) return;

            var rootCategories = _allCategoriesCache.Where(c => c.ParentId == null && c.Type == _currentType).ToList();
            var subCategories = _allCategoriesCache.Where(c => c.ParentId != null && c.Type == _currentType).ToList();

            foreach (var root in rootCategories)
            {
                if (string.IsNullOrEmpty(root.Color)) root.Color = "#A1A1AA";
                if (string.IsNullOrEmpty(root.Icon)) root.Icon = "bookmark";

                root.SubCategories.Clear();
                foreach (var sub in subCategories.Where(s => s.ParentId == root.Id))
                {
                    if (string.IsNullOrEmpty(sub.Color)) sub.Color = "#A1A1AA";
                    if (string.IsNullOrEmpty(sub.Icon)) sub.Icon = "bookmark";
                    root.SubCategories.Add(sub);
                }
                Categories.Add(root);
            }
        }

        private void UpdatePaymentSourcesList()
        {
            PaymentSources.Clear();
            SelectedSource = null;

            if (_currentType == null) return;

            foreach (var acc in _allAccountsCache)
            {
                PaymentSources.Add(acc);
            }

            if (_currentType == TransactionType.DESPESA)
            {
                foreach (var card in _allCardsCache)
                {
                    PaymentSources.Add(card);
                }
            }
        }

        [RelayCommand]
        public async Task GoToSelectCategory()
        {
            if (_currentType == null)
            {
                await App.Current.MainPage.DisplayAlert("Aviso", "Selecione o Tipo de Lançamento primeiro.", "OK");
                return;
            }

            _selectionTarget = "Category";
            var selectionList = new List<SelectionItem>();

            foreach (var cat in Categories.Where(c => c.ParentId == null))
            {
                var item = new SelectionItem { Id = cat.Id, Name = cat.Name, Icon = cat.Icon, Color = cat.Color, OriginalObject = cat };
                foreach (var sub in cat.SubCategories) item.SubItems.Add(new SelectionItem { Id = sub.Id, Name = sub.Name, Icon = sub.Icon, Color = sub.Color, OriginalObject = sub, IsSubItem = true });
                selectionList.Add(item);
            }

            var navParams = new Dictionary<string, object> { { "Title", "Selecione a Categoria" }, { "Items", selectionList } };
            await Shell.Current.GoToAsync(nameof(Views.Privates.ItemSelectionPage), navParams);
        }

        [RelayCommand]
        public async Task GoToSelectSource()
        {
            if (!IsCreationMode) return;

            if (_currentType == null)
            {
                await App.Current.MainPage.DisplayAlert("Aviso", "Selecione o Tipo de Lançamento primeiro.", "OK");
                return;
            }

            _selectionTarget = "Source";
            var selectionList = PaymentSources.Select(p => new SelectionItem { Id = p.Id, Name = p.Name, Icon = p.Icon, Color = p.Color, OriginalObject = p }).ToList();
            var navParams = new Dictionary<string, object> { { "Title", SourceAccountTitle }, { "Items", selectionList } };
            await Shell.Current.GoToAsync(nameof(Views.Privates.ItemSelectionPage), navParams);
        }

        [RelayCommand]
        public async Task GoToSelectDestination()
        {
            if (!IsCreationMode) return;

            _selectionTarget = "Destination";
            var selectionList = _allAccountsCache.Select(p => new SelectionItem { Id = p.Id, Name = p.Name, Icon = p.Icon, Color = p.Color, OriginalObject = p }).ToList();
            var navParams = new Dictionary<string, object> { { "Title", "Conta de Destino" }, { "Items", selectionList } };
            await Shell.Current.GoToAsync(nameof(Views.Privates.ItemSelectionPage), navParams);
        }

        [RelayCommand]
        public async Task SaveAndContinue()
        {
            bool success = await SaveInternal();
            if (success)
            {
                await App.Current.MainPage.DisplayAlert("Sucesso", "Salvo!", "OK");
                if (IsCreationMode)
                {
                    Amount = string.Empty;
                    Description = string.Empty;
                    SelectedCategory = null;
                    IsFixed = false;
                    IsRecurring = false;
                    Installments = 2;
                }
                else await Shell.Current.GoToAsync("..");
            }
        }

        [RelayCommand]
        public async Task SaveAndClose()
        {
            bool success = await SaveInternal();
            if (success) await Shell.Current.GoToAsync("..");
        }

        private async Task<bool> SaveInternal()
        {
            if (_currentType == null)
            {
                await App.Current.MainPage.DisplayAlert("Atenção", "Selecione o Tipo de Lançamento no topo.", "OK");
                return false;
            }

            if (string.IsNullOrWhiteSpace(Description) || string.IsNullOrWhiteSpace(Amount))
            {
                await App.Current.MainPage.DisplayAlert("Atenção", "Preencha a Descrição e o Valor.", "OK");
                return false;
            }
            if (SelectedSource == null)
            {
                await App.Current.MainPage.DisplayAlert("Atenção", $"Selecione a {SourceAccountTitle.ToLower()}.", "OK");
                return false;
            }
            if (!IsTransfer && SelectedCategory == null)
            {
                await App.Current.MainPage.DisplayAlert("Atenção", "Selecione a categoria.", "OK");
                return false;
            }
            if (IsTransfer && SelectedDestination == null)
            {
                await App.Current.MainPage.DisplayAlert("Atenção", "Selecione a conta de destino.", "OK");
                return false;
            }

            string cleanAmount = Amount.Replace("R$", "").Replace(".", "").Trim();
            if (!decimal.TryParse(cleanAmount, System.Globalization.NumberStyles.Any, new System.Globalization.CultureInfo("pt-BR"), out decimal decimalAmount))
            {
                await App.Current.MainPage.DisplayAlert("Erro", "Valor inválido.", "OK");
                return false;
            }

            bool savedSuccessfully = false;

            await ExecuteWithErrorHandlingAsync(async () =>
            {
                DateTimeOffset safeDate = new DateTimeOffset(Date.Year, Date.Month, Date.Day, 12, 0, 0, TimeSpan.Zero);
                Transaction transaction = new Transaction
                {
                    Name = Description,
                    Description = Description,
                    Type = _currentType.Value,
                    Amount = decimalAmount,
                    Date = safeDate.ToUnixTimeMilliseconds(), // Usa a data blindada
                    Paid = IsPaid,
                    AccountId = SelectedSource.Id,
                    CategoryId = IsTransfer ? (_systemTransferCategory?.Id ?? Guid.Empty) : (SelectedCategory?.Id ?? Guid.Empty),
                    IsFixed = IsFixed,
                    IsRecurring = IsRecurring,
                    Installments = IsRecurring ? Installments : 1,
                    CreditCardId = SelectedSource.Type == "CREDIT_CARD" ? SelectedSource.Id : (Guid?)null
                };

                if (IsTransfer) transaction.TargetAccountId = SelectedDestination?.Id;

                if (IsCreationMode)
                {
                    transaction.Id = Guid.NewGuid();
                    bool success = await _transactionService.SaveTransactionAsync(transaction);
                    if (!success) throw new Exception("Erro ao salvar no servidor.");
                }
                else
                {
                    transaction.Id = _editingTransaction.Id;
                    transaction.AccountId = _editingTransaction.AccountId;

                    bool success = await _transactionService.UpdateTransactionAsync(transaction.Id.Value, transaction);
                    if (!success) throw new Exception("Erro ao atualizar no servidor.");
                }

                savedSuccessfully = true;
            });

            return savedSuccessfully;
        }

        [RelayCommand]
        public async Task Cancel() => await Shell.Current.GoToAsync("..");
    }

    public class PaymentSource
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Icon { get; set; }
        public string Color { get; set; }
    }
}