package com.cainanbt.controleja.ui.screens.dashboard

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.cainanbt.controleja.data.local.TokenManager
import com.cainanbt.controleja.data.model.*
import com.cainanbt.controleja.data.remote.ApiService
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch
import java.text.NumberFormat
import java.text.SimpleDateFormat
import java.util.*
import javax.inject.Inject

data class DashboardUiState(
    val isLoading: Boolean = false,
    val userName: String = "",
    val currentMonthDisplay: String = "",
    val availableBalance: String = "R$ 0,00",
    val projectedBalance: String = "R$ 0,00",
    val projectedPayables: String = "R$ 0,00",
    val projectedVariables: String = "R$ 0,00",
    val accounts: List<Account> = emptyList(),
    val creditCards: List<CreditCard> = emptyList(),
    val overduePayables: List<DashboardAlert> = emptyList(),
    val overdueInvoices: List<DashboardAlert> = emptyList(),
    val pendingPayables: List<DashboardAlert> = emptyList(),
    val pendingReceivables: List<DashboardAlert> = emptyList(),
    val pendingInvoices: List<DashboardAlert> = emptyList(),
    val expenseChartData: List<ChartData> = emptyList(),
    val incomeChartData: List<ChartData> = emptyList(),
    val creditChartData: List<ChartData> = emptyList()
)

@HiltViewModel
class DashboardViewModel @Inject constructor(
    private val apiService: ApiService,
    private val tokenManager: TokenManager
) : ViewModel() {

    private val _uiState = MutableStateFlow(DashboardUiState())
    val uiState: StateFlow<DashboardUiState> = _uiState.asStateFlow()

    private val formatter = NumberFormat.getCurrencyInstance(Locale("pt", "BR"))
    private val calendar = Calendar.getInstance()

    init {
        _uiState.value = _uiState.value.copy(userName = tokenManager.getUsername())
        updateMonthDisplay()
        loadDashboard()
    }

    fun previousMonth() {
        calendar.add(Calendar.MONTH, -1)
        updateMonthDisplay()
        loadDashboard()
    }

    fun nextMonth() {
        calendar.add(Calendar.MONTH, 1)
        updateMonthDisplay()
        loadDashboard()
    }

    fun refresh() = loadDashboard()

    fun logout() {
        tokenManager.clearAll()
    }

    private fun updateMonthDisplay() {
        val sdf = SimpleDateFormat("MMMM 'de' yyyy", Locale("pt", "BR"))
        val display = sdf.format(calendar.time).replaceFirstChar { it.uppercase() }
        _uiState.value = _uiState.value.copy(currentMonthDisplay = display)
    }

    private fun getMonthRange(): Pair<Long, Long> {
        val cal = calendar.clone() as Calendar
        cal.set(Calendar.DAY_OF_MONTH, 1)
        cal.set(Calendar.HOUR_OF_DAY, 0)
        cal.set(Calendar.MINUTE, 0)
        cal.set(Calendar.SECOND, 0)
        cal.set(Calendar.MILLISECOND, 0)
        val start = cal.timeInMillis
        cal.add(Calendar.MONTH, 1)
        cal.add(Calendar.MILLISECOND, -1)
        val end = cal.timeInMillis
        return start to end
    }

    private fun loadDashboard() {
        viewModelScope.launch {
            _uiState.value = _uiState.value.copy(isLoading = true)
            try {
                val (start, end) = getMonthRange()
                val response = apiService.getFullSummary(start, end)
                if (response.isSuccessful) {
                    val data = response.body()
                    if (data != null) {
                        _uiState.value = _uiState.value.copy(
                            availableBalance = formatter.format(data.availableBalance),
                            projectedBalance = formatter.format(data.projectedBalance),
                            projectedPayables = formatter.format(data.projectedPayables),
                            projectedVariables = formatter.format(data.projectedVariables),
                            accounts = data.accounts,
                            creditCards = data.creditCards,
                            overduePayables = data.overduePayables,
                            overdueInvoices = data.overdueInvoices,
                            pendingPayables = data.pendingPayables,
                            pendingReceivables = data.pendingReceivables,
                            pendingInvoices = data.pendingInvoices
                        )
                    }
                }

                // Load chart data
                val expResp = apiService.getExpensesByCategory(start, end)
                val incResp = apiService.getIncomesByCategory(start, end)
                val credResp = apiService.getCreditExpensesByCategory(start, end)

                _uiState.value = _uiState.value.copy(
                    expenseChartData = expResp.body() ?: emptyList(),
                    incomeChartData = incResp.body() ?: emptyList(),
                    creditChartData = credResp.body() ?: emptyList()
                )
            } catch (_: Exception) { }
            _uiState.value = _uiState.value.copy(isLoading = false)
        }
    }
}
