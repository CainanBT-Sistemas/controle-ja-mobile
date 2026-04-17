package com.cainanbt.controleja.ui.screens.transactions

import androidx.compose.foundation.background
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.*
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.hilt.navigation.compose.hiltViewModel
import com.cainanbt.controleja.data.model.Transaction
import com.cainanbt.controleja.data.model.TransactionType
import com.cainanbt.controleja.ui.theme.*

@Composable
fun TransactionsScreen(
    viewModel: TransactionsViewModel = hiltViewModel(),
    onNavigateToAdd: () -> Unit = {},
    onNavigateToEdit: (String) -> Unit = {}
) {
    val uiState by viewModel.uiState.collectAsState()

    LazyColumn(
        modifier = Modifier.fillMaxSize().background(PrimaryDark),
        contentPadding = PaddingValues(horizontal = 16.dp, vertical = 8.dp),
        verticalArrangement = Arrangement.spacedBy(4.dp)
    ) {
        // Month navigator
        item {
            Row(Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.SpaceBetween, verticalAlignment = Alignment.CenterVertically) {
                IconButton(onClick = viewModel::previousMonth) { Icon(Icons.Filled.ChevronLeft, null, tint = TextPrimary) }
                Text(uiState.currentMonthDisplay, color = TextPrimary, fontWeight = FontWeight.Medium)
                IconButton(onClick = viewModel::nextMonth) { Icon(Icons.Filled.ChevronRight, null, tint = TextPrimary) }
            }
        }

        // Summary
        item {
            Card(Modifier.fillMaxWidth(), shape = RoundedCornerShape(12.dp), colors = CardDefaults.cardColors(containerColor = SurfaceDark)) {
                Row(Modifier.padding(16.dp).fillMaxWidth(), horizontalArrangement = Arrangement.SpaceBetween) {
                    Column(horizontalAlignment = Alignment.CenterHorizontally) {
                        Text("Receitas", color = TextSecondary, fontSize = 12.sp)
                        Text(uiState.totalIncome, color = IncomeGreen, fontWeight = FontWeight.Bold, fontSize = 14.sp)
                    }
                    Column(horizontalAlignment = Alignment.CenterHorizontally) {
                        Text("Despesas", color = TextSecondary, fontSize = 12.sp)
                        Text(uiState.totalExpense, color = ExpenseRed, fontWeight = FontWeight.Bold, fontSize = 14.sp)
                    }
                    Column(horizontalAlignment = Alignment.CenterHorizontally) {
                        Text("Saldo", color = TextSecondary, fontSize = 12.sp)
                        Text(uiState.balance, color = TransferBlue, fontWeight = FontWeight.Bold, fontSize = 14.sp)
                    }
                }
            }
        }

        // Transaction groups
        if (uiState.groups.isEmpty() && !uiState.isLoading) {
            item {
                Box(Modifier.fillMaxWidth().padding(32.dp), contentAlignment = Alignment.Center) {
                    Text("Nenhuma transação neste mês", color = TextMuted)
                }
            }
        }

        uiState.groups.forEach { group ->
            item {
                Text(group.dateHeader, color = TextSecondary, fontSize = 13.sp, fontWeight = FontWeight.Medium,
                    modifier = Modifier.padding(top = 12.dp, bottom = 4.dp))
            }
            items(group.transactions) { tx ->
                TransactionItem(tx, onClick = { tx.id?.let { onNavigateToEdit(it) } })
            }
        }

        if (uiState.isLoading) {
            item { Box(Modifier.fillMaxWidth().padding(16.dp), contentAlignment = Alignment.Center) { CircularProgressIndicator(color = PrimaryGreen) } }
        }
    }
}

@Composable
private fun TransactionItem(tx: Transaction, onClick: () -> Unit) {
    val iconColor = Color(tx.amountColor)
    val icon = when (tx.type) {
        TransactionType.RECEITA -> Icons.Filled.TrendingDown
        TransactionType.DESPESA -> Icons.Filled.TrendingUp
        TransactionType.TRANSFERENCIA, TransactionType.TRANSFERENCIA_ENTRADA, TransactionType.TRANSFERENCIA_SAIDA -> Icons.Filled.SwapHoriz
        TransactionType.PAGAMENTO_FATURA -> Icons.Filled.Receipt
    }

    Card(
        modifier = Modifier.fillMaxWidth(),
        shape = RoundedCornerShape(12.dp),
        colors = CardDefaults.cardColors(containerColor = SurfaceDark.copy(alpha = if (tx.paid) 0.5f else 1f)),
        onClick = onClick
    ) {
        Row(Modifier.padding(12.dp).fillMaxWidth(), verticalAlignment = Alignment.CenterVertically) {
            Box(
                modifier = Modifier.size(36.dp).clip(CircleShape).background(iconColor.copy(alpha = 0.15f)),
                contentAlignment = Alignment.Center
            ) {
                Icon(icon, null, tint = iconColor, modifier = Modifier.size(18.dp))
            }
            Spacer(Modifier.width(12.dp))
            Column(Modifier.weight(1f)) {
                Text(tx.name, color = TextPrimary.copy(alpha = tx.itemOpacity), fontWeight = FontWeight.Medium, fontSize = 14.sp)
                Text(tx.subtitleDisplay, color = TextSecondary.copy(alpha = tx.itemOpacity), fontSize = 12.sp)
            }
            Column(horizontalAlignment = Alignment.End) {
                Text(tx.extratoAmount, color = iconColor.copy(alpha = tx.itemOpacity), fontWeight = FontWeight.Bold, fontSize = 14.sp)
                Text(tx.statusText, color = Color(tx.statusColor), fontSize = 10.sp, fontWeight = FontWeight.Medium)
            }
        }
    }
}
