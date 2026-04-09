using System;
using System.Text.Json.Serialization;

namespace controle_ja_mobile.Models
{
    public class Transaction
    {
        [JsonPropertyName("id")]
        public Guid? Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("type")]
        public TransactionType Type { get; set; }

        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }

        [JsonPropertyName("date")]
        public long Date { get; set; }

        [JsonPropertyName("paid")]
        public bool Paid { get; set; }

        [JsonPropertyName("accountId")]
        public Guid AccountId { get; set; }

        [JsonPropertyName("targetAccountId")]
        public Guid? TargetAccountId { get; set; }

        [JsonPropertyName("categoryId")]
        public Guid CategoryId { get; set; }

        [JsonPropertyName("categoryName")]
        public string? CategoryName { get; set; }

        [JsonPropertyName("accountName")]
        public string? AccountName { get; set; }

        [JsonPropertyName("isRecurring")]
        public bool IsRecurring { get; set; }

        [JsonPropertyName("isFixed")]
        public bool IsFixed { get; set; }

        [JsonPropertyName("recurrenceFrequency")]
        public RecurrenceFrequency? RecurrenceFrequency { get; set; }

        [JsonPropertyName("recurrenceEndDate")]
        public long? RecurrenceEndDate { get; set; }

        [JsonPropertyName("recurrenceRuleId")]
        public Guid? RecurrenceRuleId { get; set; }

        private int _installments = 1;
        [JsonPropertyName("installments")]
        public int Installments
        {
            get => _installments;
            set => _installments = value < 1 ? 1 : value;
        }

        [JsonPropertyName("creditCardId")]
        public Guid? CreditCardId { get; set; }

        [JsonPropertyName("vehicleId")]
        public Guid? VehicleId { get; set; }

        [JsonPropertyName("vehicleName")]
        public string? VehicleName { get; set; }

        [JsonPropertyName("currentOdometer")]
        public decimal? CurrentOdometer { get; set; }

        [JsonPropertyName("liters")]
        public double? Liters { get; set; }

        [JsonPropertyName("fuelType")]
        public FuelType? FuelType { get; set; }

        [JsonPropertyName("efficiency")]
        public double? Efficiency { get; set; }

        [JsonIgnore]
        public string FormattedAmount => string.Format("{0:C}", Amount);

        [JsonIgnore]
        public DateTime DateTimeObject => DateTimeOffset.FromUnixTimeMilliseconds(Date).DateTime.ToLocalTime();

        // --- MÁGICA VISUAL DO EXTRATO BANCÁRIO ---

        [JsonIgnore]
        public string ExtratoIcon => Type switch
        {
            TransactionType.RECEITA => "arrow_downward",
            TransactionType.DESPESA => "trending_up",
            TransactionType.TRANSFERENCIA => "swap_horiz",
            TransactionType.TRANSFERENCIA_ENTRADA => "swap_horiz",
            TransactionType.TRANSFERENCIA_SAIDA => "swap_horiz",
            TransactionType.PAGAMENTO_FATURA => "receipt_long",
            _ => "account_balance_wallet"
        };

        [JsonIgnore]
        public string ExtratoAmount => (Type == TransactionType.RECEITA || Type == TransactionType.TRANSFERENCIA_ENTRADA)
            ? FormattedAmount
            : $"- {FormattedAmount}"; // Adiciona o negativo visual nas saídas

        // AJUSTE: COR VERMELHA PARA AS SAÍDAS E VERDE PARA ENTRADAS
        [JsonIgnore]
        public string ExtratoAmountColor => (Type == TransactionType.RECEITA || Type == TransactionType.TRANSFERENCIA_ENTRADA)
            ? "#00E676"  // Verde
            : "#EF4444"; // Vermelho

        [JsonIgnore]
        public string ColorHex => (Type == TransactionType.RECEITA || Type == TransactionType.TRANSFERENCIA_ENTRADA)
            ? "#00E676"
            : "#EF4444";

        // AJUSTE: LEGENDA INTELIGENTE DA CONTA
        [JsonIgnore]
        public string SubtitleDisplay
        {
            get
            {
                string catName = string.IsNullOrEmpty(CategoryName) ? "Outros" : CategoryName;
                string accName = string.IsNullOrEmpty(AccountName) ? "Conta" : AccountName;

                if (Type == TransactionType.TRANSFERENCIA_SAIDA)
                    return $"Saiu de: {accName}";

                if (Type == TransactionType.TRANSFERENCIA_ENTRADA)
                    return $"Entrou em: {accName}";

                return $"{catName} • {accName}";
            }
        }
    }
}