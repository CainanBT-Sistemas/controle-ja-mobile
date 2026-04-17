package com.cainanbt.controleja.data.model

import com.google.gson.annotations.SerializedName
import java.util.UUID

data class Category(
    @SerializedName("id") val id: String = UUID.randomUUID().toString(),
    @SerializedName("name") val name: String = "",
    @SerializedName("categoryType") val type: TransactionType = TransactionType.DESPESA,
    @SerializedName("icon") val icon: String = "category",
    @SerializedName("color") val color: String = "#42A5F5",
    @SerializedName("isDefault") val isDefault: Boolean = false,
    @SerializedName("parentId") val parentId: String? = null,
    @SerializedName("subCategories") val subCategories: List<Category> = emptyList()
) {
    val hasChildren: Boolean get() = subCategories.isNotEmpty()
    val childrenCount: Int get() = subCategories.size
}
