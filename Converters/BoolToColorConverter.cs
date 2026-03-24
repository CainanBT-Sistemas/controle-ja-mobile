using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace controle_ja_mobile.Converters
{
    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isExpensesSelected && parameter is string targetTab)
            {
                // Definição das cores
                var colorExpense = Color.FromArgb("#FF5252"); // Vermelho
                var colorIncome = Color.FromArgb("#00E676");  // Verde
                var colorInactive = Color.FromArgb("#71717A"); // Cinza

                // Verifica se o parametro passado é 'True' (Aba Despesa) ou 'False' (Aba Receita)
                bool isTargetExpenseTab = targetTab.ToLower() == "true";

                // Se a Aba selecionada for DESPESA (true) e estamos pintando a aba DESPESA
                if (isExpensesSelected && isTargetExpenseTab)
                    return colorExpense;

                // Se a Aba selecionada for RECEITA (false) e estamos pintando a aba RECEITA
                if (!isExpensesSelected && !isTargetExpenseTab)
                    return colorIncome;

                return colorInactive;
            }

            return Colors.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;
    }
}
