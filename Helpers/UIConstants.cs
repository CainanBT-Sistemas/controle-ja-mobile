using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace controle_ja_mobile.Helpers
{
    public static class UIConstants
    {
        // Lista centralizada com as 64 cores
        public static readonly List<string> AvailableColors = new()
        {
            // Vermelhos
            "#FFEBEE", "#FFCDD2", "#EF9A9A", "#E57373", "#EF5350", "#F44336", "#E53935", "#D32F2F",
            // Rosas
            "#FCE4EC", "#F8BBD0", "#F48FB1", "#F06292", "#EC407A", "#E91E63", "#D81B60", "#C2185B",
            // Roxos
            "#F3E5F5", "#E1BEE7", "#CE93D8", "#BA68C8", "#AB47BC", "#9C27B0", "#8E24AA", "#7B1FA2",
            // Azuis
            "#E3F2FD", "#BBDEFB", "#90CAF9", "#64B5F6", "#42A5F5", "#2196F3", "#1E88E5", "#1976D2",
            // Cianos e Teals
            "#E0F7FA", "#B2EBF2", "#80DEEA", "#4DD0E1", "#26C6DA", "#00BCD4", "#00ACC1", "#0097A7",
            // Verdes
            "#E8F5E9", "#C8E6C9", "#A5D6A7", "#81C784", "#66BB6A", "#4CAF50", "#43A047", "#388E3C",
            // Amarelos e Laranjas
            "#FFFDE7", "#FFF59D", "#FFF176", "#FFEE58", "#FFEB3B", "#FDD835", "#FF9800", "#F57C00",
            // Marrons e Cinzas
            "#EFEBE9", "#D7CCC8", "#BCAAA4", "#A1887F", "#8D6E63", "#795548", "#6D4C41", "#5D4037"
        };

        // 64 ÍCONES MATERIAL DESIGN (Focados em Finanças e Estilo de Vida)
        public static readonly List<string> AvailableIcons = new()
        {
            // Finanças e Contas (12)
            "account_balance", "account_balance_wallet", "credit_card", "savings",
            "payments", "attach_money", "monetization_on", "receipt",
            "receipt_long", "pie_chart", "trending_up", "trending_down",

            // Alimentação (4)
            "restaurant", "fastfood", "local_cafe", "local_bar",

            // Transporte (7)
            "directions_car", "local_gas_station", "directions_bus", "flight",
            "train", "two_wheeler", "pedal_bike",

            // Moradia e Contas Fixas (6)
            "home", "apartment", "water_drop", "bolt", "wifi", "build",

            // Saúde e Cuidados (5)
            "local_hospital", "medical_services", "fitness_center", "spa", "content_cut",

            // Compras e Vestuário (4)
            "shopping_cart", "local_mall", "store", "checkroom",

            // Lazer, Hobbies e Pets (10) - Corrigido "theaters"
            "sports_soccer", "sports_esports", "gamepad", "pets", "movie",
            "theaters", "music_note", "celebration", "cake", "card_giftcard",

            // Educação e Família (3)
            "school", "menu_book", "child_care",

            // Tecnologia e Trabalho (7)
            "computer", "smartphone", "tv", "headphones", "camera_alt",
            "work", "work_outline",

            // Genéricos (6)
            "category", "star", "favorite", "public", "account_circle", "palette"
        };
    }
}
