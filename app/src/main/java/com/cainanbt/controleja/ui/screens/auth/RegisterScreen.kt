package com.cainanbt.controleja.ui.screens.auth

import androidx.compose.foundation.background
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.*
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.text.input.PasswordVisualTransformation
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.hilt.navigation.compose.hiltViewModel
import com.cainanbt.controleja.ui.theme.*

@Composable
fun RegisterScreen(
    viewModel: RegisterViewModel = hiltViewModel(),
    onRegisterSuccess: () -> Unit,
    onNavigateBack: () -> Unit
) {
    val uiState by viewModel.uiState.collectAsState()

    LaunchedEffect(uiState.isRegistered) {
        if (uiState.isRegistered) onRegisterSuccess()
    }

    val fieldColors = OutlinedTextFieldDefaults.colors(
        focusedBorderColor = PrimaryGreen,
        unfocusedBorderColor = DividerColor,
        focusedLabelColor = PrimaryGreen,
        cursorColor = PrimaryGreen,
        focusedTextColor = TextPrimary,
        unfocusedTextColor = TextPrimary
    )

    Box(
        modifier = Modifier.fillMaxSize().background(PrimaryDark).padding(24.dp),
        contentAlignment = Alignment.Center
    ) {
        Column(horizontalAlignment = Alignment.CenterHorizontally) {
            Text("Criar Conta", style = MaterialTheme.typography.headlineLarge, color = PrimaryGreen, fontWeight = FontWeight.Bold)
            Text("Preencha os dados para se cadastrar", color = TextSecondary, style = MaterialTheme.typography.bodyMedium)
            Spacer(modifier = Modifier.height(32.dp))

            OutlinedTextField(
                value = uiState.username, onValueChange = viewModel::onUsernameChange,
                label = { Text("Nome") }, leadingIcon = { Icon(Icons.Filled.Person, null) },
                singleLine = true, modifier = Modifier.fillMaxWidth(), shape = RoundedCornerShape(12.dp), colors = fieldColors
            )
            Spacer(modifier = Modifier.height(12.dp))

            OutlinedTextField(
                value = uiState.email, onValueChange = viewModel::onEmailChange,
                label = { Text("Email") }, leadingIcon = { Icon(Icons.Filled.Email, null) },
                keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Email),
                singleLine = true, modifier = Modifier.fillMaxWidth(), shape = RoundedCornerShape(12.dp), colors = fieldColors
            )
            Spacer(modifier = Modifier.height(12.dp))

            OutlinedTextField(
                value = uiState.password, onValueChange = viewModel::onPasswordChange,
                label = { Text("Senha") }, leadingIcon = { Icon(Icons.Filled.Lock, null) },
                visualTransformation = PasswordVisualTransformation(),
                singleLine = true, modifier = Modifier.fillMaxWidth(), shape = RoundedCornerShape(12.dp), colors = fieldColors
            )

            uiState.error?.let { Spacer(Modifier.height(8.dp)); Text(it, color = ExpenseRed, style = MaterialTheme.typography.bodySmall) }
            Spacer(modifier = Modifier.height(24.dp))

            Button(
                onClick = viewModel::register, modifier = Modifier.fillMaxWidth().height(52.dp),
                shape = RoundedCornerShape(12.dp), colors = ButtonDefaults.buttonColors(containerColor = PrimaryGreen),
                enabled = !uiState.isLoading
            ) {
                if (uiState.isLoading) CircularProgressIndicator(Modifier.size(24.dp), color = PrimaryDark, strokeWidth = 2.dp)
                else Text("Cadastrar", fontWeight = FontWeight.Bold, fontSize = 16.sp, color = PrimaryDark)
            }

            Spacer(modifier = Modifier.height(16.dp))
            TextButton(onClick = onNavigateBack) { Text("Já tem conta? Faça login", color = PrimaryGreen) }
        }
    }
}
