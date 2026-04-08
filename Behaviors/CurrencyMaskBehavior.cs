using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace controle_ja_mobile.Behaviors
{
    public class CurrencyMaskBehavior : Behavior<Entry>
    {
        private bool _isFormatting = false;

        protected override void OnAttachedTo(Entry entry)
        {
            entry.TextChanged += OnEntryTextChanged;
            base.OnAttachedTo(entry);
        }

        protected override void OnDetachingFrom(Entry entry)
        {
            entry.TextChanged -= OnEntryTextChanged;
            base.OnDetachingFrom(entry);
        }

        private async void OnEntryTextChanged(object sender, TextChangedEventArgs args)
        {
            if (_isFormatting) return;

            var entry = (Entry)sender;

            // Se o usuário apagar tudo, podemos deixar "0,00" ou vazio.
            if (string.IsNullOrWhiteSpace(args.NewTextValue))
                return;

            // 1. Pega APENAS os números digitados, ignorando vírgulas e pontos antigos
            string numbersOnly = new string(args.NewTextValue.Where(char.IsDigit).ToArray());

            if (string.IsNullOrEmpty(numbersOnly))
            {
                _isFormatting = true;
                entry.Text = string.Empty;
                _isFormatting = false;
                return;
            }

            // 2. A Mágica: Converte a string de números para Decimal e divide por 100
            if (decimal.TryParse(numbersOnly, out decimal parsedValue))
            {
                decimal finalValue = parsedValue / 100m;

                // 3. Formata com 2 casas decimais ("N2") e separador de milhar no padrão BR
                string formatted = finalValue.ToString("N2", new System.Globalization.CultureInfo("pt-BR"));

                if (entry.Text != formatted)
                {
                    _isFormatting = true;

                    entry.Text = formatted;

                    // O pequeno atraso para a tela do Android não se perder com o cursor
                    await Task.Delay(15);

                    // Trava o cursor sempre no final
                    entry.CursorPosition = formatted.Length;

                    _isFormatting = false;
                }
            }
        }
    }
}
