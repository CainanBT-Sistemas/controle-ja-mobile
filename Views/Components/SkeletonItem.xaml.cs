namespace controle_ja_mobile.Views.Components;

public partial class SkeletonItem : ContentView
{
    private bool _isAnimating = false;

    public SkeletonItem()
    {
        InitializeComponent();
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();
        if (Handler != null)
        {
            _isAnimating = true;
            AnimateShimmer();
        }
        else
        {
            _isAnimating = false;
        }
    }

    private async void AnimateShimmer()
    {
        while (_isAnimating)
        {
            await Task.WhenAll(
                ShimmerIcon.FadeTo(0.2, 500, Easing.Linear),
                ShimmerTitle.FadeTo(0.2, 500, Easing.Linear),
                ShimmerSubtitle.FadeTo(0.2, 500, Easing.Linear),
                ShimmerAmount.FadeTo(0.2, 500, Easing.Linear)
            );

            if (!_isAnimating) break;

            await Task.WhenAll(
                ShimmerIcon.FadeTo(0.8, 500, Easing.Linear),
                ShimmerTitle.FadeTo(0.8, 500, Easing.Linear),
                ShimmerSubtitle.FadeTo(0.8, 500, Easing.Linear),
                ShimmerAmount.FadeTo(0.8, 500, Easing.Linear)
            );
        }
    }
}