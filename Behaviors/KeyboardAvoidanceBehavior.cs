using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace controle_ja_mobile.Behaviors
{
    public class KeyboardAvoidanceBehavior : Behavior<ScrollView>
    {
        private double _defaultHeight;

        protected override void OnAttachedTo(ScrollView scrollView)
        {
            base.OnAttachedTo(scrollView);

            // Salvar o tamanho inicial
            scrollView.SizeChanged += (sender, args) =>
            {
                if (_defaultHeight == 0)
                    _defaultHeight = scrollView.Height;
            };

            scrollView.Focused += (sender, args) =>
            {
                var element = sender as ScrollView;
                if (element != null)
                {
                    // Ajustar rolagem para trazer elementos ativos quando necessário
                    element.ScrollToAsync(0, 100, true);
                }
            };
        }
    }
}
