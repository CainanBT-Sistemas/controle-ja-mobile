package com.cainanbt.controleja.data.model

import com.google.gson.annotations.SerializedName
import java.text.NumberFormat
import java.text.SimpleDateFormat
import java.util.Date
import java.util.Locale

data class ChartData(
    @SerializedName("label") val label: String = "",
    @SerializedName("value") val value: Double = 0.0,
    @SerializedName("color") val color: String? = null
)

data class FinancialSummary(
    @SerializedName("totalIncome") val totalIncome: Double = 0.0,
    @SerializedName("totalExpense") val totalExpense: Double = 0.0,
    @SerializedName("balance") val balance: Double = 0.0
)

data class DashboardAlert(
    @SerializedName("id") val id: String = "",
    @SerializedName("description") val description: String = "",
    @SerializedName("amount") val amount: Double = 0.0,
    @SerializedName("dueDate") val dueDate: Long = 0,
    @SerializedName("icon") val icon: String? = null,
    @SerializedName("color") val color: String? = null,
    @SerializedName("type") val type: String? = null
) {
    private val formatter = NumberFormat.getCurrencyInstance(Locale("pt", "BR"))
    private val dateFormatter = SimpleDateFormat("dd/MM/yyyy", Locale("pt", "BR"))

    val formattedAmount: String get() = formatter.format(amount)
    val formattedDueDate: String get() = dateFormatter.format(Date(dueDate))
}

data class DashboardFullSummary(
    @SerializedName("availableBalance") val availableBalance: Double = 0.0,
    @SerializedName("projectedBalance") val projectedBalance: Double = 0.0,
    @SerializedName("projectedPayables") val projectedPayables: Double = 0.0,
    @SerializedName("projectedVariables") val projectedVariables: Double = 0.0,
    @SerializedName("pendingPayables") val pendingPayables: List<DashboardAlert> = emptyList(),
    @SerializedName("pendingReceivables") val pendingReceivables: List<DashboardAlert> = emptyList(),
    @SerializedName("pendingInvoices") val pendingInvoices: List<DashboardAlert> = emptyList(),
    @SerializedName("accounts") val accounts: List<Account> = emptyList(),
    @SerializedName("creditCards") val creditCards: List<CreditCard> = emptyList(),
    @SerializedName("overduePayables") val overduePayables: List<DashboardAlert> = emptyList(),
    @SerializedName("overdueInvoices") val overdueInvoices: List<DashboardAlert> = emptyList()
)
