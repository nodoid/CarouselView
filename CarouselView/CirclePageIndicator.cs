using System;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using AndroidX.Core.View;
using AndroidX.ViewPager.Widget;
using Java.Lang;
using Math = System.Math;
using Orientation = Android.Widget.Orientation;

namespace CarouselView
{
    public class CirclePageIndicator : View, IPageIndicator
    {
        static int INVALID_POINTER = -1;

        float mRadius;
        Paint mPaintPageFill = new Paint(PaintFlags.AntiAlias);
        Paint mPaintStroke = new Paint(PaintFlags.AntiAlias);
        Paint mPaintFill = new Paint(PaintFlags.AntiAlias);
        ViewPager mViewPager;
        ViewPager.IOnPageChangeListener mListener;
        int mCurrentPage;
        int mSnapPage;
        float mPageOffset;
        int mScrollState;
        Orientation mOrientation;
        bool mCentered;
        bool mSnap;

        int mTouchSlop;
        float mLastMotionX = -1;
        int mActivePointerId = INVALID_POINTER;
        bool mIsDragging;

        public CirclePageIndicator(Context context) : base(context)
        {
            this(context, null);
        }

        public CirclePageIndicator(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            this(context, attrs, Resource.Attribute.vpiCirclePageIndicatorStyle);
        }

        [Obsolete]
        public CirclePageIndicator(Context context, IAttributeSet attrs, int defStyle) : base(context, attrs, defStyle)
        {
            if (IsInEditMode) return;

            //Load defaults from resources
            int defaultPageColor = Resources.GetColor(Resource.Color.default_circle_indicator_page_color);
            int defaultFillColor = Resources.GetColor(Resource.Color.default_circle_indicator_fill_color);
            int defaultOrientation = Resources.GetInteger(Resource.Integer.default_circle_indicator_orientation);
            int defaultStrokeColor = Resources.GetColor(Resource.Color.default_circle_indicator_stroke_color);
            float defaultStrokeWidth = Resources.GetDimension(Resource.Dimension.default_circle_indicator_stroke_width);
            float defaultRadius = Resources.GetDimension(Resource.Dimension.default_circle_indicator_radius);
            bool defaultCentered = Resources.GetBoolean(Resource.Boolean.default_circle_indicator_centered);
            bool defaultSnap = Resources.GetBoolean(Resource.Boolean.default_circle_indicator_snap);

            //Retrieve styles attributes
            var a = context.ObtainStyledAttributes(attrs, Resource.Styleable.CirclePageIndicator, defStyle, 0);

            mCentered = a.GetBoolean(Resource.Styleable.CirclePageIndicator_centered, defaultCentered);
            mOrientation = a.GetInt(Resource.Styleable.CirclePageIndicator_android_orientation, defaultOrientation);
            mPaintPageFill.SetStyle(Paint.Style.Fill);
            mPaintPageFill.Color =a.GetColor(Resource.Styleable.CirclePageIndicator_pageColor, defaultPageColor);
            mPaintStroke.SetStyle(Paint.Style.Stroke);
            mPaintStroke.Color = a.GetColor(Resource.Styleable.CirclePageIndicator_strokeColor, defaultStrokeColor);
            mPaintStroke.StrokeWidth=a.GetDimension(Resource.Styleable.CirclePageIndicator_strokeWidth, defaultStrokeWidth);
            mPaintFill.SetStyle(Paint.Style.Fill);
            mPaintFill.Color=a.GetColor(Resource.Styleable.CirclePageIndicator_fillColor, defaultFillColor);
            mRadius = a.GetDimension(Resource.Styleable.CirclePageIndicator_radius, defaultRadius);
            mSnap = a.GetBoolean(Resource.Styleable.CirclePageIndicator_snap, defaultSnap);

            var background = a.GetDrawable(Resource.Styleable.CirclePageIndicator_android_background);
            if (background != null)
            {
                SetBackgroundDrawable(background);
            }

            a.Recycle();

            ViewConfiguration configuration = ViewConfiguration.Get(context);
            mTouchSlop = ViewConfigurationCompat.GetScaledPagingTouchSlop(configuration);
        }

        public bool Centered
        {
            get => mCentered;
            set
            {
                mCentered = value;
                Invalidate();
            }
        }

        public Color PageColor
        {
            get => mPaintPageFill.Color;
            set
            {
                mPaintPageFill.Color = value;
                Invalidate();

            }
        }

        public Color FillColor
        {
            get => mPaintFill.Color;
            set
            {
                mPaintFill.Color = value;
                Invalidate();
            }
        }

        public Orientation Orientation
        {
            get => mOrientation;
            set
            {
                switch (value)
                {
                    case Orientation.Horizontal:
                    case Orientation.Vertical:
                        mOrientation = value;
                        RequestLayout();
                        break;
                }
            }
        }

        public Paint StrokeColor
        {
            get => mPaintStroke;
            set
            {
                mPaintStroke = value;
                Invalidate();
            }
        }

        public float Radius
        {
            get => mRadius;
            set
            {
                mRadius = value;
                Invalidate();
            }
        }

