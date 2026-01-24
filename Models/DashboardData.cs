using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace controle_ja_mobile.Models
{
    public class ChartData
    {
        [JsonPropertyName("label")]
        public string Label { get; set; } = string.Empty;

        [JsonPropertyName("value")]
        public decimal Value { get; set; }
    }

    public class FinancialSummary
    {
        [JsonPropertyName("totalIncome")]
        public decimal TotalIncome { get; set; }

        [JsonPropertyName("totalExpense")]
        public decimal TotalExpense { get; set; }

        [JsonPropertyName("balance")]
        public decimal Balance { get; set; }
    }
}
