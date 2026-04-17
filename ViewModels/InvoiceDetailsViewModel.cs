using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using controle_ja_mobile.Models;
using controle_ja_mobile.Services;
using System.Collections.ObjectModel;
using System.Globalization;

namespace controle_ja_mobile.ViewModels
{
    [QueryProperty(nameof(CardId), "cardId")]
    [QueryProperty(nameof(TargetMonth), "month")]
    [QueryProperty(nameof(TargetYear), "year")]
    public partial class InvoiceDetailsViewModel : BaseViewModel
    {
        private readonly InvoiceService _invoiceService;
        private readonly AccountService _accountService;
        private readonly CreditCardService _creditCardService;
        private readonly TransactionService _transactionService;
        private readonly CategoryService _categoryService;

        private readonly CultureInfo _culture = new CultureInfo("pt-BR");

        [ObservableProperty] private string cardId;
        [ObservableProperty] private string targetMonth;
        [ObservableProperty] private string targetYear;

        [ObservableProperty] private string title = "Fatura";
        [ObservableProperty] private DateTime currentDate;
        [ObservableProperty] private string currentMonthDisplay;

        [ObservableProperty] private decimal rawTotalAmount;
        [ObservableProperty] private string totalAmount = "R$ 0,00";
        [ObservableProperty] private string statusText = "CARREGANDO...";
        [ObservableProperty] private Color statusColor = Colors.Gray;
        [ObservableProperty] private string datesInfo = "Fechamento • Vencimento";

        [ObservableProperty] private bool isInvoiceEmpty = true;
        [ObservableProperty] private bool hasInvoiceData = false;
        [ObservableProperty] private bool isPayButtonVisible = false;

        private Guid? _currentInvoiceId;

        public ObservableCollection<InvoiceItemDTO> InvoiceItems { get; } = new();

        public InvoiceDetailsViewModel(InvoiceService invoiceService, AccountService accountService, CreditCardService creditCardService, TransactionService transactionService, CategoryService categoryService)
        {
            _invoiceService = invoiceService;
            _accountService = accountService;
            _creditCardService = creditCardService;
            _transactionService = transactionService;
            _categoryService = categoryService;
            CurrentDate = DateTime.Now;
        }

        partial void OnCardIdChanged(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                if (!string.IsNullOrEmpty(TargetMonth) && !string.IsNullOrEmpty(TargetYear))
                {
                    CurrentDate = new DateTime(int.Parse(TargetYear), int.Parse(TargetMonth), 1);
                }
                UpdateMonthDisplay();
            }
        }

        [RelayCommand] public void NextMonth() { CurrentDate = CurrentDate.AddMonths(1); UpdateMonthDisplay(); }
        [RelayCommand] public void PreviousMonth() { CurrentDate = CurrentDate.AddMonths(-1); UpdateMonthDisplay(); }

        private void UpdateMonthDisplay()
        {
            CurrentMonthDisplay = CurrentDate.ToString("MMMM 'de' yyyy", _culture).ToUpper();
            _ = LoadInvoiceDataAsync();
        }

