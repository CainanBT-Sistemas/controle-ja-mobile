package com.cainanbt.controleja.ui.screens.dashboard

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
import com.cainanbt.controleja.data.model.Account
import com.cainanbt.controleja.data.model.CreditCard
import com.cainanbt.controleja.ui.theme.*

@Composable
fun DashboardScreen(
    viewModel: DashboardViewModel = hiltViewModel(),
    onNavigateToInvoice: (String) -> Unit = {}
) {
    val uiState by viewModel.uiState.collectAsState()

    LazyColumn(
        modifier = Modifier.fillMaxSize().background(PrimaryDark),
        contentPadding = PaddingValues(16.dp),
        verticalArrangement = Arrangement.spacedBy(12.dp)
    ) {
        // Header
        item {
            Text(
                text = "Olá, ${uiState.userName} 👋",
                style = MaterialTheme.typography.headlineMedium,
                color = TextPrimary,
                fontWeight = FontWeight.Bold
            )
        }

        // Month navigator
        item {
            Row(
                modifier = Modifier.fillMaxWidth(),
                horizontalArrangement = Arrangement.SpaceBetween,
                verticalAlignment = Alignment.CenterVertically
            ) {
                IconButton(onClick = viewModel::previousMonth) {
                    Icon(Icons.Filled.ChevronLeft, null, tint = TextPrimary)
                }
                Text(uiState.currentMonthDisplay, color = TextPrimary, fontWeight = FontWeight.Medium)
                IconButton(onClick = viewModel::nextMonth) {
                    Icon(Icons.Filled.ChevronRight, null, tint = TextPrimary)
                }
            }
        }

        // Balance Card
        item {
            Card(
                modifier = Modifier.fillMaxWidth(),
                shape = RoundedCornerShape(16.dp),
                colors = CardDefaults.cardColors(containerColor = SurfaceDark)
            ) {
                Column(modifier = Modifier.padding(20.dp)) {
                    Text("Saldo Disponível", color = TextSecondary, fontSize = 13.sp)
                    Text(uiState.availableBalance, color = PrimaryGreen, fontSize = 28.sp, fontWeight = FontWeight.Bold)
                    Spacer(Modifier.height(16.dp))
                    Row(Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.SpaceBetween) {
                        BalanceItem("Projetado", uiState.projectedBalance, PrimaryGreen)
                        BalanceItem("A Pagar", uiState.projectedPayables, ExpenseRed)
                        BalanceItem("Variáveis", uiState.projectedVariables, PendingOrange)
                    }
                }
            }
        }

        // Accounts section
        if (uiState.accounts.isNotEmpty()) {
            item { SectionHeader("Minhas Contas") }
            items(uiState.accounts) { account -> AccountCard(account) }
        }

        // Credit Cards section
        if (uiState.creditCards.isNotEmpty()) {
            item { SectionHeader("Cartões de Crédito") }
            items(uiState.creditCards) { card -> CreditCardItem(card, onNavigateToInvoice) }
        }

        // Loading
        if (uiState.isLoading) {
            item {
                Box(Modifier.fillMaxWidth(), contentAlignment = Alignment.Center) {
                    CircularProgressIndicator(color = PrimaryGreen)
                }
            }
        }
    }
}

@Composable
private fun BalanceItem(label: String, value: String, color: Color) {
    Column(horizontalAlignment = Alignment.CenterHorizontally) {
        Text(label, color = TextMuted, fontSize = 11.sp)
        Text(value, color = color, fontSize = 14.sp, fontWeight = FontWeight.SemiBold)
    }
}

@Composable
private fun SectionHeader(title: String) {
    Text(title, color = TextSecondary, fontSize = 14.sp, fontWeight = FontWeight.SemiBold,
        modifier = Modifier.padding(top = 8.dp))
}

@Composable
private fun AccountCard(account: Account) {
    Card(
        modifier = Modifier.fillMaxWidth(),
        shape = RoundedCornerShape(12.dp),
        colors = CardDefaults.cardColors(containerColor = SurfaceDark)
    ) {
        Row(
            modifier = Modifier.padding(16.dp).fillMaxWidth(),
            verticalAlignment = Alignment.CenterVertically
        ) {
            Box(
                modifier = Modifier.size(40.dp).clip(CircleShape)
                    .background(Color(android.graphics.Color.parseColor(account.color)).copy(alpha = 0.2f)),
                contentAlignment = Alignment.Center
            ) {
                Icon(Icons.Filled.AccountBalance, null, tint = Color(android.graphics.Color.parseColor(account.color)), modifier = Modifier.size(20.dp))
            }
            Spacer(Modifier.width(12.dp))
            Column(modifier = Modifier.weight(1f)) {
                Text(account.name, color = TextPrimary, fontWeight = FontWeight.SemiBold, fontSize = 14.sp)
                Text(account.typeDisplay, color = TextSecondary, fontSize = 12.sp)
            }
            Text(account.formattedBalance, color = TextPrimary, fontWeight = FontWeight.Bold, fontSize = 14.sp)
        }
    }
}

@Composable
private fun CreditCardItem(card: CreditCard, onNavigateToInvoice: (String) -> Unit) {
    Card(
        modifier = Modifier.fillMaxWidth(),
        shape = RoundedCornerShape(12.dp),
        colors = CardDefaults.cardColors(containerColor = SurfaceDark),
        onClick = { onNavigateToInvoice(card.id) }
    ) {
        Column(modifier = Modifier.padding(16.dp)) {
            Row(verticalAlignment = Alignment.CenterVertically) {
                Box(
                    modifier = Modifier.size(40.dp).clip(CircleShape)
                        .background(Color(android.graphics.Color.parseColor(card.color)).copy(alpha = 0.2f)),
                    contentAlignment = Alignment.Center
                ) {
                    Icon(Icons.Filled.CreditCard, null, tint = Color(android.graphics.Color.parseColor(card.color)), modifier = Modifier.size(20.dp))
                }
                Spacer(Modifier.width(12.dp))
                Column(modifier = Modifier.weight(1f)) {
                    Text(card.name, color = TextPrimary, fontWeight = FontWeight.SemiBold, fontSize = 14.sp)
                    Text(card.invoiceInfo, color = TextSecondary, fontSize = 12.sp)
                }
            }
            Spacer(Modifier.height(12.dp))
            LinearProgressIndicator(
                progress = { card.limitProgress },
                modifier = Modifier.fillMaxWidth().height(6.dp).clip(RoundedCornerShape(3.dp)),
                color = Color(android.graphics.Color.parseColor(card.color)),
                trackColor = DividerColor
            )
            Spacer(Modifier.height(8.dp))
            Row(Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.SpaceBetween) {
                Text("Usado: ${card.formattedUsed}", color = TextSecondary, fontSize = 12.sp)
                Text("Disponível: ${card.formattedAvailable}", color = PrimaryGreen, fontSize = 12.sp)
            }
        }
    }
}
