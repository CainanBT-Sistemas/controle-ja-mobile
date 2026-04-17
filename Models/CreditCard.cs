using System.Text.Json.Serialization;
using System.Globalization;
using Microcharts;

namespace controle_ja_mobile.Models
{
    public class CreditCard
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("accountId")]
        public Guid AccountId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("totalLimit")]
        public decimal TotalLimit { get; set; }

        [JsonPropertyName("currentLimit")]
        public decimal CurrentLimit { get; set; }

        [JsonPropertyName("closeDay")]
        public int CloseDay { get; set; }

        [JsonPropertyName("bestDay")]
        public int BestDay { get; set; }

        [JsonPropertyName("icon")]
        public string Icon { get; set; } = "credit_card";

        [JsonPropertyName("color")]
        public string Color { get; set; } = "#9C27B0";

        [JsonIgnore]
        public decimal UsedAmount => Math.Max(0, TotalLimit - CurrentLimit);

        [JsonIgnore]
        public string FormattedTotalLimit => $"R$ {TotalLimit.ToString("N2", new CultureInfo("pt-BR"))}";

        [JsonIgnore]
        public string FormattedAvailable => $"R$ {CurrentLimit.ToString("N2", new CultureInfo("pt-BR"))}";

        [JsonIgnore]
        public string FormattedUsed => $"R$ {UsedAmount.ToString("N2", new CultureInfo("pt-BR"))}";

        [JsonIgnore]
        public string InvoiceInfo => $"Fecha dia {CloseDay} • Vence dia {BestDay}";

        [JsonIgnore]
        public double LimitProgress => TotalLimit == 0 ? 0 : (double)UsedAmount / (double)TotalLimit;

        [JsonIgnore]
        public string UsedPercentageText => $"{LimitProgress * 100:F0}% utilizado";

        [JsonIgnore]
        public Chart CategoryChart { get; set; }

        [JsonIgnore]
        public bool HasChartData { get; set; }
    }
}