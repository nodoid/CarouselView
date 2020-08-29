using System;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using AndroidX.ViewPager.Widget;
using Java.Lang;
using Java.Util;
using Timer = Java.Util.Timer;

namespace CarouselView
{
    public class CarouselView : FrameLayout, ViewPager.IOnPageChangeListener
    {
        GravityFlags DEFAULT_GRAVITY = GravityFlags.CenterHorizontal | GravityFlags.Bottom;

        static int DEFAULT_SLIDE_INTERVAL = 3500;
        static int DEFAULT_SLIDE_VELOCITY = 400;
        public static int DEFAULT_INDICATOR_VISIBILITY = 0;

        int mPageCount;
        int slideInterval = DEFAULT_SLIDE_INTERVAL;
        GravityFlags mIndicatorGravity;
        int indicatorMarginVertical;
        int indicatorMarginHorizontal;
        int pageTransformInterval = DEFAULT_SLIDE_VELOCITY;
        int indicatorVisibility = DEFAULT_INDICATOR_VISIBILITY;

        CarouselViewPager containerViewPager;
        CirclePageIndicator mIndicator;
        IViewListener mViewListener = null;
        IImageListener mImageListener = null;
        IImageClickListener imageClickListener = null;

        Timer swipeTimer;
        SwipeTask swipeTask;

        bool autoPlay;
        bool disableAutoPlayOnUserInteraction;
        bool animateOnBoundary = true;

        int previousState;

        ViewPager.IPageTransformer pageTransformer;

        protected CarouselView(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }
        public CarouselView(Context c) : base(c)
        {
            InitView(c);
        }

        public CarouselView(Context c, IAttributeSet attribs) : base(c, attribs)
        {
            InitView(c, attribs);
        }

        public CarouselView(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes)
        {
            InitView(context, attrs, defStyleAttr, defStyleRes);
        }

        void InitView(Context context, IAttributeSet attrs = null, int defStyleAttr = 0, int defStyleRes = 0)
        {
            if (IsInEditMode)
            {
                return;
            }
            else
            {
                View view = LayoutInflater.From(context).Inflate(Resource.Layout.view_carousel, this, true);
                containerViewPager = (CarouselViewPager)view.FindViewById(Resource.Id.containerViewPager);
                mIndicator = (CirclePageIndicator)view.FindViewById(Resource.Id.indicator);

                containerViewPager.AddOnPageChangeListener((ViewPager.IOnPageChangeListener)carouselOnPageChangeListener);


                //Retrieve styles attributes
                var a = context.ObtainStyledAttributes(attrs, Resource.Styleable.CarouselView, defStyleAttr, 0);
                try
                {
                    indicatorMarginVertical = a.GetDimensionPixelSize(Resource.Styleable.CarouselView_indicatorMarginVertical,
                        Resources.GetDimensionPixelSize(Resource.Dimension.default_indicator_margin_vertical));
                    indicatorMarginHorizontal = a.GetDimensionPixelSize(Resource.Styleable.CarouselView_indicatorMarginHorizontal,
                        Resources.GetDimensionPixelSize(Resource.Dimension.default_indicator_margin_horizontal));
                    SetPageTransformInterval(a.GetInt(Resource.Styleable.CarouselView_pageTransformInterval, DEFAULT_SLIDE_VELOCITY));
                    SlideInterval=a.GetInt(Resource.Styleable.CarouselView_slideInterval, DEFAULT_SLIDE_INTERVAL);
                    Orientation=a.GetInt(Resource.Styleable.CarouselView_indicatorOrientation, LinearLayoutCompat.Horizontal);
                    IndicatorGravity=a.GetInt(Resource.Styleable.CarouselView_indicatorGravity, GravityFlags.Bottom | GravityFlags.CenterHorizontal);
                    IsAutoPlay=a.GetBoolean(Resource.Styleable.CarouselView_autoPlay, true);
                    SetDisableAutoPlayOnUserInteraction(a.GetBoolean(Resource.Styleable.CarouselView_disableAutoPlayOnUserInteraction, false));
                    SetAnimateOnBoundary(a.GetBoolean(Resource.Styleable.CarouselView_animateOnBoundary, true));
                    SetPageTransformer(a.GetInt(Resource.Styleable.CarouselView_pageTransformer, CarouselViewPagerTransformer.DEFAULT));

                    indicatorVisibility = a.GetInt(Resource.Styleable.CarouselView_indicatorVisibility, CarouselView.DEFAULT_INDICATOR_VISIBILITY);

                    SetIndicatorVisibility(indicatorVisibility);

                    if (indicatorVisibility == (int)SystemUiFlags.Visible)
                    {
                        var fillColor = a.GetColor(Resource.Styleable.CarouselView_fillColor, 0);
                        if (fillColor != 0)
                        {
                            FillColor=fillColor;
                        }
                        var pageColor = a.GetColor(Resource.Styleable.CarouselView_pageColor, 0);
                        if (pageColor != 0)
                        {
                            PageColor=pageColor;
                        }
                        float radius = a.GetDimensionPixelSize(Resource.Styleable.CarouselView_radius, 0);
                        if (radius != 0)
                        {
                            Radius=radius;
                        }
                        IsSnap = a.GetBoolean(Resource.Styleable.CarouselView_snap, Resources.GetBoolean(Resource.Boolean.default_circle_indicator_snap));
                        var strokeColor = a.GetColor(Resource.Styleable.CarouselView_strokeColor, 0);
                        if (strokeColor != 0)
                        {
                            StrokeColor=strokeColor;
                        }
                        float strokeWidth = a.GetDimensionPixelSize(Resource.Styleable.CarouselView_strokeWidth, 0);
                        if (strokeWidth != 0)
                        {
                            StrokeWidth=strokeWidth;
                        }
                    }
                }
                finally
                {
                    a.Recycle();
                }
            }
        }

