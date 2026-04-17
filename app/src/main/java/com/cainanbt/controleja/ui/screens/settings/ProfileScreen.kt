package com.cainanbt.controleja.ui.screens.settings

import androidx.compose.foundation.background
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.*
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.hilt.navigation.compose.hiltViewModel
import com.cainanbt.controleja.ui.theme.*

@Composable
fun ProfileScreen(
    viewModel: ProfileViewModel = hiltViewModel(),
    onLogout: () -> Unit
) {
    val uiState by viewModel.uiState.collectAsState()

    Column(
        modifier = Modifier.fillMaxSize().background(PrimaryDark).padding(24.dp),
        horizontalAlignment = Alignment.CenterHorizontally
    ) {
        Spacer(Modifier.height(24.dp))

        // Avatar
        Box(
            modifier = Modifier.size(80.dp).clip(CircleShape).background(PrimaryGreen.copy(alpha = 0.2f)),
            contentAlignment = Alignment.Center
        ) {
            Text(
                text = uiState.username.firstOrNull()?.uppercase() ?: "?",
                fontSize = 32.sp, fontWeight = FontWeight.Bold, color = PrimaryGreen
            )
        }

        Spacer(Modifier.height(16.dp))
        Text(uiState.username, style = MaterialTheme.typography.headlineMedium, color = TextPrimary, fontWeight = FontWeight.Bold)
        Text(uiState.email, color = TextSecondary, style = MaterialTheme.typography.bodyMedium)

        Spacer(Modifier.height(32.dp))

        // Menu items
        ProfileMenuItem(Icons.Filled.Person, "Editar Perfil") { }
        ProfileMenuItem(Icons.Filled.Lock, "Alterar Senha") { }
        ProfileMenuItem(Icons.Filled.Settings, "Configurações") { }
        ProfileMenuItem(Icons.Filled.AccountBalance, "Gerenciar Contas") { }
        ProfileMenuItem(Icons.Filled.Category, "Gerenciar Categorias") { }

        Spacer(Modifier.weight(1f))

        Button(
            onClick = { viewModel.logout(); onLogout() },
            modifier = Modifier.fillMaxWidth().height(52.dp),
            shape = RoundedCornerShape(12.dp),
            colors = ButtonDefaults.buttonColors(containerColor = ExpenseRed)
        ) {
            Icon(Icons.Filled.Logout, null, tint = TextPrimary)
            Spacer(Modifier.width(8.dp))
            Text("Sair", fontWeight = FontWeight.Bold, color = TextPrimary)
        }
    }
}

@Composable
private fun ProfileMenuItem(icon: androidx.compose.ui.graphics.vector.ImageVector, title: String, onClick: () -> Unit) {
    Card(
        modifier = Modifier.fillMaxWidth().padding(vertical = 4.dp),
        shape = RoundedCornerShape(12.dp),
        colors = CardDefaults.cardColors(containerColor = SurfaceDark),
        onClick = onClick
    ) {
        Row(Modifier.padding(16.dp).fillMaxWidth(), verticalAlignment = Alignment.CenterVertically) {
            Icon(icon, null, tint = PrimaryGreen, modifier = Modifier.size(24.dp))
            Spacer(Modifier.width(16.dp))
            Text(title, color = TextPrimary, fontWeight = FontWeight.Medium, modifier = Modifier.weight(1f))
            Icon(Icons.Filled.ChevronRight, null, tint = TextMuted)
        }
    }
}
