package com.cainanbt.controleja.data.model

import com.google.gson.annotations.SerializedName
import java.text.NumberFormat
import java.util.Locale
import java.util.UUID

data class CreditCard(
    @SerializedName("id") val id: String = UUID.randomUUID().toString(),
    @SerializedName("accountId") val accountId: String = "",
    @SerializedName("name") val name: String = "",
    @SerializedName("totalLimit") val totalLimit: Double = 0.0,
    @SerializedName("currentLimit") val currentLimit: Double = 0.0,
    @SerializedName("closeDay") val closeDay: Int = 1,
    @SerializedName("bestDay") val bestDay: Int = 10,
    @SerializedName("icon") val icon: String = "credit_card",
    @SerializedName("color") val color: String = "#7C4DFF"
) {
    private val formatter = NumberFormat.getCurrencyInstance(Locale("pt", "BR"))

    val usedAmount: Double get() = totalLimit - currentLimit
    val formattedTotalLimit: String get() = formatter.format(totalLimit)
    val formattedAvailable: String get() = formatter.format(currentLimit)
    val formattedUsed: String get() = formatter.format(usedAmount)
    val invoiceInfo: String get() = "Fecha dia $closeDay • Vence dia $bestDay"
    val limitProgress: Float get() = if (totalLimit > 0) (usedAmount / totalLimit).toFloat() else 0f
    val usedPercent: String get() = "${(limitProgress * 100).toInt()}% usado"
}
