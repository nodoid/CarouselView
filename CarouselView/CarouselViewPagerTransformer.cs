using System;
using Android.Views;
using AndroidX.ViewPager.Widget;
using Java.Interop;

namespace CarouselView
{
    public enum SlideTransforms
    {
        FLOW = 0, SLIDE_OVER, DEPTH, ZOOM, DEFAULT = -1
    }
    public class CarouselViewPagerTransformer : ViewPager.IPageTransformer
    {
        int mTransformType;
        static float MIN_SCALE_DEPTH = 0.75f;
        static float MIN_SCALE_ZOOM = 0.85f;
        static float MIN_ALPHA_ZOOM = 0.5f;
        static float SCALE_FACTOR_SLIDE = 0.85f;
        static float MIN_ALPHA_SLIDE = 0.35f;

        CarouselViewPagerTransformer(int transformType)
        {
            mTransformType = transformType;
        }

        
        public IntPtr Handle => throw new NotImplementedException();
        public int JniIdentityHashCode => throw new NotImplementedException();

        public JniObjectReference PeerReference => throw new NotImplementedException();

        public JniPeerMembers JniPeerMembers => throw new NotImplementedException();

        public JniManagedPeerStates JniManagedPeerState => throw new NotImplementedException();

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Disposed()
        {
            throw new NotImplementedException();
        }

        public void DisposeUnlessReferenced()
        {
            throw new NotImplementedException();
        }

        public void Finalized()
        {
            throw new NotImplementedException();
        }

        public void SetJniIdentityHashCode(int value)
        {
            throw new NotImplementedException();
        }

        public void SetJniManagedPeerState(JniManagedPeerStates value)
        {
            throw new NotImplementedException();
        }

        public void SetPeerReference(JniObjectReference reference)
        {
            throw new NotImplementedException();
        }

        public void TransformPage(View page, float position)
        {
            float alpha;
            float scale;
            float translationX;

            switch (mTransformType)
            {
                case (int)SlideTransforms.FLOW:
                    page.RotationY = position * -30f;
                    return;

                case (int)SlideTransforms.SLIDE_OVER:
                    if (position < 0 && position > -1)
                    {
                        // this is the page to the left
                        scale = Math.Abs(Math.Abs(position) - 1) * (1.0f - SCALE_FACTOR_SLIDE) + SCALE_FACTOR_SLIDE;
                        alpha = Math.Max(MIN_ALPHA_SLIDE, 1 - Math.Abs(position));
                        int pageWidth = page.Width;
                        float translateValue = position * -pageWidth;
                        if (translateValue > -pageWidth)
                        {
                            translationX = translateValue;
                        }
                        else
                        {
                            translationX = 0;
                        }
                    }
                    else
                    {
                        alpha = 1;
                        scale = 1;
                        translationX = 0;
                    }
                    break;

                case (int)SlideTransforms.DEPTH:
                    if (position > 0 && position < 1)
                    {
                        // moving to the right
                        alpha = (1 - position);
                        scale = MIN_SCALE_DEPTH + (1 - MIN_SCALE_DEPTH) * (1 - Math.Abs(position));
                        translationX = (page.Width * -position);
                    }
                    else
                    {
                        // use default for all other cases
                        alpha = 1;
                        scale = 1;
                        translationX = 0;
                    }
                    break;

                case (int)SlideTransforms.ZOOM:
                    if (position >= -1 && position <= 1)
                    {
                        scale = Math.Max(MIN_SCALE_ZOOM, 1 - Math.Abs(position));
                        alpha = MIN_ALPHA_ZOOM +
                                (scale - MIN_SCALE_ZOOM) / (1 - MIN_SCALE_ZOOM) * (1 - MIN_ALPHA_ZOOM);
                        float vMargin = page.Height * (1 - scale) / 2;
                        float hMargin = page.Width * (1 - scale) / 2;
                        if (position < 0)
                        {
                            translationX = (hMargin - vMargin / 2);
                        }
                        else
                        {
                            translationX = (-hMargin + vMargin / 2);
                        }
                    }
                    else
                    {
                        alpha = 1;
                        scale = 1;
                        translationX = 0;
                    }
                    break;

                default:
                    return;
            }

            page.Alpha =alpha;
            page.TranslationX = translationX;
            page.ScaleX = scale;
            page.ScaleY = scale;
        }

        public void UnregisterFromRuntime()
        {
            throw new NotImplementedException();
        }
    }
}