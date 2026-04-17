package com.cainanbt.controleja.ui.screens.settings

import androidx.lifecycle.ViewModel
import com.cainanbt.controleja.data.local.TokenManager
import com.cainanbt.controleja.data.remote.ApiService
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import javax.inject.Inject

data class ProfileUiState(
    val username: String = "",
    val email: String = ""
)

@HiltViewModel
class ProfileViewModel @Inject constructor(
    private val apiService: ApiService,
    private val tokenManager: TokenManager
) : ViewModel() {

    private val _uiState = MutableStateFlow(ProfileUiState())
    val uiState: StateFlow<ProfileUiState> = _uiState.asStateFlow()

    init {
        _uiState.value = ProfileUiState(
            username = tokenManager.getUsername(),
            email = tokenManager.getEmail()
        )
    }

    fun logout() {
        tokenManager.clearAll()
    }
}
