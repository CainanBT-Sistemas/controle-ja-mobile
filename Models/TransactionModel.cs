using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace controle_ja_mobile.Models
{
    public class TransactionModel
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

        [JsonPropertyName("categoryId")]
        public Guid CategoryId { get; set; }

        [JsonPropertyName("categoryName")]
        public string? CategoryName { get; set; }

        [JsonPropertyName("accountName")]
        public string? AccountName { get; set; }

        [JsonPropertyName("vehicleId")]
        public Guid? VehicleId { get; set; }

        [JsonPropertyName("vehicleName")]
        public string? VehicleName { get; set; }

        [JsonPropertyName("currentOdometer")]
        public decimal? CurrentOdometer { get; set; }

        [JsonPropertyName("liters")]
        public double? Liters { get; set; }

        [JsonPropertyName("fuelType")]
        public FuelType FuelType { get; set; }

        [JsonPropertyName("efficiency")]
        public double? Efficiency { get; set; }

        [JsonIgnore]
        public string ColorHex => Type == TransactionType.RECEITA ? "#00E676" : "#EF4444";

        [JsonIgnore]
        public string FormattedAmount => string.Format("{0:C}", Amount);

        [JsonIgnore]
        public DateTime DateTimeObject
        {
            get
            {
                // Converte timestamp (ms) para DateTime
                return DateTimeOffset.FromUnixTimeMilliseconds(Date).DateTime.ToLocalTime();
            }
        }
    }
}
