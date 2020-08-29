using AndroidX.ViewPager.Widget;

namespace CarouselView
{
    public interface IPageIndicator
    {
        void SetViewPager(ViewPager view);
        void SetViewPager(ViewPager view, int initialPosition);
        void SetCurrentItem(int item);
        void SetOnPageChangeListener(ViewPager.IOnPageChangeListener listener);
        void NotifyDataSetChanged();
    }
}