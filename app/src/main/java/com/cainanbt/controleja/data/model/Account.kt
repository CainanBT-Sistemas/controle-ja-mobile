package com.cainanbt.controleja.data.model

import com.google.gson.annotations.SerializedName
import java.text.NumberFormat
import java.util.Locale
import java.util.UUID

data class Account(
    @SerializedName("id") val id: String = UUID.randomUUID().toString(),
    @SerializedName("name") val name: String = "",
    @SerializedName("type") val type: AccountType = AccountType.BANK,
    @SerializedName("institution") val institution: String = "",
    @SerializedName("currentBalance") val balance: Double = 0.0,
    @SerializedName("icon") val icon: String = "account_balance_wallet",
    @SerializedName("color") val color: String = "#42A5F5",
    @SerializedName("isDefault") val isDefault: Boolean = false
) {
    val formattedBalance: String
        get() = NumberFormat.getCurrencyInstance(Locale("pt", "BR")).format(balance)

    val typeDisplay: String
        get() = when (type) {
            AccountType.BANK -> "Conta Bancária"
            AccountType.WALLET -> "Carteira"
            AccountType.SAVINGS -> "Poupança"
            AccountType.CREDIT_CARD -> "Cartão de Crédito"
        }
}
