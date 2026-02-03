using System.Text.Json.Serialization;
using Microcharts; // Necessário para o gráfico

namespace controle_ja_mobile.Models
{
    public class Vehicle
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("brand")]
        public string Brand { get; set; } = string.Empty;

        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;

        [JsonPropertyName("year")]
        public int Year { get; set; }

        [JsonPropertyName("plate")]
        public string Plate { get; set; } = string.Empty;

        [JsonPropertyName("currentOdometer")]
        public decimal CurrentOdometer { get; set; }

        // Campos de Inteligência
        [JsonPropertyName("avgGasoline")]
        public double? AvgGasoline { get; set; }

        [JsonPropertyName("avgEthanol")]
        public double? AvgEthanol { get; set; }

        [JsonIgnore]
        public string FullDescription => $"{Brand} {Model} - {Year}";

        [JsonIgnore]
        public string FormattedOdometer => $"{CurrentOdometer:N0} km";

        [JsonIgnore]
        public string ConsumptionInfo
        {
            get
            {
                string info = "";
                if (AvgGasoline > 0) info += $"Gas: {AvgGasoline:F1} km/L\n";
                if (AvgEthanol > 0) info += $"Eta: {AvgEthanol:F1} km/L";
                return string.IsNullOrWhiteSpace(info) ? "Sem dados" : info.Trim();
            }
        }

        // Simulação de Custo Mensal (futuramente virá da API)
        [JsonIgnore]
        public string MonthlyCost { get; set; } = "R$ 0,00";

        // --- Gráfico de Histórico ---
        [JsonIgnore]
        public Chart ExpensesChart { get; set; }
    }
}