        public bool IsSnap
        {
            get => mSnap;
            set
            {
                mSnap = value;
                Invalidate();
            }
        }

        public float StrokeWidth
        {
            get => mPaintStroke.StrokeWidth;
            set
            {
                mPaintStroke.StrokeWidth = value;
                Invalidate();
            }
        }

        protected override void OnDraw(Canvas canvas)
        {
            base.OnDraw(canvas);
            if (mViewPager == null)
            {
                return;
            }
            int count = mViewPager.Adapter.Count;
            if (count == 0)
            {
                return;
            }

            if (mCurrentPage >= count)
            {
                CurrentItem = count - 1;
                return;
            }

            int longSize;
            int longPaddingBefore;
            int longPaddingAfter;
            int shortPaddingBefore;
            if (mOrientation == Orientation.Horizontal)
            {
                longSize = Width;
                longPaddingBefore = PaddingLeft;
                longPaddingAfter = PaddingRight;
                shortPaddingBefore = PaddingTop;
            }
            else
            {
                longSize = Height;
                longPaddingBefore = PaddingTop;
                longPaddingAfter = PaddingBottom;
                shortPaddingBefore = PaddingLeft;
            }

            float threeRadius = mRadius * 3;
            float shortOffset = shortPaddingBefore + mRadius;
            float longOffset = longPaddingBefore + mRadius;
            if (mCentered)
            {
                longOffset += ((longSize - longPaddingBefore - longPaddingAfter) / 2.0f) - ((count * threeRadius) / 2.0f);
            }

            float dX;
            float dY;

            float pageFillRadius = mRadius;
            if (mPaintStroke.StrokeWidth > 0)
            {
                pageFillRadius -= mPaintStroke.StrokeWidth / 2.0f;
            }

            //Draw stroked circles
            for (int iLoop = 0; iLoop < count; iLoop++)
            {
                float drawLong = longOffset + (iLoop * threeRadius);
                if (mOrientation == Orientation.Horizontal)
                {
                    dX = drawLong;
                    dY = shortOffset;
                }
                else
                {
                    dX = shortOffset;
                    dY = drawLong;
                }
                // Only paint fill if not completely transparent
                if (mPaintPageFill.Alpha > 0)
                {
                    canvas.DrawCircle(dX, dY, pageFillRadius, mPaintPageFill);
                }

                // Only paint stroke if a stroke width was non-zero
                if (pageFillRadius != mRadius)
                {
                    canvas.DrawCircle(dX, dY, mRadius, mPaintStroke);
                }
            }

            //Draw the filled circle according to the current scroll
            float cx = (mSnap ? mSnapPage : mCurrentPage) * threeRadius;
            if (!mSnap)
            {
                cx += mPageOffset * threeRadius;
            }
            if (mOrientation == Orientation.Horizontal)
            {
                dX = longOffset + cx;
                dY = shortOffset;
            }
            else
            {
                dX = shortOffset;
                dY = longOffset + cx;
            }
            canvas.DrawCircle(dX, dY, mRadius, mPaintFill);
        }

        [Obsolete]
        public override bool OnTouchEvent(MotionEvent e)
        {
            if (base.OnTouchEvent(e)) return true;

            if ((mViewPager == null) || (mViewPager.Adapter.Count == 0))
            {
                return false;
            }

            var action = e.Action & (MotionEventActions)MotionEventCompat.ActionMask;
            switch (action)
            {
                case MotionEventActions.Down:
                    mActivePointerId = MotionEventCompat.GetPointerId(e, 0);
                    mLastMotionX = e.GetX();
                    break;

                case MotionEventActions.Move:
                    {
                        int activePointerIndex = MotionEventCompat.FindPointerIndex(e, mActivePointerId);
                        float x = MotionEventCompat.GetX(e, activePointerIndex);
                        float deltaX = x - mLastMotionX;

                        if (!mIsDragging)
                        {
                            if (Math.Abs(deltaX) > mTouchSlop)
                            {
                                mIsDragging = true;
                            }
                        }

                        if (mIsDragging)
                        {
                            mLastMotionX = x;
                            if (mViewPager.IsFakeDragging || mViewPager.BeginFakeDrag())
                            {
                                mViewPager.FakeDragBy(deltaX);
                            }
                        }

                        break;
                    }

                case MotionEventActions.Cancel:
                case MotionEventActions.Up:
                    if (!mIsDragging)
                    {
                        int count = mViewPager.Adapter.Count;
                        int width = Width;
                        float halfWidth = width / 2f;
                        float sixthWidth = width / 6f;

                        if ((mCurrentPage > 0) && (e.GetX() < halfWidth - sixthWidth))
                        {
                            if (action != MotionEventActions.Cancel)
                            {
                                mViewPager.CurrentItem = mCurrentPage - 1;
                            }
                            return true;
                        }
                        else if ((mCurrentPage < count - 1) && (e.GetX() > halfWidth + sixthWidth))
                        {
                            if (action != MotionEventActions.Cancel)
                            {
                                mViewPager.CurrentItem = mCurrentPage + 1;
                            }
                            return true;
                        }
                    }

                    mIsDragging = false;
                    mActivePointerId = INVALID_POINTER;
                    if (mViewPager.IsFakeDragging) 
                        mViewPager.EndFakeDrag();
                    break;

                case (MotionEventActions)MotionEventCompat.ActionPointerDown:
                    {
                        int index = MotionEventCompat.GetActionIndex(e);
                        mLastMotionX = MotionEventCompat.GetX(e, index);
                        mActivePointerId = MotionEventCompat.GetPointerId(e, index);
                        break;
                    }

                case (MotionEventActions)MotionEventCompat.ActionPointerUp:
                    int pointerIndex = MotionEventCompat.GetActionIndex(e);
                    int pointerId = MotionEventCompat.GetPointerId(e, pointerIndex);
                    if (pointerId == mActivePointerId)
                    {
                        int newPointerIndex = pointerIndex == 0 ? 1 : 0;
                        mActivePointerId = MotionEventCompat.GetPointerId(e, newPointerIndex);
                    }
                    mLastMotionX = MotionEventCompat.GetX(ev, MotionEventCompat.FindPointerIndex(e, mActivePointerId));
                    break;
            }

            return true;
        }

