using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using controle_ja_mobile.Helpers;
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

        private bool _isInitialized = false;

        public ObservableCollection<PaymentSource> PaymentSources { get; } = new();
        public ObservableCollection<Category> Categories { get; } = new();
        public ObservableCollection<string> TransactionTypes { get; } = new() { "Despesa", "Despesa no Cartão", "Receita", "Transferência" };

        [ObservableProperty] private bool isCreditCardMode;
        [ObservableProperty] private bool isStatusVisible = true; // Controla se o switch "Pago" aparece
        [ObservableProperty] private string installmentSimulation;
        [ObservableProperty] private string moreDetailsText = "MAIS DETALHES"; // Texto dinâmico
        [ObservableProperty] private string selectedInvoice = "Fatura atual"; // Guarda a fatura selecionada

        [ObservableProperty] private string selectedTransactionType;
        partial void OnSelectedTransactionTypeChanged(string value)
        {
            if (string.IsNullOrEmpty(value)) return;

            IsCreditCardMode = value == "Despesa no Cartão";

            if (value == "Despesa" || value == "Despesa no Cartão") _currentType = TransactionType.DESPESA;
            else if (value == "Receita") _currentType = TransactionType.RECEITA;
            else if (value == "Transferência") _currentType = TransactionType.TRANSFERENCIA;

            SetupScreenVisuals();
            UpdatePaymentSourcesList();
            UpdateCategoriesList();
            CalculateInstallment();

            // Define uma fatura padrão inicial baseada na data
            if (IsCreditCardMode && SelectedInvoice == "Fatura atual")
            {
                SelectedInvoice = $"Fatura: 10 {Date.ToString("MMM", new System.Globalization.CultureInfo("pt-BR"))}. {Date.Year}";
            }
        }

        [ObservableProperty] private string amountTitle = "Novo Lançamento";
        [ObservableProperty] private string semanticColor = "#3F3F46";
        [ObservableProperty] private string sourceAccountTitle = "Conta / Cartão";

        [ObservableProperty] private string transactionName;
        [ObservableProperty] private string description;

        [ObservableProperty] private string amount = "0,00";
        partial void OnAmountChanged(string value) => CalculateInstallment();

        [ObservableProperty] private DateTime date = DateTime.Today;
        partial void OnDateChanged(DateTime value)
        {
            OnPropertyChanged(nameof(FormattedDate));

            if (IsCreditCardMode)
            {
                SelectedInvoice = $"Fatura: 10 {value.ToString("MMM", new System.Globalization.CultureInfo("pt-BR"))}. {value.Year}";
            }
        }

        [ObservableProperty] private bool isPaid = true;
        [ObservableProperty] private string paidLabelText = "Status";

        [ObservableProperty] private bool showMoreDetails = false;
        [ObservableProperty] private bool isFixed;
        [ObservableProperty] private bool isRecurring;
        [ObservableProperty] private int installments = 1;
        partial void OnInstallmentsChanged(int value) => CalculateInstallment();

        public ObservableCollection<string> Periods { get; } = new() { "Mensal", "Semanal", "Anual" };
        [ObservableProperty] private string selectedPeriod = "Mensal";

        [ObservableProperty] private bool isTransfer;
        [ObservableProperty] private bool isCategoryVisible;
        [ObservableProperty] private bool hasTypeSelected;
        [ObservableProperty] private bool isCreationMode = true;

        private Transaction _editingTransaction;

        [ObservableProperty] private Category selectedCategory;
        partial void OnSelectedCategoryChanged(Category value) { OnPropertyChanged(nameof(HasCategory)); OnPropertyChanged(nameof(IsCategoryEmpty)); }
        public bool HasCategory => SelectedCategory != null;
        public bool IsCategoryEmpty => SelectedCategory == null;

        [ObservableProperty] private PaymentSource selectedSource;
        partial void OnSelectedSourceChanged(PaymentSource value) { OnPropertyChanged(nameof(HasSource)); OnPropertyChanged(nameof(IsSourceEmpty)); }
        public bool HasSource => SelectedSource != null;
        public bool IsSourceEmpty => SelectedSource == null;

        [ObservableProperty] private PaymentSource selectedDestination;
        partial void OnSelectedDestinationChanged(PaymentSource value) { OnPropertyChanged(nameof(HasDestination)); OnPropertyChanged(nameof(IsDestinationEmpty)); }
        public bool HasDestination => SelectedDestination != null;
        public bool IsDestinationEmpty => SelectedDestination == null;

        public string FormattedDate => Date.Date == DateTime.Today ? "Hoje" : (Date.Date == DateTime.Today.AddDays(-1) ? "Ontem" : Date.ToString("dd/MM/yyyy"));

        private TransactionType? _currentType = null;
        private string _selectionTarget = "";

        private List<Category> _allCategoriesCache = new();
        private List<PaymentSource> _allAccountsCache = new();
        private List<PaymentSource> _allCardsCache = new();

        [ObservableProperty] private bool isCalculatorOpen;
        [ObservableProperty] private string calcDisplay = "0";
        [ObservableProperty] private string calcEquation = "";
        private decimal _calcStoredValue = 0;
        private string _calcCurrentOp = "";
        private bool _calcIsNewNumber = true;

        public TransactionAddViewModel(TransactionService ts, CategoryService cs, AccountService asrv, CreditCardService ccs)
        {
            _transactionService = ts; _categoryService = cs; _accountService = asrv; _creditCardService = ccs;

            WeakReferenceMessenger.Default.Register<ItemSelectedMessage>(this, (r, m) =>
            {
                var vm = (TransactionAddViewModel)r;
                if (string.IsNullOrEmpty(vm._selectionTarget)) return;

                string target = vm._selectionTarget; object data = m.Value; vm._selectionTarget = "";
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    try
                    {
                        await Task.Delay(100);
                        if (data is SelectionItem selItem) data = selItem.OriginalObject;

                        if (target == "Category" && data is Category cat) vm.SelectedCategory = cat;
                        else if (target == "Source" && data is PaymentSource src) vm.SelectedSource = src;
                        else if (target == "Destination" && data is PaymentSource dest) vm.SelectedDestination = dest;
                        else if (target == "Invoice" && data is string inv) vm.SelectedInvoice = inv;
                    }
                    catch { }
                });
            });
        }

        private void CalculateInstallment()
        {
            if (Installments > 0 && decimal.TryParse(Amount.Replace("R$", "").Replace(".", "").Trim(), System.Globalization.NumberStyles.Any, new System.Globalization.CultureInfo("pt-BR"), out decimal decimalAmount))
            {
                decimal portion = decimalAmount / Installments;
                InstallmentSimulation = $"{Installments}x de {portion.ToString("C", new System.Globalization.CultureInfo("pt-BR"))}";
            }
            else { InstallmentSimulation = ""; }
        }

        private void CloseKeyboard()
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
#if ANDROID
                var activity = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity;
                var currentFocus = activity?.CurrentFocus;
                if (currentFocus != null)
                {
                    currentFocus.ClearFocus();
                    var inputMethodManager = activity.GetSystemService(Android.Content.Context.InputMethodService) as Android.Views.InputMethods.InputMethodManager;
                    inputMethodManager?.HideSoftInputFromWindow(currentFocus.WindowToken, Android.Views.InputMethods.HideSoftInputFlags.None);
                }
