using System.Text.Json.Serialization;
using Microcharts; // Necessário para o gráfico

namespace controle_ja_mobile.Models
{
    public class CreditCard
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } // Ex: Nubank

        [JsonPropertyName("limit")]
        public decimal TotalLimit { get; set; }

        [JsonPropertyName("currentLimit")]
        public decimal CurrentLimit { get; set; } // Limite Disponível

        [JsonPropertyName("closeDay")]
        public int CloseDay { get; set; }

        [JsonPropertyName("bestDay")]
        public int BestDay { get; set; }

        // --- Helpers Visuais ---

        [JsonIgnore]
        public decimal UsedAmount => TotalLimit - CurrentLimit;

        [JsonIgnore]
        public string FormattedTotalLimit => $"Limite: {TotalLimit:C}";

        [JsonIgnore]
        public string FormattedAvailable => $"{CurrentLimit:C}";

        [JsonIgnore]
        public string FormattedUsed => $"{UsedAmount:C}";

        [JsonIgnore]
        public string InvoiceInfo => $"Fecha dia {CloseDay} • Melhor dia {BestDay}";

        // Calcula porcentagem usada para a barra de progresso (0 a 1)
        [JsonIgnore]
        public double LimitProgress
        {
            get
            {
                if (TotalLimit == 0) return 0;
                return (double)(TotalLimit - CurrentLimit) / (double)TotalLimit;
            }
        }

        [JsonIgnore]
        public string UsedPercentageText => $"{LimitProgress * 100:F0}% usado";

        // --- Propriedades para o Gráfico ---
        [JsonIgnore]
        public Chart CategoryChart { get; set; }

        [JsonIgnore]
        public bool HasChartData { get; set; }
    }
}