package com.cainanbt.controleja.data.model

import com.google.gson.annotations.SerializedName

data class LoginRequest(
    @SerializedName("email") val email: String,
    @SerializedName("password") val password: String
)

data class UserResponse(
    @SerializedName("id") val id: String = "",
    @SerializedName("username") val username: String = "",
    @SerializedName("email") val email: String = "",
    @SerializedName("createdAt") val createdAt: Long = 0,
    @SerializedName("tokens") val tokens: AuthTokens? = null
)

data class AuthTokens(
    @SerializedName("accessToken") val accessToken: String = "",
    @SerializedName("refreshToken") val refreshToken: String = ""
)

data class InsertUpdateUserDTO(
    @SerializedName("username") val username: String,
    @SerializedName("password") val password: String,
    @SerializedName("email") val email: String
)
