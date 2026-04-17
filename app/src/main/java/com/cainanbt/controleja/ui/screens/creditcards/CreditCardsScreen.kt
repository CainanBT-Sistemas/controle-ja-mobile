package com.cainanbt.controleja.ui.screens.creditcards

import androidx.compose.foundation.background
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.CreditCard
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
import com.cainanbt.controleja.ui.theme.*

@Composable
fun CreditCardsScreen(
    viewModel: CreditCardsViewModel = hiltViewModel(),
    onNavigateToInvoice: (String) -> Unit = {}
) {
    val uiState by viewModel.uiState.collectAsState()

    LazyColumn(
        modifier = Modifier.fillMaxSize().background(PrimaryDark),
        contentPadding = PaddingValues(16.dp),
        verticalArrangement = Arrangement.spacedBy(12.dp)
    ) {
        item { Text("Cartões de Crédito", style = MaterialTheme.typography.headlineMedium, color = TextPrimary, fontWeight = FontWeight.Bold) }

        if (uiState.cards.isEmpty() && !uiState.isLoading) {
            item { Box(Modifier.fillMaxWidth().padding(32.dp), contentAlignment = Alignment.Center) { Text("Nenhum cartão cadastrado", color = TextMuted) } }
        }

        items(uiState.cards) { card ->
            Card(
                modifier = Modifier.fillMaxWidth(),
                shape = RoundedCornerShape(16.dp),
                colors = CardDefaults.cardColors(containerColor = SurfaceDark),
                onClick = { onNavigateToInvoice(card.id) }
            ) {
                Column(Modifier.padding(16.dp)) {
                    Row(verticalAlignment = Alignment.CenterVertically) {
                        Box(Modifier.size(44.dp).clip(CircleShape).background(Color(android.graphics.Color.parseColor(card.color)).copy(alpha = 0.2f)),
                            contentAlignment = Alignment.Center) {
                            Icon(Icons.Filled.CreditCard, null, tint = Color(android.graphics.Color.parseColor(card.color)), modifier = Modifier.size(22.dp))
                        }
                        Spacer(Modifier.width(12.dp))
                        Column(Modifier.weight(1f)) {
                            Text(card.name, color = TextPrimary, fontWeight = FontWeight.SemiBold, fontSize = 16.sp)
                            Text(card.invoiceInfo, color = TextSecondary, fontSize = 12.sp)
                        }
                    }
                    Spacer(Modifier.height(16.dp))
                    LinearProgressIndicator(
                        progress = { card.limitProgress },
                        modifier = Modifier.fillMaxWidth().height(8.dp).clip(RoundedCornerShape(4.dp)),
                        color = Color(android.graphics.Color.parseColor(card.color)),
                        trackColor = DividerColor
                    )
                    Spacer(Modifier.height(8.dp))
                    Row(Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.SpaceBetween) {
                        Column {
                            Text("Usado", color = TextMuted, fontSize = 11.sp)
                            Text(card.formattedUsed, color = ExpenseRed, fontWeight = FontWeight.Bold, fontSize = 14.sp)
                        }
                        Column(horizontalAlignment = Alignment.CenterHorizontally) {
                            Text(card.usedPercent, color = TextSecondary, fontSize = 11.sp)
                        }
                        Column(horizontalAlignment = Alignment.End) {
                            Text("Disponível", color = TextMuted, fontSize = 11.sp)
                            Text(card.formattedAvailable, color = IncomeGreen, fontWeight = FontWeight.Bold, fontSize = 14.sp)
                        }
                    }
                    Spacer(Modifier.height(4.dp))
                    Text("Limite: ${card.formattedTotalLimit}", color = TextMuted, fontSize = 12.sp, modifier = Modifier.align(Alignment.CenterHorizontally))
                }
            }
        }

        if (uiState.isLoading) {
            item { Box(Modifier.fillMaxWidth().padding(16.dp), contentAlignment = Alignment.Center) { CircularProgressIndicator(color = PrimaryGreen) } }
        }
    }
}
