using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json.Serialization;

namespace controle_ja_mobile.Models
{
    public partial class Account : ObservableObject
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public AccountType Type { get; set; }

        [JsonPropertyName("institution")]
        public string Institution { get; set; } = string.Empty;

        [JsonPropertyName("currentBalance")]
        public decimal Balance { get; set; }

        // IMPORTANTE: Adicione esses campos no seu Java (AccountDTO e AccountResponseDTO)
        [JsonPropertyName("icon")]
        public string Icon { get; set; } = "account_balance_wallet";

        [JsonPropertyName("color")]
        public string Color { get; set; } = "#42A5F5";

        [JsonPropertyName("isDefault")]
        public bool IsDefault { get; set; }

        [JsonIgnore]
        public string FormattedBalance => $"{(Balance >= 0 ? "" : "-")}R$ {Math.Abs(Balance):N2}";
    }
}