        [RelayCommand]
        public async Task LoadInvoiceDataAsync()
        {
            if (string.IsNullOrEmpty(CardId) || !Guid.TryParse(CardId, out Guid parsedCardId)) return;

            await ExecuteWithErrorHandlingAsync(async () =>
            {
                var invoice = await _invoiceService.GetInvoiceDetailsAsync(parsedCardId, CurrentDate.Month, CurrentDate.Year);

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    InvoiceItems.Clear();

                    if (invoice != null)
                    {
                        _currentInvoiceId = invoice.InvoiceId; // Se for nulo, a fatura fantasma assumiu
                        Title = invoice.CardName;
                        RawTotalAmount = invoice.TotalAmount;
                        TotalAmount = invoice.TotalAmount.ToString("C", _culture);

                        DateTime closeDate = DateTimeOffset.FromUnixTimeMilliseconds(invoice.CloseDate).ToLocalTime().DateTime;
                        DateTime expDate = DateTimeOffset.FromUnixTimeMilliseconds(invoice.ExpirationDate).ToLocalTime().DateTime;
                        DatesInfo = $"Fecha em {closeDate:dd/MM} • Vence em {expDate:dd/MM}";

                        StatusText = invoice.Status;
                        StatusColor = invoice.Status switch
                        {
                            "PAGA" => Color.FromArgb("#00E676"),
                            "ABERTA" => Color.FromArgb("#3B82F6"),
                            "FECHADA" => Color.FromArgb("#FF9800"),
                            "ATRASADA" => Color.FromArgb("#FF5252"),
                            _ => Colors.Gray
                        };

                        foreach (var item in invoice.Items) InvoiceItems.Add(item);

                        HasInvoiceData = InvoiceItems.Any();
                        IsInvoiceEmpty = !HasInvoiceData;
                        IsPayButtonVisible = (invoice.Status == "ABERTA" || invoice.Status == "FECHADA" || invoice.Status == "ATRASADA") && RawTotalAmount > 0;
                    }
                });
            });
        }

        // ==========================================
        // FLUXO DE PAGAMENTO DE FATURA (PARCIAL/TOTAL)
        // ==========================================
        [RelayCommand]
        public async Task PayInvoice()
        {
            if (_currentInvoiceId == null || RawTotalAmount <= 0) return;

            var accounts = await _accountService.GetAccountsAsync();
            var validSourceAccounts = accounts.Where(a => a.Type == AccountType.BANK || a.Type == AccountType.WALLET).ToList();

            if (!validSourceAccounts.Any())
            {
                await Shell.Current.DisplayAlert("Aviso", "Cadastre uma conta corrente ou carteira com saldo para pagar a fatura.", "OK");
                return;
            }

            decimal payAmount = 0;
            string defaultAmountFormatted = RawTotalAmount.ToString("N2", _culture);

            // 1. Menu rápido de opção de pagamento
            string paymentOption = await Shell.Current.DisplayActionSheet(
                "Como deseja pagar a fatura?",
                "Cancelar",
                null,
                $"Valor Total (R$ {defaultAmountFormatted})",
                "Outro Valor");

            if (paymentOption == "Cancelar" || string.IsNullOrEmpty(paymentOption)) return;

            if (paymentOption.StartsWith("Valor Total"))
            {
                payAmount = RawTotalAmount;
            }
            else if (paymentOption == "Outro Valor")
            {
                // 2. Só abre a digitação se quiser pagar parcial
                string amountStr = await Shell.Current.DisplayPromptAsync("Pagar Fatura", "Qual valor deseja pagar?", "Avançar", "Cancelar", defaultAmountFormatted, keyboard: Keyboard.Numeric);
                if (string.IsNullOrWhiteSpace(amountStr)) return;

                if (!decimal.TryParse(amountStr, NumberStyles.Any, _culture, out payAmount) || payAmount <= 0)
                {
                    await Shell.Current.DisplayAlert("Erro", "Valor inválido.", "OK");
                    return;
                }
            }

            // 3. Escolhe a conta de origem
            var accountNames = validSourceAccounts.Select(a => a.Name).ToArray();
            string selectedAccName = await Shell.Current.DisplayActionSheet("De qual conta sairá o dinheiro?", "Cancelar", null, accountNames);
            if (selectedAccName == "Cancelar" || string.IsNullOrEmpty(selectedAccName)) return;

            var selectedAcc = validSourceAccounts.First(a => a.Name == selectedAccName);

            // 4. Confirmação final antes de bater no banco
            bool confirm = await Shell.Current.DisplayAlert(
                "Confirmar Pagamento",
                $"Deseja pagar a fatura no valor de {payAmount.ToString("C", _culture)}\nusando o saldo de '{selectedAcc.Name}'?",
                "Sim, Pagar",
                "Cancelar");

            if (!confirm) return;

            // 5. Envia para a API
            await ExecuteWithErrorHandlingAsync(async () =>
            {
                var cards = await _creditCardService.GetCreditCardsAsync();
                var thisCard = cards.FirstOrDefault(c => c.Id == Guid.Parse(CardId));
                if (thisCard == null) return;

                var categories = await _categoryService.GetCategoriesAsync();
                var cat = categories.FirstOrDefault(c => c.Type == TransactionType.DESPESA) ?? categories.First();

                var tx = new Transaction
                {
                    Name = $"Pagamento Fatura {Title}",
                    Type = TransactionType.PAGAMENTO_FATURA,
                    Amount = payAmount,
                    Date = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    Paid = true,
                    AccountId = selectedAcc.Id,
                    TargetAccountId = thisCard.AccountId,
                    TargetInvoiceId = _currentInvoiceId,
                    CategoryId = cat.Id,
                    IsFixed = false,
                    Installments = 1
                };

                bool success = await _transactionService.SaveTransactionAsync(tx);

                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    if (success)
                    {
                        await Shell.Current.DisplayAlert("Sucesso", "Pagamento registrado com sucesso!", "OK");
                        await LoadInvoiceDataAsync();
                    }
                    else
                    {
                        await Shell.Current.DisplayAlert("Erro", "Falha ao registrar pagamento.", "OK");
                    }
                });
            });
        }

        // ==========================================
        // FLUXO DE ADIANTAMENTO (MENU SUPERIOR)
        // ==========================================
        [RelayCommand]
        public async Task OpenMenuActions()
        {
            string action = await Shell.Current.DisplayActionSheet("Opções da Fatura", "Cancelar", null, "Adiantar Parcelas");
            if (action == "Adiantar Parcelas") await ProcessAdvanceFlow();
        }

        private async Task ProcessAdvanceFlow()
        {
            if (string.IsNullOrEmpty(CardId)) return;

            // Busca as compras que podem ser adiantadas usando o serviço novo
            var advanceables = await _invoiceService.GetAdvanceablePurchasesAsync(Guid.Parse(CardId), CurrentDate.Month, CurrentDate.Year);

            if (advanceables == null || !advanceables.Any())
            {
                await Shell.Current.DisplayAlert("Aviso", "Não há compras parceladas em faturas futuras para adiantar.", "OK");
                return;
            }

            var options = advanceables.Select(a => a.Name).ToArray();
            string selectedName = await Shell.Current.DisplayActionSheet("Qual compra deseja adiantar?", "Cancelar", null, options);

            if (selectedName == "Cancelar" || string.IsNullOrEmpty(selectedName)) return;

            var selectedPurchase = advanceables.First(a => a.Name == selectedName);

            string qtyStr = await Shell.Current.DisplayPromptAsync("Quantidade", $"Quantas parcelas deseja adiantar?\n(Máx disponível: {selectedPurchase.MaxInstallmentsAvailable})", "Avançar", "Cancelar", "1", keyboard: Keyboard.Numeric);

            if (string.IsNullOrWhiteSpace(qtyStr) || !int.TryParse(qtyStr, out int qty) || qty <= 0 || qty > selectedPurchase.MaxInstallmentsAvailable)
            {
                await Shell.Current.DisplayAlert("Erro", "Quantidade inválida.", "OK");
                return;
            }

            string discountStr = await Shell.Current.DisplayPromptAsync("Desconto", "Houve algum desconto (em R$) por adiantar?\n(Deixe 0 se não houve)", "Confirmar", "Cancelar", "0", keyboard: Keyboard.Numeric);
            if (string.IsNullOrWhiteSpace(discountStr)) return;

            decimal.TryParse(discountStr, NumberStyles.Any, _culture, out decimal discount);

            await ExecuteWithErrorHandlingAsync(async () =>
            {
                bool success = await _invoiceService.AdvanceInstallmentsAsync(_currentInvoiceId.Value, new AdvanceRequestDTO { PurchaseId = selectedPurchase.PurchaseId, QuantityToAdvance = qty, DiscountAmount = discount });

                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    if (success) { await Shell.Current.DisplayAlert("Sucesso", "Parcelas adiantadas com sucesso!", "OK"); await LoadInvoiceDataAsync(); }
                    else { await Shell.Current.DisplayAlert("Erro", "Falha ao adiantar parcelas.", "OK"); }
                });
            });
        }

        // ==========================================
        // FLUXO DE ESTORNO (CLIQUE NO ITEM)
        // ==========================================
        [RelayCommand]
        public async Task OpenItemActions(InvoiceItemDTO item)
        {
            if (_currentInvoiceId == null) return;
            if (item.Amount < 0) return; // Impede interagir com estornos passados ou descontos

            string action = await Shell.Current.DisplayActionSheet($"Opções: {item.Name}", "Cancelar", null, "Lançar Estorno");
            if (action == "Lançar Estorno") await ProcessRefundFlow(item);
        }

        private async Task ProcessRefundFlow(InvoiceItemDTO item)
        {
            string amountStr = await Shell.Current.DisplayPromptAsync("Estornar", $"Qual o valor do estorno para '{item.Name}'?\n(Use vírgula para centavos)", "Confirmar", "Cancelar", item.Amount.ToString("F2", _culture), keyboard: Keyboard.Numeric);
            if (string.IsNullOrWhiteSpace(amountStr)) return;

            if (decimal.TryParse(amountStr, NumberStyles.Any, _culture, out decimal refundAmount) && refundAmount > 0)
            {
                await ExecuteWithErrorHandlingAsync(async () =>
                {
                    bool success = await _invoiceService.ProcessRefundAsync(_currentInvoiceId.Value, new RefundRequestDTO { InstallmentId = item.Id, RefundAmount = refundAmount });

                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        if (success) { await Shell.Current.DisplayAlert("Sucesso", "Estorno lançado na fatura e limite restaurado!", "OK"); await LoadInvoiceDataAsync(); }
                        else { await Shell.Current.DisplayAlert("Erro", "Falha ao lançar estorno.", "OK"); }
                    });
                });
            }
        }

        [RelayCommand]
        public async Task GoBack() => await NavigateToAsync("..");
    }
}