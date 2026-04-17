using System.Collections.Generic;

namespace controle_ja_mobile.Helpers
{
    public static class UIConstants
    {
        // MÁGICA DA PERFORMANCE E UX: Paleta reduzida para as 18 cores principais (Material Design)
        public static readonly List<string> AvailableColors = new()
        {
            "#F44336", // Vermelho
            "#E91E63", // Rosa
            "#9C27B0", // Roxo
            "#673AB7", // Roxo Escuro
            "#3F51B5", // Indigo
            "#2196F3", // Azul
            "#03A9F4", // Azul Claro
            "#00BCD4", // Ciano
            "#009688", // Verde Água (Teal)
            "#4CAF50", // Verde
            "#8BC34A", // Verde Claro
            "#CDDC39", // Lima
            "#FFC107", // Âmbar
            "#FF9800", // Laranja
            "#FF5722", // Laranja Escuro
            "#795548", // Marrom
            "#9E9E9E", // Cinza
            "#607D8B"  // Azul Cinzento
        };

        // ==========================================
        // LISTAS CURADAS (Alta Performance)
        // ==========================================

        public static readonly List<string> AccountIcons = new()
        {
            "account_balance_wallet", "account_balance", "savings", "store"
        };

        public static readonly List<string> CardIcons = new()
        {
            "credit_card", "local_dining", "work", "card_giftcard", "flight"
        };

        public static readonly List<string> CategoryIcons = new()
        {
            "restaurant", "fastfood", "local_cafe", "directions_car", "local_gas_station",
            "flight", "home", "water_drop", "bolt", "wifi", "local_hospital", "fitness_center",
            "shopping_cart", "local_mall", "sports_esports", "pets", "movie", "school",
            "category", "star", "favorite", "attach_money", "receipt_long", "checkroom"
        };
    }
}