using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace controle_ja_mobile.Models
{
    public class CreditCard
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } // Ex: Nubank

        [JsonPropertyName("totalLimit")]
        public decimal TotalLimit { get; set; }

        [JsonPropertyName("currentLimit")]
        public decimal CurrentLimit { get; set; } // Limite Disponível

        [JsonPropertyName("closeDay")]
        public int CloseDay { get; set; }

        [JsonPropertyName("bestDay")]
        public int BestDay { get; set; }

        // --- Helpers Visuais ---
        [JsonIgnore]
        public string FormattedTotalLimit => $"Limite Total: {TotalLimit:C}";

        [JsonIgnore]
        public string FormattedAvailable => $"{CurrentLimit:C}";

        [JsonIgnore]
        public string InvoiceInfo => $"Fecha dia {CloseDay} • Melhor dia {BestDay}";

        // Calcula porcentagem usada para a barra de progresso (0 a 1)
        [JsonIgnore]
        public double LimitProgress
        {
            get
            {
                if (TotalLimit == 0) return 0;
                // Se Total = 1000 e Atual = 200, Usado = 800. Progresso = 0.8
                return (double)(TotalLimit - CurrentLimit) / (double)TotalLimit;
            }
        }

        [JsonIgnore]
        public string UsedPercentageText => $"{LimitProgress * 100:F0}% usado";
    }
}
