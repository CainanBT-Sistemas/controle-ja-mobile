package com.cainanbt.controleja.data.model

import com.google.gson.annotations.SerializedName
import java.text.NumberFormat
import java.util.Locale
import java.util.UUID

data class Vehicle(
    @SerializedName("id") val id: String = UUID.randomUUID().toString(),
    @SerializedName("name") val name: String = "",
    @SerializedName("brand") val brand: String = "",
    @SerializedName("model") val model: String = "",
    @SerializedName("year") val year: Int = 0,
    @SerializedName("plate") val plate: String = "",
    @SerializedName("currentOdometer") val currentOdometer: Double = 0.0,
    @SerializedName("avgGasoline") val avgGasoline: Double? = null,
    @SerializedName("avgEthanol") val avgEthanol: Double? = null
) {
    val fullDescription: String get() = "$brand $model $year"
    val formattedOdometer: String
        get() = NumberFormat.getNumberInstance(Locale("pt", "BR")).format(currentOdometer) + " km"
    val consumptionInfo: String
        get() {
            val parts = mutableListOf<String>()
            avgGasoline?.let { if (it > 0) parts.add("Gasolina: ${"%.1f".format(it)} km/L") }
            avgEthanol?.let { if (it > 0) parts.add("Etanol: ${"%.1f".format(it)} km/L") }
            return parts.joinToString(" • ").ifEmpty { "Sem dados" }
        }
}
