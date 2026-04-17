package com.cainanbt.controleja.ui.navigation

import androidx.compose.foundation.background
import androidx.compose.foundation.layout.*
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.*
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.vector.ImageVector
import androidx.compose.ui.unit.dp
import androidx.navigation.NavDestination.Companion.hierarchy
import androidx.navigation.NavGraph.Companion.findStartDestination
import androidx.navigation.compose.NavHost
import androidx.navigation.compose.composable
import androidx.navigation.compose.currentBackStackEntryAsState
import androidx.navigation.compose.rememberNavController
import com.cainanbt.controleja.ui.screens.auth.LoginScreen
import com.cainanbt.controleja.ui.screens.auth.RegisterScreen
import com.cainanbt.controleja.ui.screens.creditcards.CreditCardsScreen
import com.cainanbt.controleja.ui.screens.dashboard.DashboardScreen
import com.cainanbt.controleja.ui.screens.settings.ProfileScreen
import com.cainanbt.controleja.ui.screens.transactions.TransactionsScreen
import com.cainanbt.controleja.ui.screens.vehicles.VehiclesScreen
import com.cainanbt.controleja.ui.theme.*

data class BottomNavItem(
    val label: String,
    val icon: ImageVector,
    val route: String
)

@Composable
fun AppNavigation() {
    val navController = rememberNavController()

    NavHost(
        navController = navController,
        startDestination = Screen.Login.route
    ) {
        composable(Screen.Login.route) {
            LoginScreen(
                onLoginSuccess = {
                    navController.navigate(Screen.Main.route) {
                        popUpTo(Screen.Login.route) { inclusive = true }
                    }
                },
                onNavigateToRegister = { navController.navigate(Screen.Register.route) }
            )
        }

        composable(Screen.Register.route) {
            RegisterScreen(
                onRegisterSuccess = { navController.popBackStack() },
                onNavigateBack = { navController.popBackStack() }
            )
        }

        composable(Screen.Main.route) {
            MainScreen(
                onLogout = {
                    navController.navigate(Screen.Login.route) {
                        popUpTo(0) { inclusive = true }
                    }
                }
            )
        }
    }
}

@Composable
fun MainScreen(onLogout: () -> Unit) {
    val bottomNavController = rememberNavController()
    val navBackStackEntry by bottomNavController.currentBackStackEntryAsState()
    val currentDestination = navBackStackEntry?.destination

    val items = listOf(
        BottomNavItem("Início", Icons.Filled.Home, Screen.Dashboard.route),
        BottomNavItem("Extrato", Icons.Filled.Receipt, Screen.Transactions.route),
        BottomNavItem("Cartões", Icons.Filled.CreditCard, Screen.CreditCards.route),
        BottomNavItem("Veículos", Icons.Filled.DirectionsCar, Screen.Vehicles.route),
        BottomNavItem("Perfil", Icons.Filled.Person, Screen.Profile.route)
    )

    Scaffold(
        bottomBar = {
            NavigationBar(
                containerColor = SurfaceDark,
                contentColor = TextPrimary,
                tonalElevation = 0.dp
            ) {
                items.forEach { item ->
                    val selected = currentDestination?.hierarchy?.any { it.route == item.route } == true
                    NavigationBarItem(
                        selected = selected,
                        onClick = {
                            bottomNavController.navigate(item.route) {
                                popUpTo(bottomNavController.graph.findStartDestination().id) { saveState = true }
                                launchSingleTop = true
                                restoreState = true
                            }
                        },
                        icon = { Icon(item.icon, contentDescription = item.label) },
                        label = { Text(item.label) },
                        colors = NavigationBarItemDefaults.colors(
                            selectedIconColor = PrimaryGreen,
                            selectedTextColor = PrimaryGreen,
                            unselectedIconColor = TextMuted,
                            unselectedTextColor = TextMuted,
                            indicatorColor = PrimaryGreen.copy(alpha = 0.1f)
                        )
                    )
                }
            }
        }
    ) { paddingValues ->
        NavHost(
            navController = bottomNavController,
            startDestination = Screen.Dashboard.route,
            modifier = Modifier
                .fillMaxSize()
                .padding(paddingValues)
                .background(PrimaryDark)
        ) {
            composable(Screen.Dashboard.route) {
                DashboardScreen()
            }
            composable(Screen.Transactions.route) {
                TransactionsScreen()
            }
            composable(Screen.CreditCards.route) {
                CreditCardsScreen()
            }
            composable(Screen.Vehicles.route) {
                VehiclesScreen()
            }
            composable(Screen.Profile.route) {
                ProfileScreen(onLogout = onLogout)
            }
        }
    }
}