        protected override void OnDetachedFromWindow()
        {
            base.OnDetachedFromWindow();
            ResetScrollTimer();
        }

        protected override void OnAttachedToWindow()
        {
            base.OnAttachedToWindow();
            PlayCarousel();
        }

        public int GetSliderInterval => slideInterval;

        public void SetSliderInterval(int interval)
        {
            slideInterval = interval;
            if (containerViewPager != null)
                PlayCarousel();
        }

        public void RsetSlideInterval(int interval)
        {
            SetSliderInterval(interval);
            if (containerViewPager != null)
                PlayCarousel();
        }

        public void SetPageTransformInterval(int interval)
        {
            pageTransformInterval = pageTransformInterval > 0 ? interval : DEFAULT_SLIDE_INTERVAL;

            containerViewPager.TrasmissionVelocity=pageTransformInterval;
        }

        public ViewPager.IPageTransformer GetPageTransformer => pageTransformer;

        public void SetPageTransformer(ViewPager.IPageTransformer transform)
        {
            pageTransformer = transform;
            containerViewPager.SetPageTransformer(true, pageTransformer);
        }

        public void SetAnimateBoundary(bool animate)
        {
            animateOnBoundary = animate;
        }

        public bool IsAutoPlay
        {
            get => autoPlay;
            set => autoPlay = value;
        }

        public bool IsDisableAutoPlayOnUserInteteraction
        {
            get => disableAutoPlayOnUserInteraction;
            set => disableAutoPlayOnUserInteraction = value;
        }

        void SetData()
        {
            CarouselPagerAdapter carouselPagerAdapter = new CarouselPagerAdapter(Context);
            containerViewPager.Adapter = carouselPagerAdapter;
            if (PageCount > 1)
            {
                mIndicator.SetViewPager(containerViewPager);
                mIndicator.RequestLayout();
                mIndicator.Invalidate();
                containerViewPager.OffscreenPageLimit=PageCount;
                PlayCarousel();
            }
        }

        void StopScrollTimer()
        {
            if (swipeTimer != null)
                swipeTimer.Stop();
            if (swipeTask != null)
                swipeTask.Cancel();
        }

        void ResetScrollTimer()
        {
            StopScrollTimer();
            swipeTask = new SwipeTask();
            swipeTimer = new Timer();
        }

        public void PlayCarousel()
        {
            ResetScrollTimer();
            if (autoPlay && slideInterval > 0 && containerViewPager.Adapter != null && containerViewPager.Adapter.Count > 0)
            {
                swipeTimer.Interval = slideInterval;
                swipeTimer.Start();
            }
        }

        public void PauseCarousel()
        {
            ResetScrollTimer();
        }

        public void StopCarousel()
        {
            autoPlay = false;
        }

        public void OnPageScrollStateChanged(int state)
        {
            if (previousState == ViewPager.ScrollStateDragging
                    && state == ViewPager.ScrollStateSettling)
            {

                if (disableAutoPlayOnUserInteraction)
                {
                    PauseCarousel();
                }
                else
                {
                    PlayCarousel();
                }

            }
            else if (previousState == ViewPager.ScrollStateSettling
                  && state == ViewPager.ScrollStateIdle)
            {
            }

            previousState = state;
        }

        public void OnPageScrolled(int position, float positionOffset, int positionOffsetPixels)
        {
        }

        public void OnPageSelected(int position)
        {
        }

        public void SetImageListener(IImageListener listener)
        {
            mImageListener = listener;
        }

        public void SetViewListener(IViewListener listener)
        {
            mViewListener = listener;
        }

        public void SetImageClickListener(IImageClickListener listener)
        {
            imageClickListener = listener;
            containerViewPager.SetImageClickListener(listener);
        }

        public int PageCount
        {
            get => mPageCount;
            set { mPageCount = value; SetData(); }
        }

        public void AddOnPageChangeListener(ViewPager.IOnPageChangeListener listener)
        {
            containerViewPager.AddOnPageChangeListener(listener);
        }

