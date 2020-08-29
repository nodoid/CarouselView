using Android.Content;
using Android.Graphics;
using Android.Widget;

namespace CarouselView
{
    public class CarouselViewPagerScroller : Scroller
    {
        int mScrollDuration = 600;

        public CarouselViewPagerScroller(Context context) : base(context)
        {
        }

        public CarouselViewPagerScroller(Context context, Interpolator interpolator) : base(context, (Android.Views.Animations.IInterpolator)interpolator)
        {
        }

        public int ScrollDuration
        {
            get => mScrollDuration;
            set => mScrollDuration = value;
        }

        public override void StartScroll(int startX, int startY, int dx, int dy, int duration)
        {
            base.StartScroll(startX, startY, dx, dy, duration);
        }

        public override void StartScroll(int startX, int startY, int dx, int dy)
        {
            base.StartScroll(startX, startY, dx, dy);
        }
    }
}