        public override void SetViewPager(ViewPager view)
        {
            if (mViewPager == view)
            {
                return;
            }
            if (mViewPager != null)
            {
                mViewPager.AddOnPageChangeListener(null);
            }
            if (view.Adapter == null)
            {
                throw new IllegalStateException("ViewPager does not have adapter instance.");
            }
            mViewPager = view;
            mViewPager.AddOnPageChangeListener((ViewPager.IOnPageChangeListener)this);
            Invalidate();
        }

        public override void SetViewPager(ViewPager view, int initialPosn)
        {
            SetViewPager(view);
            SetCurrentItem(initialPosn);
        }

        public override void SetCurrentItem(int item)
        {
            if (mViewPager == null)
                throw new IllegalStateException("ViewPager has not been bound.");

            mViewPager.CurrentItem = item;
            mCurrentPage = item;
            Invalidate();
        }

        public override void NotifyDataSetChanged()
        {
            Invalidate();
        }

        public override void OnPageScrollStateChanged(int state)
        {
            mScrollState = state;
            if (mListener != null)
                mListener.OnPageScrollStateChanged(state);
        }

        public override void OnPageScrolled(int posn, float offset, int posOffsetPixels)
        {
            mCurrentPage = posn;
            mPageOffset = offset;
            Invalidate();

            if (mListener != null)
                mListener.OnPageScrolled(posn, offset, posOffsetPixels);
        }

        public override void SetOnPageChangeListener(ViewPager.IOnPageChangeListener l)
        {
            mListener = l;
        }

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            if (mOrientation == Orientation.Horizontal)
            {
                SetMeasuredDimension(MeasureLong(widthMeasureSpec), MeasureShort(heightMeasureSpec));
            }
            else
            {
                SetMeasuredDimension(MeasureShort(widthMeasureSpec), MeasureLong(heightMeasureSpec));
            }
        }

        int MeasureLong(int spec)
        {
            int result;
            var specMode = MeasureSpec.GetMode(spec);
            int specSize = MeasureSpec.GetSize(spec);

            if ((specMode == MeasureSpecMode.Exactly || (mViewPager == null))
            {
                //We were told how big to be
                result = specSize;
            }
            else
            {
                //Calculate the width according the views count
                int count = mViewPager.Adapter.Count;
                result = (int)(PaddingLeft + PaddingRight
                        + (count * 2 * mRadius) + (count - 1) * mRadius + 1);
                //Respect AT_MOST value if that was what is called for by measureSpec
                if (specMode == MeasureSpecMode.AtMost)
                {
                    result = Math.Min(result, specSize);
                }
            }
            return result;
        }

        int MeasureShort(int spec)
        {
            int result;
            var specMode = MeasureSpec.GetMode(spec);
            int specSize = MeasureSpec.GetSize(spec);

            if (specMode == MeasureSpecMode.Exactly)
            {
                //We were told how big to be
                result = specSize;
            }
            else
            {
                //Measure the height
                result = (int)(2 * mRadius + PaddingTop + PaddingBottom + 1);
                //Respect AT_MOST value if that was what is called for by measureSpec
                if (specMode == MeasureSpecMode.AtMost)
                {
                    result = Math.Min(result, specSize);
                }
            }
            return result;
        }

        public override void OnRestoreInstanceState(Parcelable state)
        {
            var savedState = (SavedState)state;
            base.OnRestoreInstanceState(savedState.GetSuperState());
            mCurrentPage = savedState.CurrentPage;
            mSnapPage = savedState.currentPage;
            RequestLayout();
        }

        public override void OnSavedInstanceState()
        {
            Parcelable superState = (Parcelable)base.OnSaveInstanceState();
            SavedState savedState = new SavedState(superState);
            savedState.CurrentPage = mCurrentPage;
            return savedState;
        }

        public class SavedState : BaseSavedState
        {
            public int CurrentPage;
            
            public SavedState(Parcelable state) : base((IParcelable)state)
            {
            }

            public SavedState(Parcel inside) : base(inside)
            {
                CurrentPage = inside.ReadInt();
            }
        }
    }
}