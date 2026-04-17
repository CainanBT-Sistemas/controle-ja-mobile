package com.cainanbt.controleja.ui.screens.vehicles

import androidx.compose.foundation.background
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.DirectionsCar
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.hilt.navigation.compose.hiltViewModel
import com.cainanbt.controleja.data.model.Vehicle
import com.cainanbt.controleja.ui.theme.*

@Composable
fun VehiclesScreen(
    viewModel: VehiclesViewModel = hiltViewModel()
) {
    val uiState by viewModel.uiState.collectAsState()

    LazyColumn(
        modifier = Modifier.fillMaxSize().background(PrimaryDark),
        contentPadding = PaddingValues(16.dp),
        verticalArrangement = Arrangement.spacedBy(12.dp)
    ) {
        item { Text("Meus Veículos", style = MaterialTheme.typography.headlineMedium, color = TextPrimary, fontWeight = FontWeight.Bold) }

        if (uiState.vehicles.isEmpty() && !uiState.isLoading) {
            item { Box(Modifier.fillMaxWidth().padding(32.dp), contentAlignment = Alignment.Center) { Text("Nenhum veículo cadastrado", color = TextMuted) } }
        }

        items(uiState.vehicles) { vehicle -> VehicleCard(vehicle) }

        if (uiState.isLoading) {
            item { Box(Modifier.fillMaxWidth().padding(16.dp), contentAlignment = Alignment.Center) { CircularProgressIndicator(color = PrimaryGreen) } }
        }
    }
}

@Composable
private fun VehicleCard(vehicle: Vehicle) {
    Card(
        modifier = Modifier.fillMaxWidth(),
        shape = RoundedCornerShape(16.dp),
        colors = CardDefaults.cardColors(containerColor = SurfaceDark)
    ) {
        Row(Modifier.padding(16.dp).fillMaxWidth(), verticalAlignment = Alignment.CenterVertically) {
            Box(Modifier.size(48.dp).clip(CircleShape).background(TransferBlue.copy(alpha = 0.15f)),
                contentAlignment = Alignment.Center) {
                Icon(Icons.Filled.DirectionsCar, null, tint = TransferBlue, modifier = Modifier.size(24.dp))
            }
            Spacer(Modifier.width(16.dp))
            Column(Modifier.weight(1f)) {
                Text(vehicle.name, color = TextPrimary, fontWeight = FontWeight.SemiBold, fontSize = 16.sp)
                Text(vehicle.fullDescription, color = TextSecondary, fontSize = 13.sp)
                Text(vehicle.formattedOdometer, color = TextMuted, fontSize = 12.sp)
                Text(vehicle.consumptionInfo, color = PrimaryGreen, fontSize = 12.sp)
                if (vehicle.plate.isNotEmpty()) {
                    Text("Placa: ${vehicle.plate}", color = TextMuted, fontSize = 12.sp)
                }
            }
        }
    }
}