        public void ClearOnPageChangeListeners()
        {
            containerViewPager.ClearOnPageChangeListeners();
        }

        public void SetCurrentItem(int item)
        {
            containerViewPager.CurrentItem = item;
        }

        public void SetCurrentItem(int item, bool smoothScroll)
        {
            containerViewPager.SetCurrentItem(item, smoothScroll);
        }

        public int IndicatorMarginVertical
        {
            get => indicatorMarginVertical;
            set
            {
                indicatorMarginVertical = value;
                FrameLayout.LayoutParams layout = (LayoutParams)LayoutParameters;
                layout.TopMargin = indicatorMarginVertical;
                layout.BottomMargin = indicatorMarginVertical;
            }
        }

        public int IndicatorMarginHorizontal
        {
            get => indicatorMarginHorizontal;
            set
            {
                indicatorMarginHorizontal = value;
                FrameLayout.LayoutParams layout = (LayoutParams)LayoutParameters;
                layout.LeftMargin = indicatorMarginHorizontal;
                layout.RightMargin = indicatorMarginHorizontal;
            }
        }
        public int GetCurrentItem() => containerViewPager.CurrentItem;

        public CarouselViewPager GetContainerViewPager()
        {
            return containerViewPager;
        }

        public GravityFlags IndicatorGravity
        {
            get => mIndicatorGravity;
            set
            {
                mIndicatorGravity = value;
                FrameLayout.LayoutParams layout = new FrameLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
                layout.Gravity = mIndicatorGravity;
                layout.SetMargins(indicatorMarginHorizontal, indicatorMarginVertical, indicatorMarginHorizontal, indicatorMarginVertical);
                mIndicator.LayoutParameters = layout;
            }
        }

        public void SetIndicatorVisibility(int visible)
        {
            mIndicator.Visibility= (ViewStates)visible;
        }

        public Orientation Orientation
        {
            get => mIndicator.Orientation;
            set => mIndicator.Orientation = value;
        }

        public Color FillColor
        {
            get => mIndicator.FillColor;
            set => mIndicator.FillColor = value;
        }

        public Color StrokeColor
        {
            get => mIndicator.StrokeColor;
            set => mIndicator.StrokeColor = value;
        }

        public float StrokeWidth
        {
            get => mIndicator.StrokeWidth;
            set
            {
                mIndicator.StrokeWidth = value;
                var padding = (int)value;
                mIndicator.SetPadding(padding, padding, padding, padding);
            }
        }

        public Color PageColor
        {
            get => mIndicator.PageColor;
            set => mIndicator.PageColor = value;
        }

        [Obsolete]
        public override void SetBackgroundDrawable(Drawable background)
        {
            base.SetBackgroundDrawable(background);
        }

        public Drawable GetIndicatorBackground => mIndicator.Background;

        public bool IsSnap
        {
            get => mIndicator.IsSnap;
            set => mIndicator.IsSnap = value;
        }

        public float Radius
        {
            get => mIndicator.Radius;
            set => mIndicator.Radius = value;
        }

        class CarouselPagerAdapter : PagerAdapter
        {
            Context mContext;

            public CarouselPagerAdapter(Context context)
            {
                mContext = context;
            }

            [Obsolete]
            public override Java.Lang.Object InstantiateItem(View container, int position)
            {
                Java.Lang.Object objectToReturn = null;

                //Either let user set image to ImageView
                if (mImageListener != null)
                {

                    ImageView imageView = new ImageView(mContext);
                    imageView.LayoutParameters = new LayoutParams(LayoutParams.MatchParent, LayoutParams.WrapContent);  //setting image position
                    imageView.SetScaleType(ImageView.ScaleType.CenterCrop);

                    objectToReturn = imageView;
                    mImageListener.SetImageForPosition(position, imageView);

                    collection.AddView(imageView);

                    //Or let user add his own ViewGroup
                }
                else
                {
                    if (mViewListener != null)
                    {
                        View view = mViewListener.SetViewForPosition(position);

                        if (null != view)
                        {
                            objectToReturn = view;
                            collection.AddView(view);
                        }
                        else
                        {
                            throw new RuntimeException("View can not be null for position " + position);
                        }
                    }
                }
                return objectToReturn;
            }

            public override void DestroyItem(ViewGroup container, int position, Java.Lang.Object @object)
            {
                container.RemoveView((View)@object);
            }

            public override bool IsViewFromObject(View view, Java.Lang.Object @object) => view == @object;

            public override int Count => PageCount;
        }

        class SwipeTask : TimerTask
        {
            public override void Run()
            {
                containerViewPager.Post(new Runnable(t)=>
                {
                   int nextPage = (containerViewPager.getCurrentItem() + 1) % PageCount;
                    containerViewPager.setCurrentItem(nextPage, 0 != nextPage || animateOnBoundary);
                };
            }
            
        }

    }
}