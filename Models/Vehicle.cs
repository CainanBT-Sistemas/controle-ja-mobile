using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

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

        [JsonPropertyName("currentOdometer")]
        public decimal CurrentOdometer { get; set; }

        // Campos de Inteligência (podem vir nulos)
        [JsonPropertyName("avgGasoline")]
        public double? AvgGasoline { get; set; }

        [JsonPropertyName("avgEthanol")]
        public double? AvgEthanol { get; set; }

        [JsonIgnore]
        public string FullDescription => $"{Brand} {Model}";

        [JsonIgnore]
        public string FormattedOdometer => $"{CurrentOdometer:N0} km";

        [JsonIgnore]
        public string ConsumptionInfo
        {
            get
            {
                string info = "";
                // Verifica se tem média e formata
                if (AvgGasoline > 0) info += $"Gas: {AvgGasoline:F1} km/L  ";
                if (AvgEthanol > 0) info += $"Eta: {AvgEthanol:F1} km/L";

                return string.IsNullOrWhiteSpace(info) ? "Sem dados de consumo" : info;
            }
        }
    }
}
