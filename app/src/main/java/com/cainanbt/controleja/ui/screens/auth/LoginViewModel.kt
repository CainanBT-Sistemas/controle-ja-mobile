package com.cainanbt.controleja.ui.screens.auth

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.cainanbt.controleja.data.local.TokenManager
import com.cainanbt.controleja.data.model.LoginRequest
import com.cainanbt.controleja.data.remote.ApiService
import com.cainanbt.controleja.util.ErrorHandler
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch
import javax.inject.Inject

data class LoginUiState(
    val isLoading: Boolean = false,
    val email: String = "",
    val password: String = "",
    val error: String? = null,
    val isLoggedIn: Boolean = false
)

@HiltViewModel
class LoginViewModel @Inject constructor(
    private val apiService: ApiService,
    private val tokenManager: TokenManager
) : ViewModel() {

    private val _uiState = MutableStateFlow(LoginUiState())
    val uiState: StateFlow<LoginUiState> = _uiState.asStateFlow()

    init {
        tryAutoLogin()
    }

    fun onEmailChange(email: String) {
        _uiState.value = _uiState.value.copy(email = email, error = null)
    }

    fun onPasswordChange(password: String) {
        _uiState.value = _uiState.value.copy(password = password, error = null)
    }

    fun login() {
        val state = _uiState.value
        if (state.email.isBlank() || state.password.isBlank()) {
            _uiState.value = state.copy(error = "Preencha todos os campos")
            return
        }

        viewModelScope.launch {
            _uiState.value = _uiState.value.copy(isLoading = true, error = null)
            try {
                val response = apiService.login(LoginRequest(state.email, state.password))
                if (response.isSuccessful) {
                    val user = response.body()
                    if (user != null) {
                        user.tokens?.let { tokens ->
                            tokenManager.saveTokens(tokens.accessToken, tokens.refreshToken)
                        }
                        tokenManager.saveUserInfo(user.id, user.username, user.email)
                        _uiState.value = _uiState.value.copy(isLoading = false, isLoggedIn = true)
                    }
                } else {
                    _uiState.value = _uiState.value.copy(
                        isLoading = false,
                        error = "Email ou senha incorretos"
                    )
                }
            } catch (e: Exception) {
                val err = ErrorHandler.parse(e)
                _uiState.value = _uiState.value.copy(isLoading = false, error = err.message)
            }
        }
    }

    private fun tryAutoLogin() {
        val refreshToken = tokenManager.getRefreshToken() ?: return
        viewModelScope.launch {
            _uiState.value = _uiState.value.copy(isLoading = true)
            try {
                val response = apiService.autoLogin(mapOf("refreshToken" to refreshToken))
                if (response.isSuccessful) {
                    val user = response.body()
                    if (user != null) {
                        user.tokens?.let { tokens ->
                            tokenManager.saveTokens(tokens.accessToken, tokens.refreshToken)
                        }
                        tokenManager.saveUserInfo(user.id, user.username, user.email)
                        _uiState.value = _uiState.value.copy(isLoading = false, isLoggedIn = true)
                    }
                } else {
                    tokenManager.clearAll()
                    _uiState.value = _uiState.value.copy(isLoading = false)
                }
            } catch (e: Exception) {
                _uiState.value = _uiState.value.copy(isLoading = false)
            }
        }
    }
}
