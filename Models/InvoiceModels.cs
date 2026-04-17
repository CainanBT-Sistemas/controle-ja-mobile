using System.Text.Json.Serialization;
using System.Globalization;

namespace controle_ja_mobile.Models
{
    public class AdvanceablePurchaseDTO
    {
        [JsonPropertyName("purchaseId")] public Guid PurchaseId { get; set; }
        [JsonPropertyName("name")] public string Name { get; set; }
        [JsonPropertyName("maxInstallmentsAvailable")] public int MaxInstallmentsAvailable { get; set; }
    }

    public class InvoiceItemDTO
    {
        [JsonPropertyName("id")] public Guid Id { get; set; }
        [JsonPropertyName("date")] public long Date { get; set; }
        [JsonPropertyName("name")] public string Name { get; set; }
        [JsonPropertyName("currentInstallment")] public int CurrentInstallment { get; set; }
        [JsonPropertyName("totalInstallmentsPlan")] public int TotalInstallmentsPlan { get; set; }
        [JsonPropertyName("amount")] public decimal Amount { get; set; }

        [JsonIgnore]
        public string FormattedDate => DateTimeOffset.FromUnixTimeMilliseconds(Date).ToLocalTime().ToString("dd MMM").ToUpper();

        [JsonIgnore]
        public string FormattedAmount => Amount.ToString("C", new CultureInfo("pt-BR"));

        [JsonIgnore]
        public string InstallmentText => TotalInstallmentsPlan > 1 ? $"{CurrentInstallment}/{TotalInstallmentsPlan}" : "";

        [JsonIgnore]
        public string DisplayName => string.IsNullOrEmpty(InstallmentText) ? Name : $"{Name} ({InstallmentText})";

        [JsonIgnore]
        public Color AmountColor => Amount < 0 ? Color.FromArgb("#00E676") : Colors.White;
    }

    public class InvoiceDetailsDTO
    {
        [JsonPropertyName("invoiceId")] public Guid InvoiceId { get; set; }
        [JsonPropertyName("cardId")] public Guid CardId { get; set; }
        [JsonPropertyName("cardName")] public string CardName { get; set; }
        [JsonPropertyName("month")] public int Month { get; set; }
        [JsonPropertyName("year")] public int Year { get; set; }
        [JsonPropertyName("totalAmount")] public decimal TotalAmount { get; set; }
        [JsonPropertyName("expirationDate")] public long ExpirationDate { get; set; }
        [JsonPropertyName("closeDate")] public long CloseDate { get; set; }
        [JsonPropertyName("status")] public string Status { get; set; }
        [JsonPropertyName("items")] public List<InvoiceItemDTO> Items { get; set; } = new();
    }

    public class RefundRequestDTO
    {
        [JsonPropertyName("installmentId")] public Guid InstallmentId { get; set; }
        [JsonPropertyName("refundAmount")] public decimal RefundAmount { get; set; }
    }

    public class AdvanceRequestDTO
    {
        [JsonPropertyName("purchaseId")] public Guid PurchaseId { get; set; }
        [JsonPropertyName("quantityToAdvance")] public int QuantityToAdvance { get; set; }
        [JsonPropertyName("discountAmount")] public decimal DiscountAmount { get; set; }
    }
}