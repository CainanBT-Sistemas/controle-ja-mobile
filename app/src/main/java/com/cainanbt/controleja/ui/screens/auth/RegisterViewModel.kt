package com.cainanbt.controleja.ui.screens.auth

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.cainanbt.controleja.data.model.InsertUpdateUserDTO
import com.cainanbt.controleja.data.remote.ApiService
import com.cainanbt.controleja.util.ErrorHandler
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch
import javax.inject.Inject

data class RegisterUiState(
    val isLoading: Boolean = false,
    val username: String = "",
    val email: String = "",
    val password: String = "",
    val error: String? = null,
    val isRegistered: Boolean = false
)

@HiltViewModel
class RegisterViewModel @Inject constructor(
    private val apiService: ApiService
) : ViewModel() {

    private val _uiState = MutableStateFlow(RegisterUiState())
    val uiState: StateFlow<RegisterUiState> = _uiState.asStateFlow()

    fun onUsernameChange(v: String) { _uiState.value = _uiState.value.copy(username = v, error = null) }
    fun onEmailChange(v: String) { _uiState.value = _uiState.value.copy(email = v, error = null) }
    fun onPasswordChange(v: String) { _uiState.value = _uiState.value.copy(password = v, error = null) }

    fun register() {
        val state = _uiState.value
        if (state.username.isBlank() || state.email.isBlank() || state.password.isBlank()) {
            _uiState.value = state.copy(error = "Preencha todos os campos")
            return
        }

        viewModelScope.launch {
            _uiState.value = _uiState.value.copy(isLoading = true, error = null)
            try {
                val dto = InsertUpdateUserDTO(state.username, state.password, state.email)
                val response = apiService.register(dto)
                if (response.isSuccessful) {
                    _uiState.value = _uiState.value.copy(isLoading = false, isRegistered = true)
                } else {
                    _uiState.value = _uiState.value.copy(
                        isLoading = false,
                        error = "Não foi possível criar a conta. Tente novamente."
                    )
                }
            } catch (e: Exception) {
                val err = ErrorHandler.parse(e)
                _uiState.value = _uiState.value.copy(isLoading = false, error = err.message)
            }
        }
    }
}
