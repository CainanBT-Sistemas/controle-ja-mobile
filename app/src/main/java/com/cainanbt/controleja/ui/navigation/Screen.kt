package com.cainanbt.controleja.ui.navigation

sealed class Screen(val route: String) {
    data object Login : Screen("login")
    data object Register : Screen("register")
    data object Main : Screen("main")
    data object Dashboard : Screen("dashboard")
    data object Transactions : Screen("transactions")
    data object CreditCards : Screen("credit_cards")
    data object Vehicles : Screen("vehicles")
    data object Profile : Screen("profile")
    data object InvoiceDetails : Screen("invoice_details/{cardId}") {
        fun createRoute(cardId: String) = "invoice_details/$cardId"
    }
}
