package com.cainanbt.controleja.ui.screens.transactions

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
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

data class TransactionsUiState(
    val isLoading: Boolean = false,
    val currentMonthDisplay: String = "",
    val groups: List<TransactionGroup> = emptyList(),
    val totalIncome: String = "R$ 0,00",
    val totalExpense: String = "R$ 0,00",
    val balance: String = "R$ 0,00"
)

@HiltViewModel
class TransactionsViewModel @Inject constructor(
    private val apiService: ApiService
) : ViewModel() {

    private val _uiState = MutableStateFlow(TransactionsUiState())
    val uiState: StateFlow<TransactionsUiState> = _uiState.asStateFlow()

    private val formatter = NumberFormat.getCurrencyInstance(Locale("pt", "BR"))
    private val calendar = Calendar.getInstance()

    init {
        updateMonthDisplay()
        loadTransactions()
    }

    fun previousMonth() {
        calendar.add(Calendar.MONTH, -1)
        updateMonthDisplay()
        loadTransactions()
    }

    fun nextMonth() {
        calendar.add(Calendar.MONTH, 1)
        updateMonthDisplay()
        loadTransactions()
    }

    fun refresh() = loadTransactions()

    fun deleteTransaction(id: String, cancelFuture: Boolean = false) {
        viewModelScope.launch {
            try {
                apiService.deleteTransaction(id, cancelFuture)
                loadTransactions()
            } catch (_: Exception) { }
        }
    }

    private fun updateMonthDisplay() {
        val sdf = SimpleDateFormat("MMMM 'de' yyyy", Locale("pt", "BR"))
        _uiState.value = _uiState.value.copy(
            currentMonthDisplay = sdf.format(calendar.time).replaceFirstChar { it.uppercase() }
        )
    }

    private fun loadTransactions() {
        viewModelScope.launch {
            _uiState.value = _uiState.value.copy(isLoading = true)
            try {
                val cal = calendar.clone() as Calendar
                cal.set(Calendar.DAY_OF_MONTH, 1)
                cal.set(Calendar.HOUR_OF_DAY, 0)
                cal.set(Calendar.MINUTE, 0)
                cal.set(Calendar.SECOND, 0)
                val start = cal.timeInMillis
                cal.add(Calendar.MONTH, 1)
                cal.add(Calendar.MILLISECOND, -1)
                val end = cal.timeInMillis

                val response = apiService.getTransactions(start, end)
                if (response.isSuccessful) {
                    val transactions = response.body() ?: emptyList()
                    val dateFormat = SimpleDateFormat("EEE, dd/MM/yyyy", Locale("pt", "BR"))

                    val groups = transactions
                        .sortedByDescending { it.date }
                        .groupBy { dateFormat.format(Date(it.date)) }
                        .map { (header, txs) -> TransactionGroup(header, txs) }

                    val income = transactions
                        .filter { it.type == TransactionType.RECEITA || it.type == TransactionType.TRANSFERENCIA_ENTRADA }
                        .sumOf { it.amount }
                    val expense = transactions
                        .filter { it.type == TransactionType.DESPESA || it.type == TransactionType.TRANSFERENCIA_SAIDA || it.type == TransactionType.PAGAMENTO_FATURA }
                        .sumOf { it.amount }

                    _uiState.value = _uiState.value.copy(
                        groups = groups,
                        totalIncome = formatter.format(income),
                        totalExpense = formatter.format(expense),
                        balance = formatter.format(income - expense)
                    )
                }
            } catch (_: Exception) { }
            _uiState.value = _uiState.value.copy(isLoading = false)
        }
    }
}
