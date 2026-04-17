package com.cainanbt.controleja.ui.screens.vehicles

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.cainanbt.controleja.data.model.Vehicle
import com.cainanbt.controleja.data.remote.ApiService
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch
import javax.inject.Inject

data class VehiclesUiState(
    val isLoading: Boolean = false,
    val vehicles: List<Vehicle> = emptyList()
)

@HiltViewModel
class VehiclesViewModel @Inject constructor(
    private val apiService: ApiService
) : ViewModel() {

    private val _uiState = MutableStateFlow(VehiclesUiState())
    val uiState: StateFlow<VehiclesUiState> = _uiState.asStateFlow()

    init { loadVehicles() }

    fun refresh() = loadVehicles()

    fun deleteVehicle(id: String) {
        viewModelScope.launch {
            try {
                apiService.deleteVehicle(id)
                loadVehicles()
            } catch (_: Exception) { }
        }
    }

    private fun loadVehicles() {
        viewModelScope.launch {
            _uiState.value = _uiState.value.copy(isLoading = true)
            try {
                val response = apiService.getVehicles()
                if (response.isSuccessful) {
                    _uiState.value = _uiState.value.copy(vehicles = response.body() ?: emptyList())
                }
            } catch (_: Exception) { }
            _uiState.value = _uiState.value.copy(isLoading = false)
        }
    }
}
