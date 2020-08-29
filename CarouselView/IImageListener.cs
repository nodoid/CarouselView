using Android.Widget;

namespace CarouselView
{
    public interface IImageListener
    {
        void SetImageForPosition(int posn, ImageView image);
    }
}