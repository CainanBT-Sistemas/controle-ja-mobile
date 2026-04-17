package com.cainanbt.controleja.data.local

import android.content.Context
import android.content.SharedPreferences
import androidx.security.crypto.EncryptedSharedPreferences
import androidx.security.crypto.MasterKey
import dagger.hilt.android.qualifiers.ApplicationContext
import javax.inject.Inject
import javax.inject.Singleton

@Singleton
class TokenManager @Inject constructor(
    @ApplicationContext context: Context
) {
    private val masterKey = MasterKey.Builder(context)
        .setKeyScheme(MasterKey.KeyScheme.AES256_GCM)
        .build()

    private val securePrefs: SharedPreferences = EncryptedSharedPreferences.create(
        context,
        "controle_ja_secure_prefs",
        masterKey,
        EncryptedSharedPreferences.PrefKeyEncryptionScheme.AES256_SIV,
        EncryptedSharedPreferences.PrefValueEncryptionScheme.AES256_GCM
    )

    private val prefs: SharedPreferences =
        context.getSharedPreferences("controle_ja_prefs", Context.MODE_PRIVATE)

    fun saveTokens(accessToken: String, refreshToken: String) {
        securePrefs.edit()
            .putString(KEY_ACCESS_TOKEN, accessToken)
            .putString(KEY_REFRESH_TOKEN, refreshToken)
            .apply()
    }

    fun getAccessToken(): String? = securePrefs.getString(KEY_ACCESS_TOKEN, null)
    fun getRefreshToken(): String? = securePrefs.getString(KEY_REFRESH_TOKEN, null)

    fun saveUserInfo(userId: String, username: String, email: String) {
        prefs.edit()
            .putString(KEY_USER_ID, userId)
            .putString(KEY_USERNAME, username)
            .putString(KEY_EMAIL, email)
            .apply()
    }

    fun getUserId(): String? = prefs.getString(KEY_USER_ID, null)
    fun getUsername(): String = prefs.getString(KEY_USERNAME, "") ?: ""
    fun getEmail(): String = prefs.getString(KEY_EMAIL, "") ?: ""

    fun clearAll() {
        securePrefs.edit().clear().apply()
        prefs.edit().clear().apply()
    }

    fun isLoggedIn(): Boolean = getAccessToken() != null

    companion object {
        private const val KEY_ACCESS_TOKEN = "access_token"
        private const val KEY_REFRESH_TOKEN = "refresh_token"
        private const val KEY_USER_ID = "user_id"
        private const val KEY_USERNAME = "username"
        private const val KEY_EMAIL = "email"
    }
}