#endif
                await Task.Delay(50);
            });
        }

        [RelayCommand]
        public void ToggleMoreDetails()
        {
            CloseKeyboard();
            ShowMoreDetails = !ShowMoreDetails;
            MoreDetailsText = ShowMoreDetails ? "MENOS DETALHES" : "MAIS DETALHES";
        }

        [RelayCommand]
        public async Task GoToSelectInvoice()
        {
            CloseKeyboard();
            _selectionTarget = "Invoice";

            var list = new List<SelectionItem>();
            var culture = new System.Globalization.CultureInfo("pt-BR");

            // Gera as próximas 4 faturas baseadas na data atual
            for (int i = 0; i < 4; i++)
            {
                var fatDate = Date.AddMonths(i);
                string fatName = $"Fatura: 10 {fatDate.ToString("MMM", culture)}. {fatDate.Year}";
                list.Add(new SelectionItem { Id = Guid.NewGuid(), Name = fatName, OriginalObject = fatName });
            }

            await Shell.Current.GoToAsync("ItemSelectionPage", new Dictionary<string, object> { { "Title", "Fatura" }, { "Items", list } });
        }

        [RelayCommand]
        public async Task GoToSelectCategory()
        {
            CloseKeyboard();
            if (_currentType == null) return;
            _selectionTarget = "Category";

            var list = Categories.Where(c => c.ParentId == null).Select(cat =>
            {
                var nivel1 = new SelectionItem { Id = cat.Id, Name = cat.Name, Icon = cat.Icon, Color = cat.Color, OriginalObject = cat };
                foreach (var sub in cat.SubCategories)
                {
                    var nivel2 = new SelectionItem { Id = sub.Id, Name = sub.Name, Icon = sub.Icon, Color = sub.Color, OriginalObject = sub, IsSubItem = true };
                    var netos = _allCategoriesCache.Where(x => x.ParentId == sub.Id).ToList();
                    foreach (var neto in netos)
                        nivel2.SubItems.Add(new SelectionItem { Id = neto.Id, Name = neto.Name, Icon = neto.Icon, Color = neto.Color, OriginalObject = neto, IsSubItem = true });
                    nivel1.SubItems.Add(nivel2);
                }
                return nivel1;
            }).ToList();

            await Shell.Current.GoToAsync("ItemSelectionPage", new Dictionary<string, object> { { "Title", "Categoria" }, { "Items", list } });
        }

        [RelayCommand] public async Task GoToSelectSource() { CloseKeyboard(); _selectionTarget = "Source"; await Shell.Current.GoToAsync("ItemSelectionPage", new Dictionary<string, object> { { "Title", SourceAccountTitle }, { "Items", PaymentSources.Select(p => new SelectionItem { Id = p.Id, Name = p.Name, Icon = p.Icon, Color = p.Color, OriginalObject = p }).ToList() } }); }
        [RelayCommand] public async Task GoToSelectDestination() { CloseKeyboard(); _selectionTarget = "Destination"; await Shell.Current.GoToAsync("ItemSelectionPage", new Dictionary<string, object> { { "Title", "Conta de Destino" }, { "Items", _allAccountsCache.Select(p => new SelectionItem { Id = p.Id, Name = p.Name, Icon = p.Icon, Color = p.Color, OriginalObject = p }).ToList() } }); }
        [RelayCommand]
        public void ToggleCalculator()
        {
            CloseKeyboard();
            if (IsCalculatorOpen) { IsCalculatorOpen = false; return; }

            // Pega o valor da tela tirando os pontos de milhar, pra não dar conflito
            CalcDisplay = Amount.Replace("R$", "").Replace(".", "").Trim();
            CalcEquation = "";
            _calcStoredValue = 0;
            _calcCurrentOp = "";
            _calcIsNewNumber = true;
            IsCalculatorOpen = true;
        }

        [RelayCommand]
        public void CalcInput(string key)
        {
            var culture = new System.Globalization.CultureInfo("pt-BR");

            if (key == "DEL")
            {
                if (CalcDisplay.Length > 1) CalcDisplay = CalcDisplay.Substring(0, CalcDisplay.Length - 1);
                else CalcDisplay = "0";
                return;
            }
            if (key == "=")
            {
                EvaluateCalc();
                _calcCurrentOp = "";
                CalcEquation = "";
                _calcIsNewNumber = true;
                return;
            }
            if (key == "+" || key == "-" || key == "×" || key == "÷")
            {
                if (_calcCurrentOp != "")
                    EvaluateCalc();
                else
                    decimal.TryParse(CalcDisplay, System.Globalization.NumberStyles.Any, culture, out _calcStoredValue);

                _calcCurrentOp = key;
                _calcIsNewNumber = true;
                CalcEquation = $"{_calcStoredValue.ToString("0.##", culture)} {key}";
                return;
            }
            if (_calcIsNewNumber)
            {
                CalcDisplay = key == "," ? "0," : key;
                _calcIsNewNumber = false;
            }
            else
            {
                if (key == "," && CalcDisplay.Contains(",")) return;
                CalcDisplay = CalcDisplay == "0" && key != "," ? key : CalcDisplay + key;
            }
        }

        private void EvaluateCalc()
        {
            var culture = new System.Globalization.CultureInfo("pt-BR");

            // Tenta converter forçando a cultura pt-BR
            if (decimal.TryParse(CalcDisplay, System.Globalization.NumberStyles.Any, culture, out decimal currentVal))
            {
                if (_calcCurrentOp == "+") _calcStoredValue += currentVal;
                else if (_calcCurrentOp == "-") _calcStoredValue -= currentVal;
                else if (_calcCurrentOp == "×") _calcStoredValue *= currentVal;
                else if (_calcCurrentOp == "÷" && currentVal != 0) _calcStoredValue /= currentVal;

                CalcDisplay = _calcStoredValue.ToString("0.##", culture);
            }
        }
        [RelayCommand]
        public void ApplyCalculator()
        {
            var culture = new System.Globalization.CultureInfo("pt-BR");

            if (_calcCurrentOp != "") EvaluateCalc();

            if (decimal.TryParse(CalcDisplay, System.Globalization.NumberStyles.Any, culture, out decimal val))
                Amount = val.ToString("N2", culture);

            IsCalculatorOpen = false;
            CalculateInstallment();
        }

        private async void SetupScreenVisuals()
        {
            if (_currentType == null) { HasTypeSelected = false; AmountTitle = "Novo Lançamento"; SemanticColor = "#3F3F46"; return; }
            HasTypeSelected = true; IsTransfer = _currentType == TransactionType.TRANSFERENCIA; IsCategoryVisible = !IsTransfer;

            IsStatusVisible = !IsCreditCardMode; // Esconde o Status se for cartão

            if (IsCreditCardMode) { AmountTitle = "Despesa no Cartão"; SemanticColor = "#9333EA"; SourceAccountTitle = "Selecione o Cartão"; }
            else if (_currentType == TransactionType.DESPESA) { AmountTitle = "Nova despesa"; SemanticColor = "#FF5252"; PaidLabelText = "Pago"; SourceAccountTitle = "Pagar com"; }
            else if (_currentType == TransactionType.RECEITA) { AmountTitle = "Nova receita"; SemanticColor = "#00E676"; PaidLabelText = "Recebido"; SourceAccountTitle = "Receber em"; }
            else if (_currentType == TransactionType.TRANSFERENCIA) { AmountTitle = "Transferência"; SemanticColor = "#3B82F6"; PaidLabelText = "Efetivado"; SourceAccountTitle = "Conta de origem"; }

            if (_allCategoriesCache.Count == 0) await LoadDependencies();
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (_isInitialized) return;
            if (query.ContainsKey("mode") && query["mode"].ToString() == "new") { IsCreationMode = true; _editingTransaction = null; _currentType = null; SelectedTransactionType = null; SetupScreenVisuals(); _isInitialized = true; }
            else if (query.ContainsKey("TransactionToEdit"))
            {
                IsCreationMode = false; _editingTransaction = query["TransactionToEdit"] as Transaction;
                if (_editingTransaction != null)
                {
                    _currentType = _editingTransaction.Type;
                    SelectedTransactionType = _currentType.ToString();
                    TransactionName = _editingTransaction.Name; Description = _editingTransaction.Description;
                    Amount = _editingTransaction.Amount.ToString("N2", new System.Globalization.CultureInfo("pt-BR"));
                    Date = _editingTransaction.DateTimeObject; IsPaid = _editingTransaction.Paid;
                    SetupScreenVisuals(); _isInitialized = true;
                }
            }
        }

        private async Task LoadDependencies()
        {
            await ExecuteWithErrorHandlingAsync(async () => {
                var tCats = _categoryService.GetCategoriesAsync();
                var tAccs = _accountService.GetAccountsAsync();
                var tCards = _creditCardService.GetCreditCardsAsync();

                await Task.WhenAll(tCats, tAccs, tCards);

                var cats = await tCats;
                var accs = await tAccs;
                var cards = await tCards;

                MainThread.BeginInvokeOnMainThread(() => {
                    _allCategoriesCache.Clear();
                    if (cats != null) _allCategoriesCache.AddRange(cats);

                    _allAccountsCache.Clear();
                    if (accs != null) foreach (var a in accs) _allAccountsCache.Add(new PaymentSource { Id = a.Id, Name = a.Name, Icon = a.Icon, Color = a.Color, Type = "ACCOUNT" });

                    _allCardsCache.Clear();
                    if (cards != null)
                    {
                        foreach (var c in cards)
                        {
                            _allCardsCache.Add(new PaymentSource
                            {
                                Id = c.Id,
                                LinkedAccountId = c.AccountId,
                                Name = c.Name,
                                Icon = c.Icon,
                                Color = c.Color,
                                Type = "CREDIT_CARD"
                            });
                        }
                    }

                    UpdateCategoriesList();
                    UpdatePaymentSourcesList();
                });
            });
        }

        private void UpdateCategoriesList() { Categories.Clear(); var roots = _allCategoriesCache.Where(c => c.ParentId == null && c.Type == _currentType).ToList(); foreach (var r in roots) Categories.Add(r); }

        private void UpdatePaymentSourcesList()
        {
            PaymentSources.Clear();
            if (IsCreditCardMode) { foreach (var card in _allCardsCache) PaymentSources.Add(card); }
            else { foreach (var acc in _allAccountsCache) PaymentSources.Add(acc); }
        }

        [RelayCommand] public async Task SaveAndClose() { if (await SaveInternal()) await Shell.Current.GoToAsync(".."); }
        [RelayCommand] public async Task Cancel() => await Shell.Current.GoToAsync("..");

        private async Task<bool> SaveInternal()
        {
            // 1. Validação básica e blindagem contra NullReferenceException
            if (string.IsNullOrWhiteSpace(TransactionName) || _currentType == null || SelectedSource == null)
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await App.Current.MainPage.DisplayAlert("Aviso", "Preencha o nome da transação e selecione uma conta/cartão.", "OK");
                });
                return false;
            }

            // 2. Formatação do valor (agora blindado para pt-BR)
            decimal.TryParse(Amount.Replace("R$", "").Replace(".", "").Trim(), System.Globalization.NumberStyles.Any, new System.Globalization.CultureInfo("pt-BR"), out decimal decimalAmount);

            // 3. SEPARAÇÃO INTELIGENTE DE IDs (Conta vs Cartão)
            Guid finalAccountId = SelectedSource.Id;
            Guid? finalCreditCardId = null;

            if (SelectedSource.Type == "CREDIT_CARD")
            {
                finalCreditCardId = SelectedSource.Id; // Aqui vai o ID do Cartão
                finalAccountId = SelectedSource.LinkedAccountId ?? Guid.Empty; // Aqui vai a Conta do Banco atrelada a ele
            }

            // 4. Montagem do DTO
            Transaction t = new Transaction
            {
                Name = TransactionName,
                Description = Description,
                Type = _currentType.Value,
                Amount = decimalAmount,
                Date = new DateTimeOffset(Date).ToUnixTimeMilliseconds(),
                Paid = IsPaid,

                AccountId = finalAccountId,
                CreditCardId = finalCreditCardId,
                CategoryId = SelectedCategory?.Id ?? Guid.Empty,

                // Já aproveitamos para mandar a recorrência/parcelas certinho!
                IsFixed = IsFixed,
                Installments = IsRecurring ? Installments : 1
            };

            // 5. Envio para a API
            return await _transactionService.SaveTransactionAsync(t);
        }
    }

    public class PaymentSource
    {
        public Guid Id { get; set; }
        public Guid? LinkedAccountId { get; set; } // NOVO: Guarda o ID da conta do banco
        public string Name { get; set; }
        public string Type { get; set; }
        public string Icon { get; set; }
        public string Color { get; set; }
    }
}