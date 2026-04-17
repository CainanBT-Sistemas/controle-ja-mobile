using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace controle_ja_mobile.Models
{
    public class ChartData
    {
        [JsonPropertyName("label")]
        public string Label { get; set; } = string.Empty;

        [JsonPropertyName("value")]
        public decimal Value { get; set; }

        [JsonPropertyName("color")]
        public string Color { get; set; }
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

    public class DashboardAlert
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }

        [JsonPropertyName("dueDate")]
        public long DueDate { get; set; }

        [JsonPropertyName("icon")]
        public string Icon { get; set; }

        [JsonPropertyName("color")]
        public string Color { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonIgnore]
        public string FormattedAmount => Amount.ToString("C", new System.Globalization.CultureInfo("pt-BR"));

        [JsonIgnore]
        public string FormattedDate => DateTimeOffset.FromUnixTimeMilliseconds(DueDate).DateTime.ToLocalTime().ToString("dd/MM/yyyy");
    }

    public class DashboardFullSummary
    {
        [JsonPropertyName("availableBalance")]
        public decimal AvailableBalance { get; set; }

        [JsonPropertyName("projectedBalance")]
        public decimal ProjectedBalance { get; set; }

        [JsonPropertyName("projectedPayables")]
        public decimal ProjectedPayables { get; set; }

        [JsonPropertyName("projectedVariables")]
        public decimal ProjectedVariables { get; set; }

        [JsonPropertyName("pendingPayables")]
        public ObservableCollection<DashboardAlert> PendingPayables { get; set; }

        [JsonPropertyName("pendingReceivables")]
        public ObservableCollection<DashboardAlert> PendingReceivables { get; set; }

        [JsonPropertyName("pendingInvoices")]
        public ObservableCollection<DashboardAlert> PendingInvoices { get; set; }

        // === NOVOS CAMPOS DO DASHBOARD ESTILO MOBILLS ===

        [JsonPropertyName("accounts")]
        public ObservableCollection<Account> Accounts { get; set; } = new();

        [JsonPropertyName("creditCards")]
        public ObservableCollection<CreditCard> CreditCards { get; set; } = new();

        [JsonPropertyName("overduePayables")]
        public ObservableCollection<DashboardAlert> OverduePayables { get; set; } = new();

        [JsonPropertyName("overdueInvoices")]
        public ObservableCollection<DashboardAlert> OverdueInvoices { get; set; } = new();
    }
}