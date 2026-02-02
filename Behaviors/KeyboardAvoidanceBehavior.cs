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

        protected override void OnAttachedTo(ScrollView bindable)
        {
            base.OnAttachedTo(bindable);

            bindable.SizeChanged += (sender, args) =>
            {
                var scrollView = bindable as ScrollView;
                if (scrollView != null && scrollView.Height > 0)
                {
                    if (_defaultHeight == 0)
                        _defaultHeight = scrollView.Height;

                    // Ajustar rolagem apenas quando altura for reduzida devido ao teclado
                    if (_defaultHeight > scrollView.Height)
                        scrollView.ScrollToAsync(0, _defaultHeight - scrollView.Height, true);
                }
            };
        }

        protected override void OnDetachingFrom(ScrollView bindable)
        {
            base.OnDetachingFrom(bindable);
            bindable.SizeChanged -= (sender, args) => { };
        }
    }
}
