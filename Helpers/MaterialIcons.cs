using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace controle_ja_mobile.Helpers
{
    // AQUI ESTAVA O ERRO: Precisa ser 'public static class' para o XAML enxergar
    public static class MaterialIcons
    {
        // Menu Inferior
        public const string Home = "\ue88a";          // Casa (Início)
        public const string ReceiptLong = "\ue8b0";   // Extrato (Papelzinho)
        public const string CreditCard = "\ue870";    // Cartões
        public const string DirectionsCar = "\ue531"; // Veículos
        public const string Person = "\ue7fd";        // Perfil / Settings

        // Ações
        public const string Add = "add";           // Botão + (FAB)
        public const string TrendingUp = "trending_up";     // Para Receita
        public const string TrendingDown = "trending_down"; // Para Despesa

        // Categorias (Para usarmos depois)
        public const string Restaurant = "\ue56c";
        public const string School = "\ue80c";
        public const string AttachMoney = "\ue227";
        public const string Category = "\ue574";

        public const string CheckCircleOutline = "check_circle_outline";
        public const string CalendarToday = "calendar_today";
        public const string Notes = "notes";
        public const string Label = "label";
        public const string AccountBalanceWallet = "account_balance_wallet";
        public const string Repeat = "repeat";
        public const string Check = "check";
    }
}