using System.Text.Json.Serialization;

namespace controle_ja_mobile.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum TransactionType
    {
        RECEITA,
        DESPESA,
        TRANSFERENCIA,
        TRANSFERENCIA_ENTRADA,
        TRANSFERENCIA_SAIDA,
        PAGAMENTO_FATURA
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum AccountType
    {
        WALLET,
        BANK,
        SAVINGS,
        CREDIT_CARD
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum FuelType
    {
        GASOLINA,
        ETANOL,
        DIESEL,
        GNV,
        ELETRICO,
        OUTRO
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum UserRole
    {
        USER,
        ADMIN,
        MANAGER
    }

    // NOVO: Necessário para as Transações Fixas (Netflix, etc)
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum RecurrenceFrequency
    {
        DAILY,
        WEEKLY,
        BIWEEKLY,
        MONTHLY,
        YEARLY
    }
}