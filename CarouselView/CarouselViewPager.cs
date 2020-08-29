using System;
using Android.Content;
using Android.Graphics;
using Android.Util;
using Android.Views;
using AndroidX.ViewPager.Widget;

namespace CarouselView
{
    public class CarouselViewPager : ViewPager
    {
        IImageClickListener imageClickListener;
        float oldX = 0, newX = 0, sens = 5;
        CarouselViewPagerScroller mScroller = null;
        Context context;

        public void SetImageClickListener(IImageClickListener listener)
        {
            imageClickListener = listener;
        }

        public CarouselViewPager(Context c) : base(c)
        {
            context = c;
            postInitViewPager();
        }

        public CarouselViewPager(Context c, IAttributeSet attrs) : base(c, attrs)
        {
            context = c;
            postInitViewPager();
        }

        void postInitViewPager()
        {
            try
            {
                var viewpager = ViewPager.Class;
                var scroller = viewpager.GetDeclaredField("mScroller");
                scroller.Accessible = true;
                var interpolator = viewpager.GetDeclaredField("sInterpolator");
                interpolator.Accessible = true;

                mScroller = new CarouselViewPagerScroller(context,
                        (Interpolator)interpolator.Get(null));
                scroller.Set(this, mScroller);
            }
            catch (Exception e)
            {
            }
        }

        public void SetTransitionVelocity(int factor)
        {
            mScroller.ScrollDuration = factor;
        }

        public override bool OnTouchEvent(MotionEvent e)
        {
            switch (e.Action)
            {
                case MotionEventActions.Down:
                    oldX = e.GetX();
                    break;

                case MotionEventActions.Up:
                    newX = e.GetX();
                    if (Math.Abs(oldX - newX) < sens)
                    {
                        if (imageClickListener != null)
                            imageClickListener.OnClick(CurrentItem);
                        return true;
                    }
                    oldX = 0;
                    newX = 0;
                    break;
            }
            return base.OnTouchEvent(e);
        }
    }
}