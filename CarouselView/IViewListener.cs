using Android.Views;

namespace CarouselView
{
    public interface IViewListener
    {
        View SetViewForPosition(int position);
    }
}