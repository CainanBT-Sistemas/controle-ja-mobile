using System.Text.Json.Serialization;
using Microcharts;
using System.Globalization; // NOVO: IMPORT NECESSÁRIO

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

        // FIX: Força formatação brasileira de inteiro (Ex: 179.327 km, not 179,327 km)
        [JsonIgnore]
        public string FormattedOdometer => CurrentOdometer.ToString("N0", new CultureInfo("pt-BR")) + " km";

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

        [JsonIgnore]
        public string MonthlyCost { get; set; } = "R$ 0,00";

        [JsonIgnore]
        public Chart ExpensesChart { get; set; }
    }
}