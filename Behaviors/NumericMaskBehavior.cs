using Microsoft.Maui.Controls;
using System.Linq;
using System.Threading.Tasks;

namespace controle_ja_mobile.Behaviors
{
    public class NumericMaskBehavior : Behavior<Entry>
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

        // Transformamos o método em ASYNC
        private async void OnEntryTextChanged(object sender, TextChangedEventArgs args)
        {
            if (_isFormatting) return;

            if (string.IsNullOrWhiteSpace(args.NewTextValue))
                return;

            var entry = (Entry)sender;
            string numbersOnly = new string(args.NewTextValue.Where(char.IsDigit).ToArray());

            if (string.IsNullOrEmpty(numbersOnly))
            {
                _isFormatting = true;
                entry.Text = string.Empty;
                _isFormatting = false;
                return;
            }

            if (long.TryParse(numbersOnly, out long parsedValue))
            {
                string formatted = parsedValue.ToString("N0", new System.Globalization.CultureInfo("pt-BR"));

                if (entry.Text != formatted)
                {
                    _isFormatting = true;

                    entry.Text = formatted;

                    // O Pulo do Gato: Esperamos 15 milissegundos para garantir 
                    // que a tela do Android terminou de desenhar o "ponto" (.)
                    await Task.Delay(15);

                    // Agora sim, forçamos o cursor para o final absoluto
                    entry.CursorPosition = formatted.Length;

                    _isFormatting = false;
                }
            }
        }
    }
}