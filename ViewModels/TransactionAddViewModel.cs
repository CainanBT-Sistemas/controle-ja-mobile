using CommunityToolkit.Maui.Views;
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

        // NOVO: Guarda a instância do Popup para conseguirmos fechar ele sem dar "GoBack" na navegação!
        public Popup? PopupInstance { get; set; }

        public ObservableCollection<PaymentSource> PaymentSources { get; } = new();
        public ObservableCollection<Category> Categories { get; } = new();
        public ObservableCollection<string> TransactionTypes { get; } = new() { "Despesa", "Despesa no Cartão", "Receita", "Transferência" };

        [ObservableProperty] private bool isCreditCardMode;
        [ObservableProperty] private bool isStatusVisible = true;
        [ObservableProperty] private string installmentSimulation;
        [ObservableProperty] private string moreDetailsText = "MAIS DETALHES";

        [ObservableProperty] private bool isEditable = true;

        public double FormOpacity => IsEditable ? 1.0 : 0.4;

        [ObservableProperty] private string selectedTransactionType;
        partial void OnSelectedTransactionTypeChanged(string value)
        {
            if (string.IsNullOrEmpty(value)) return;
            if (IsCreationMode) SelectedCategory = null;

            IsCreditCardMode = value == "Despesa no Cartão";
            if (value == "Despesa" || value == "Despesa no Cartão") _currentType = TransactionType.DESPESA;
            else if (value == "Receita") _currentType = TransactionType.RECEITA;
            else if (value == "Transferência") _currentType = TransactionType.TRANSFERENCIA;

            SetupScreenVisuals();
            UpdatePaymentSourcesList();
            UpdateCategoriesList();
            CalculateInstallment();
        }

        [ObservableProperty] private string amountTitle = "Novo Lançamento";
        [ObservableProperty] private string semanticColor = "#3F3F46";
        [ObservableProperty] private string sourceAccountTitle = "Conta / Cartão";
        [ObservableProperty] private string transactionName;
        [ObservableProperty] private string description;
        [ObservableProperty] private string amount = "0,00";
        partial void OnAmountChanged(string value) => CalculateInstallment();
        [ObservableProperty] private DateTime date = DateTime.Today;
        partial void OnDateChanged(DateTime value) => OnPropertyChanged(nameof(FormattedDate));

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

        [ObservableProperty] private string vehicleName;
        [ObservableProperty] private string currentOdometer;
        [ObservableProperty] private string liters;

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
            _transactionService = ts;
            _categoryService = cs;
            _accountService = asrv;
            _creditCardService = ccs;

            WeakReferenceMessenger.Default.Register<ItemSelectedMessage>(this, (r, m) =>
            {
                var vm = (TransactionAddViewModel)r;
                if (string.IsNullOrEmpty(vm._selectionTarget)) return;

                string target = vm._selectionTarget; object data = m.Value; vm._selectionTarget = "";

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (data is SelectionItem selItem) data = selItem.OriginalObject;

                    if (target == "Category" && data is Category cat) vm.SelectedCategory = cat;
                    else if (target == "Source" && data is PaymentSource src) vm.SelectedSource = src;
                    else if (target == "Destination" && data is PaymentSource dest) vm.SelectedDestination = dest;
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
            MainThread.BeginInvokeOnMainThread(() =>
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
            });
        }

        [RelayCommand]
        public void ToggleMoreDetails()
        {
            CloseKeyboard();
            ShowMoreDetails = !ShowMoreDetails;
            MoreDetailsText = ShowMoreDetails ? "MENOS DETALHES" : "MAIS DETALHES";
        }

        private void ShowSelectionPopup(string title, IEnumerable<SelectionItem> items)
        {
            var vm = IPlatformApplication.Current?.Services.GetService<ItemSelectionViewModel>();
            if (vm != null)
            {
                vm.Initialize(title, items);
                var popup = new Views.Popups.ItemSelectionPopup(vm);
                Shell.Current.ShowPopup(popup);
            }
        }

        [RelayCommand]
        public void GoToSelectCategory()
        {
            if (!IsEditable) return;
            CloseKeyboard();
            if (_currentType == null) return;
            _selectionTarget = "Category";

            var list = Categories
                .Where(c => c.ParentId == null)
                .OrderBy(c => c.Name)
                .Select(cat =>
                {
                    var nivel1 = new SelectionItem { Id = cat.Id, Name = cat.Name, Icon = cat.Icon, Color = cat.Color, OriginalObject = cat };
                    var filhos = _allCategoriesCache.Where(x => x.ParentId == cat.Id).OrderBy(s => s.Name).ToList();
                    foreach (var sub in filhos)
                    {
                        nivel1.SubItems.Add(new SelectionItem
                        {
                            Id = sub.Id,
                            Name = sub.Name,
                            Icon = sub.Icon,
                            Color = sub.Color,
                            ParentColor = cat.Color,
                            OriginalObject = sub,
                            IsSubItem = true
                        });
                    }
                    return nivel1;
                }).ToList();

            ShowSelectionPopup("Categoria", list);
        }

        [RelayCommand]
        public void GoToSelectSource()
        {
            if (!IsEditable) return;
            CloseKeyboard();
            _selectionTarget = "Source";
            var itemsList = PaymentSources.Select(p => new SelectionItem { Id = p.Id, Name = p.Name, Icon = p.Icon, Color = p.Color, OriginalObject = p }).ToList();
            ShowSelectionPopup(SourceAccountTitle, itemsList);
        }

        [RelayCommand]
        public void GoToSelectDestination()
        {
            if (!IsEditable) return;
            CloseKeyboard();
            _selectionTarget = "Destination";
            var itemsList = _allAccountsCache.Select(p => new SelectionItem { Id = p.Id, Name = p.Name, Icon = p.Icon, Color = p.Color, OriginalObject = p }).ToList();
            ShowSelectionPopup("Conta de Destino", itemsList);
        }

        [RelayCommand]
        public void ToggleCalculator()
        {
            if (!IsEditable) return;
            CloseKeyboard();
            if (IsCalculatorOpen) { IsCalculatorOpen = false; return; }

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

        private void SetupScreenVisuals()
        {
            if (_currentType == null) { HasTypeSelected = false; AmountTitle = "Novo Lançamento"; SemanticColor = "#3F3F46"; return; }
            HasTypeSelected = true;
            IsTransfer = (_currentType == TransactionType.TRANSFERENCIA) && IsCreationMode;
            IsCategoryVisible = !IsTransfer;
            IsStatusVisible = !IsCreditCardMode;
            if (IsCreationMode)
            {
                if (IsCreditCardMode) { AmountTitle = "Despesa no Cartão"; SemanticColor = "#9333EA"; SourceAccountTitle = "Selecione o Cartão"; }
                else if (_currentType == TransactionType.DESPESA) { AmountTitle = "Nova despesa"; SemanticColor = "#FF5252"; PaidLabelText = "Pago"; SourceAccountTitle = "Pagar com"; }
                else if (_currentType == TransactionType.RECEITA) { AmountTitle = "Nova receita"; SemanticColor = "#00E676"; PaidLabelText = "Recebido"; SourceAccountTitle = "Receber em"; }
                else { AmountTitle = "Nova Transferência"; SemanticColor = "#3B82F6"; PaidLabelText = "Efetivado"; SourceAccountTitle = "Conta de origem"; }
            }
            else
            {
                AmountTitle = "Detalhes do Lançamento";
                if (IsCreditCardMode) { SemanticColor = "#9333EA"; SourceAccountTitle = "Cartão"; }
                else if (_currentType == TransactionType.DESPESA) { SemanticColor = "#FF5252"; PaidLabelText = "Pago"; SourceAccountTitle = "Conta"; }
                else if (_currentType == TransactionType.RECEITA) { SemanticColor = "#00E676"; PaidLabelText = "Recebido"; SourceAccountTitle = "Conta"; }
                else { SemanticColor = "#3B82F6"; PaidLabelText = "Efetivado"; SourceAccountTitle = "Conta"; }
            }
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (_isInitialized) return;

            if (query.ContainsKey("mode") && query["mode"].ToString() == "new")
            {
                IsCreationMode = true;
                IsEditable = true;
                _editingTransaction = null;
                _currentType = null;
                SelectedTransactionType = null;
                TransactionName = string.Empty;
                Description = string.Empty;
                Amount = "0,00";
                Date = DateTime.Today;
                IsPaid = true;
                IsFixed = false;
                IsRecurring = false;
                Installments = 1;
                SelectedCategory = null;
                SelectedSource = null;
                SelectedDestination = null;
                VehicleName = string.Empty;
                CurrentOdometer = string.Empty;
                Liters = string.Empty;
                ShowMoreDetails = false;

                if (query.ContainsKey("type"))
                {
                    SelectedTransactionType = query["type"].ToString();
                }

                SetupScreenVisuals();
                Task.Run(LoadDependencies);
                _isInitialized = true;
            }
            else if (query.ContainsKey("TransactionToEdit"))
            {
                IsCreationMode = false;
                _editingTransaction = query["TransactionToEdit"] as Transaction;
                if (_editingTransaction != null)
                {
                    _currentType = _editingTransaction.Type;
                    if (_currentType == TransactionType.RECEITA) SelectedTransactionType = "Receita";
                    else if (_currentType == TransactionType.DESPESA) SelectedTransactionType = _editingTransaction.CreditCardId != null ? "Despesa no Cartão" : "Despesa";
                    else SelectedTransactionType = "Transferência";

                    TransactionName = _editingTransaction.Name; Description = _editingTransaction.Description;
                    Amount = _editingTransaction.Amount.ToString("N2", new System.Globalization.CultureInfo("pt-BR"));
                    Date = _editingTransaction.DateTimeObject; IsPaid = _editingTransaction.Paid;

                    IsFixed = _editingTransaction.IsFixed;
                    if (IsFixed && _editingTransaction.RecurrenceFrequency != null)
                    {
                        SelectedPeriod = _editingTransaction.RecurrenceFrequency switch
                        {
                            RecurrenceFrequency.WEEKLY => "Semanal",
                            RecurrenceFrequency.YEARLY => "Anual",
                            _ => "Mensal"
                        };
                    }

                    IsEditable = true;

                    if (_editingTransaction.CategoryId != null)
                        SelectedCategory = new Category { Id = _editingTransaction.CategoryId.Value, Name = _editingTransaction.CategoryName ?? "Categoria", Color = "#A1A1AA", Icon = "bookmark" };
                    if (_editingTransaction.AccountId != null)
                        SelectedSource = new PaymentSource { Id = _editingTransaction.AccountId.Value, Name = _editingTransaction.AccountName ?? "Conta", Color = "#A1A1AA", Icon = "account_balance_wallet" };

                    SetupScreenVisuals();
                    Task.Run(LoadDependencies);
                    _isInitialized = true;
                }
            }
        }

        private async Task LoadDependencies()
        {
            var cats = await _categoryService.GetCategoriesAsync();
            var accs = await _accountService.GetAccountsAsync();
            var cards = await _creditCardService.GetCreditCardsAsync();

            MainThread.BeginInvokeOnMainThread(() => {
                _allCategoriesCache.Clear();
                if (cats != null) _allCategoriesCache.AddRange(cats);
                _allAccountsCache.Clear(); if (accs != null) foreach (var a in accs) _allAccountsCache.Add(new PaymentSource { Id = a.Id, Name = a.Name, Icon = a.Icon, Color = a.Color, Type = "ACCOUNT" });
                _allCardsCache.Clear(); if (cards != null) foreach (var c in cards) _allCardsCache.Add(new PaymentSource { Id = c.Id, LinkedAccountId = c.AccountId, Name = c.Name, Icon = c.Icon, Color = c.Color, Type = "CREDIT_CARD" });
                UpdateCategoriesList(); UpdatePaymentSourcesList();
            });
        }

        private void UpdateCategoriesList()
        {
            Categories.Clear();
            var roots = _allCategoriesCache
                .Where(c => c.ParentId == null && c.Type == _currentType)
                .OrderBy(c => c.Name).ToList();
            foreach (var r in roots) Categories.Add(r);

            if (IsCreationMode && SelectedCategory == null)
            {
                Category defaultCat = null;
                if (_currentType == TransactionType.DESPESA)
                    defaultCat = roots.FirstOrDefault(c => c.Name.Equals("Compras", StringComparison.OrdinalIgnoreCase)) ?? roots.FirstOrDefault(c => c.Name.Equals("Mercado", StringComparison.OrdinalIgnoreCase));
                else if (_currentType == TransactionType.RECEITA)
                    defaultCat = roots.FirstOrDefault(c => c.Name.Equals("Salário", StringComparison.OrdinalIgnoreCase)) ?? roots.FirstOrDefault(c => c.Name.Equals("Outros", StringComparison.OrdinalIgnoreCase));

                if (defaultCat != null) SelectedCategory = defaultCat;
            }
        }

        private void UpdatePaymentSourcesList()
        {
            PaymentSources.Clear();
            if (IsCreditCardMode) { foreach (var card in _allCardsCache) PaymentSources.Add(card); }
            else { foreach (var acc in _allAccountsCache) PaymentSources.Add(acc); }
        }

        [RelayCommand]
        public async Task SaveAndClose()
        {
            if (!IsEditable) return;
            if (await SaveInternal())
            {
                _isInitialized = false;
                // A INTELIGÊNCIA: Fecha o popup se existir, senão volta a página
                if (PopupInstance != null) { PopupInstance.Close(); PopupInstance = null; }
                else { await Shell.Current.GoToAsync(".."); }
            }
        }

        [RelayCommand]
        public async Task SaveAndAdd()
        {
            if (!IsEditable) return;
            if (await SaveInternal())
            {
                await App.Current.MainPage.DisplayAlert("Sucesso", "Lançamento salvo!", "OK");
                TransactionName = string.Empty; Description = string.Empty; Amount = "0,00";
                CalcDisplay = "0"; _calcStoredValue = 0; _calcCurrentOp = "";
                IsFixed = false; IsRecurring = false; Installments = 1; CalculateInstallment();
            }
        }

        [RelayCommand]
        public async Task Delete()
        {
            if (_editingTransaction == null) return;
            bool confirm = await App.Current.MainPage.DisplayAlert("Excluir", $"Deseja apagar '{_editingTransaction.Name}'?", "Sim", "Não");
            if (!confirm) return;

            var success = await _transactionService.DeleteTransactionAsync(_editingTransaction.Id.Value, false);
            if (success)
            {
                WeakReferenceMessenger.Default.Send(new GlobalRefreshMessage());
                _isInitialized = false;
                if (PopupInstance != null) { PopupInstance.Close(); PopupInstance = null; }
                else { await Shell.Current.GoToAsync(".."); }
            }
        }

        [RelayCommand]
        public async Task Cancel()
        {
            _isInitialized = false;
            if (PopupInstance != null) { PopupInstance.Close(); PopupInstance = null; }
            else { await Shell.Current.GoToAsync(".."); }
        }

        private async Task<bool> SaveInternal()
        {
            if (string.IsNullOrWhiteSpace(TransactionName) || _currentType == null || SelectedSource == null)
            {
                await App.Current.MainPage.DisplayAlert("Aviso", "Preencha os campos obrigatórios.", "OK");
                return false;
            }

            decimal.TryParse(Amount.Replace("R$", "").Replace(".", "").Trim(), System.Globalization.NumberStyles.Any, new System.Globalization.CultureInfo("pt-BR"), out decimal decimalAmount);
            Guid finalAccountId = SelectedSource.Id;
            Guid? finalCreditCardId = null;

            if (SelectedSource.Type == "CREDIT_CARD")
            {
                finalCreditCardId = SelectedSource.Id;
                finalAccountId = SelectedSource.LinkedAccountId ?? Guid.Empty;
            }

            RecurrenceFrequency? freq = null;
            if (IsFixed)
            {
                freq = SelectedPeriod switch
                {
                    "Semanal" => RecurrenceFrequency.WEEKLY,
                    "Anual" => RecurrenceFrequency.YEARLY,
                    _ => RecurrenceFrequency.MONTHLY
                };
            }

            Transaction t = new Transaction
            {
                Id = _editingTransaction?.Id,
                Name = TransactionName,
                Description = Description,
                Type = _currentType.Value,
                Amount = decimalAmount,
                Date = new DateTimeOffset(Date).ToUnixTimeMilliseconds(),
                Paid = IsPaid,
                AccountId = finalAccountId,
                CreditCardId = finalCreditCardId,
                CategoryId = SelectedCategory?.Id ?? Guid.Empty,
                IsFixed = IsFixed,
                RecurrenceFrequency = freq,
                Installments = IsRecurring ? Installments : 1
            };

            bool result;
            if (_editingTransaction == null)
            {
                result = await _transactionService.SaveTransactionAsync(t);
            }
            else
            {
                result = await _transactionService.UpdateTransactionAsync(t.Id.Value, t, false);
            }

            if (result) WeakReferenceMessenger.Default.Send(new GlobalRefreshMessage());
            return result;
        }
    }

    public class PaymentSource
    {
        public Guid Id { get; set; }
        public Guid? LinkedAccountId { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Icon { get; set; }
        public string Color { get; set; }
    }
}