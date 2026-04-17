package com.cainanbt.controleja.data.model

import com.google.gson.annotations.SerializedName
import java.text.NumberFormat
import java.text.SimpleDateFormat
import java.util.Date
import java.util.Locale
import java.util.UUID

data class Transaction(
    @SerializedName("id") val id: String? = null,
    @SerializedName("name") val name: String = "",
    @SerializedName("description") val description: String? = null,
    @SerializedName("type") val type: TransactionType = TransactionType.DESPESA,
    @SerializedName("amount") val amount: Double = 0.0,
    @SerializedName("date") val date: Long = System.currentTimeMillis(),
    @SerializedName("paid") val paid: Boolean = false,
    @SerializedName("accountId") val accountId: String? = null,
    @SerializedName("targetAccountId") val targetAccountId: String? = null,
    @SerializedName("categoryId") val categoryId: String? = null,
    @SerializedName("categoryName") val categoryName: String? = null,
    @SerializedName("accountName") val accountName: String? = null,
    @SerializedName("isRecurring") val isRecurring: Boolean = false,
    @SerializedName("isFixed") val isFixed: Boolean = false,
    @SerializedName("recurrenceFrequency") val recurrenceFrequency: RecurrenceFrequency? = null,
    @SerializedName("recurrenceEndDate") val recurrenceEndDate: Long? = null,
    @SerializedName("recurrenceRuleId") val recurrenceRuleId: String? = null,
    @SerializedName("installments") val installments: Int = 1,
    @SerializedName("creditCardId") val creditCardId: String? = null,
    @SerializedName("targetInvoiceId") val targetInvoiceId: String? = null,
    @SerializedName("vehicleId") val vehicleId: String? = null,
    @SerializedName("vehicleName") val vehicleName: String? = null,
    @SerializedName("currentOdometer") val currentOdometer: Double? = null,
    @SerializedName("liters") val liters: Double? = null,
    @SerializedName("fuelType") val fuelType: FuelType? = null,
    @SerializedName("efficiency") val efficiency: Double? = null
) {
    private val currencyFormatter = NumberFormat.getCurrencyInstance(Locale("pt", "BR"))
    private val dateFormatter = SimpleDateFormat("dd/MM/yyyy", Locale("pt", "BR"))

    val formattedAmount: String get() = currencyFormatter.format(amount)
    val formattedDate: String get() = dateFormatter.format(Date(date))
    val dateObject: Date get() = Date(date)
    val itemOpacity: Float get() = if (paid) 0.4f else 1.0f
    val isPending: Boolean get() = !paid
    val statusText: String get() = if (paid) "PAGO" else "PENDENTE"
    val canDelete: Boolean get() = categoryName != "Fatura de Cartão"

    val statusColor: Long
        get() = if (paid) 0xFF00E676 else 0xFFFF9800

    val amountColor: Long
        get() = when (type) {
            TransactionType.RECEITA, TransactionType.TRANSFERENCIA_ENTRADA -> 0xFF00E676
            else -> 0xFFFF5252
        }

    val extratoIcon: String
        get() = when (type) {
            TransactionType.RECEITA -> "trending_down"
            TransactionType.DESPESA -> "trending_up"
            TransactionType.TRANSFERENCIA,
            TransactionType.TRANSFERENCIA_ENTRADA,
            TransactionType.TRANSFERENCIA_SAIDA -> "swap_horiz"
            TransactionType.PAGAMENTO_FATURA -> "receipt_long"
        }

    val extratoAmount: String
        get() = when (type) {
            TransactionType.RECEITA, TransactionType.TRANSFERENCIA_ENTRADA -> "+ $formattedAmount"
            else -> "- $formattedAmount"
        }

    val subtitleDisplay: String
        get() {
            val cat = categoryName ?: "Sem categoria"
            val acc = accountName ?: "Sem conta"
            return when (type) {
                TransactionType.TRANSFERENCIA_SAIDA -> "Saiu de: $acc"
                TransactionType.TRANSFERENCIA_ENTRADA -> "Entrou em: $acc"
                else -> "$cat • $acc"
            }
        }
}

data class TransactionGroup(
    val dateHeader: String,
    val transactions: List<Transaction>
)
