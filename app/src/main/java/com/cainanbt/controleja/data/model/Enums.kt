package com.cainanbt.controleja.data.model

enum class TransactionType {
    RECEITA,
    DESPESA,
    TRANSFERENCIA,
    TRANSFERENCIA_ENTRADA,
    TRANSFERENCIA_SAIDA,
    PAGAMENTO_FATURA
}

enum class AccountType {
    WALLET,
    BANK,
    SAVINGS,
    CREDIT_CARD
}

enum class FuelType {
    GASOLINA,
    ETANOL,
    DIESEL,
    GNV,
    ELETRICO,
    OUTRO
}

enum class UserRole {
    USER,
    ADMIN,
    MANAGER
}

enum class RecurrenceFrequency {
    DAILY,
    WEEKLY,
    BIWEEKLY,
    MONTHLY,
    YEARLY
}
