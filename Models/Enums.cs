using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace controle_ja_mobile.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum TransactionType
    {
        RECEITA,
        DESPESA,
        TRANSFERENCIA,
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
}
