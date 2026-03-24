using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace controle_ja_mobile.Converters
{
    public class StringToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var iconName = value as string;

            // 1. Se vier vazio, usa ícone padrão
            if (string.IsNullOrWhiteSpace(iconName))
                return "category_36dp.png";

            // 2. Normaliza para letras minúsculas para comparar
            var lowerName = iconName.ToLower().Trim();

            // 3. Verifica se já é um arquivo que existe no seu projeto (ex: car_36dp.svg)
            // Se o nome já contiver "36dp" ou ".", retornamos ele mesmo (adicionando .png se precisar)
            if (lowerName.Contains("36dp") || lowerName.Contains("."))
            {
                if (!lowerName.EndsWith(".png") && !lowerName.EndsWith(".svg"))
                    return $"{lowerName}.png";
                return iconName;
            }

            // 4. MAPA DE TRADUÇÃO (API -> SEUS ÍCONES LOCAIS)
            // A esquerda: O que vem da API ("home", "restaurant")
            // A direita: O arquivo que você tem na pasta Images ("home_36dp.png")
            return lowerName switch
            {
                "home" => "category_36dp.png",         // Troque por home_36dp.png se tiver
                "restaurant" => "category_36dp.png",   // Troque por food_36dp.png se tiver
                "school" => "category_36dp.png",       // Troque por school_36dp.png se tiver
                "directions_car" => "car_36dp.png",    // Esse você tem!
                "attach_money" => "account.svg",       // Esse você tem!
                "trending_up" => "category_36dp.png",
                "add_circle" => "category_36dp.png",
                _ => "category_36dp.png" // Fallback seguro
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;
    }
}
