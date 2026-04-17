package com.cainanbt.controleja.data.model

import com.google.gson.annotations.SerializedName
import java.text.NumberFormat
import java.text.SimpleDateFormat
import java.util.Date
import java.util.Locale

data class InvoiceItemDTO(
    @SerializedName("id") val id: String = "",
    @SerializedName("date") val date: Long = 0,
    @SerializedName("name") val name: String = "",
    @SerializedName("currentInstallment") val currentInstallment: Int = 0,
    @SerializedName("totalInstallments") val totalInstallments: Int = 0,
    @SerializedName("amount") val amount: Double = 0.0
) {
    private val formatter = NumberFormat.getCurrencyInstance(Locale("pt", "BR"))
    private val dateFormatter = SimpleDateFormat("dd/MM", Locale("pt", "BR"))

    val formattedAmount: String get() = formatter.format(amount)
    val formattedDate: String get() = dateFormatter.format(Date(date))
    val installmentInfo: String
        get() = if (totalInstallments > 1) "$currentInstallment/$totalInstallments" else ""
}

data class InvoiceDetailsDTO(
    @SerializedName("invoiceId") val invoiceId: String = "",
    @SerializedName("cardId") val cardId: String = "",
    @SerializedName("cardName") val cardName: String = "",
    @SerializedName("month") val month: Int = 0,
    @SerializedName("year") val year: Int = 0,
    @SerializedName("totalAmount") val totalAmount: Double = 0.0,
    @SerializedName("closeDate") val closeDate: Long = 0,
    @SerializedName("dueDate") val dueDate: Long = 0,
    @SerializedName("status") val status: String = "",
    @SerializedName("items") val items: List<InvoiceItemDTO> = emptyList()
) {
    private val formatter = NumberFormat.getCurrencyInstance(Locale("pt", "BR"))
    val formattedTotal: String get() = formatter.format(totalAmount)
}

data class RefundRequestDTO(
    @SerializedName("installmentId") val installmentId: String,
    @SerializedName("refundAmount") val refundAmount: Double
)

data class AdvanceRequestDTO(
    @SerializedName("purchaseId") val purchaseId: String,
    @SerializedName("quantityToAdvance") val quantityToAdvance: Int,
    @SerializedName("discountAmount") val discountAmount: Double
)

data class AdvanceablePurchaseDTO(
    @SerializedName("purchaseId") val purchaseId: String = "",
    @SerializedName("name") val name: String = "",
    @SerializedName("maxAdvanceable") val maxAdvanceable: Int = 0
)

data class SelectionItem(
    val id: String = "",
    val name: String = "",
    val icon: String = "",
    val color: String = "",
    val type: String = "",
    val subItems: List<SelectionItem> = emptyList(),
    var isExpanded: Boolean = false,
    var isSelected: Boolean = false
)
