using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace controle_ja_mobile.Converters
{
    public class TypeToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is controle_ja_mobile.Models.TransactionType transactionType)
            {
                return transactionType switch
                {
                    controle_ja_mobile.Models.TransactionType.RECEITA => "ic_receita.png",
                    controle_ja_mobile.Models.TransactionType.DESPESA => "ic_despesa.png",
                    _ => null
                };
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
