package com.cainanbt.controleja.ui.screens.creditcards

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.cainanbt.controleja.data.model.CreditCard
import com.cainanbt.controleja.data.remote.ApiService
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch
import javax.inject.Inject

data class CreditCardsUiState(
    val isLoading: Boolean = false,
    val cards: List<CreditCard> = emptyList()
)

@HiltViewModel
class CreditCardsViewModel @Inject constructor(
    private val apiService: ApiService
) : ViewModel() {

    private val _uiState = MutableStateFlow(CreditCardsUiState())
    val uiState: StateFlow<CreditCardsUiState> = _uiState.asStateFlow()

    init { loadCards() }

    fun refresh() = loadCards()

    private fun loadCards() {
        viewModelScope.launch {
            _uiState.value = _uiState.value.copy(isLoading = true)
            try {
                val response = apiService.getCreditCards()
                if (response.isSuccessful) {
                    _uiState.value = _uiState.value.copy(cards = response.body() ?: emptyList())
                }
            } catch (_: Exception) { }
            _uiState.value = _uiState.value.copy(isLoading = false)
        }
    }
}
