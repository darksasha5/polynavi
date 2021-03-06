﻿using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Util;
using Android.Views;
using AndroidX.Core.Content;
using Polynavi.Droid.Fragments;

namespace Polynavi.Droid.Views
{
    public class MainBuildingView : View
    {
        public enum Marker
        {
            Start,
            End,
            None
        }

        private readonly Paint routePaint = new Paint() 
        { 
            Color = Color.Blue,
            StrokeCap = Paint.Cap.Round,
            StrokeWidth = 7.0f 
        };

        private readonly Paint startPointPaint = new Paint() { Color = Color.Green };
        private readonly Paint endPointPaint = new Paint() { Color = Color.Red };

        private float[] route;        
        private Marker marker = Marker.None; //TODO Заменить на рисунки
        private Point markerPoint;

        private const int InvalidPointerId = -1;

        private readonly Drawable plan;

        private int activePointerId = -1;
        private float lastTouchX, lastTouchY;
        public float PosX { get; set; }
        public float PosY { get; set; }

        private const float ScaleFactor = 1.0f;

        private readonly DisplayMetrics displayMetrics;

        private const int BaseWidth = 3200, BaseHeight = 1800;

        private readonly float widthScale, heightScale;
        private readonly int imageWidth, imageHeight;

        public MainBuildingView(Context context, int id) :
            base(context, null, 0)
        {
            startPointPaint.SetStyle(Paint.Style.Fill); //TODO
            displayMetrics = Resources.DisplayMetrics;

            plan = ContextCompat.GetDrawable(Context, id);

            imageWidth = (int)(displayMetrics.HeightPixels * 1.777778 * 0.85); //TODO
            imageHeight = (int)(displayMetrics.HeightPixels * 0.85); //TODO

            widthScale = (float)imageWidth / BaseWidth;
            heightScale = (float)imageHeight / BaseHeight;

            routePaint.StrokeWidth *= widthScale;

            plan.SetBounds(0, 0, imageWidth, imageHeight);
        }

        public override bool OnTouchEvent(MotionEvent e)
        {
            if (!MainBuildingFragment.CheckFocus())
            {
                Utils.Utils.HideKeyboard(this, Context);
            }

            var action = e.Action & MotionEventActions.Mask;
            int pointerIndex;

            switch (action)
            {
                case MotionEventActions.Down:
                    lastTouchX = e.GetX();
                    lastTouchY = e.GetY();
                    activePointerId = e.GetPointerId(0);
                    break;

                case MotionEventActions.Move:
                    pointerIndex = e.FindPointerIndex(activePointerId);
                    var x = e.GetX(pointerIndex);
                    var y = e.GetY(pointerIndex);

                    var deltaX = x - lastTouchX;
                    var deltaY = y - lastTouchY;
                    PosX += deltaX;
                    PosY += deltaY;

                    var planScaleWidth = imageWidth * ScaleFactor;
                    var planScaleHeight = imageHeight * ScaleFactor;

                    var right = PosX + planScaleWidth;
                    var left = PosX;
                    var top = PosY;
                    var bottom = PosY + planScaleHeight;

#if DEBUG
                    LogCurrentCoordinates(right, left, top, bottom); //TODO
#endif

                    if (right < displayMetrics.WidthPixels)
                    {
                        PosX -= deltaX;
                    }

                    if (left > 0)
                    {
                        PosX -= deltaX;
                    }

                    if (top > 0)
                    {
                        PosY -= deltaY;
                    }

                    if (bottom < imageHeight)
                    {
                        PosY -= deltaY;
                    }

                    Invalidate();

                    lastTouchX = x;
                    lastTouchY = y;
                    break;

                case MotionEventActions.Up:
                case MotionEventActions.Cancel:
                    activePointerId = InvalidPointerId;
                    break;

                case MotionEventActions.PointerUp:
                    pointerIndex = (int)(e.Action & MotionEventActions.PointerIndexMask) >> //TODO ?
                                   (int)MotionEventActions.PointerIndexShift;
                    var pointerId = e.GetPointerId(pointerIndex);
                    if (pointerId == activePointerId)
                    {
                        var newPointerIndex = pointerIndex == 0 ? 1 : 0;
                        lastTouchX = e.GetX(newPointerIndex);
                        lastTouchY = e.GetY(newPointerIndex);
                        activePointerId = e.GetPointerId(newPointerIndex);
                    }

                    break;
            }

            return true;
        }

        private void LogCurrentCoordinates(in float right, in float left, in float top, in float bottom)
        {
            Log.Debug("PLAN", "Right: " + right);
            Log.Debug("PLAN", "Left: " + left);
            Log.Debug("PLAN", "Top: " + top);
            Log.Debug("PLAN",
                "Bottom: " + bottom + " // IntristicHeight: " + plan.IntrinsicHeight + " // ImageHeight: " +
                imageHeight);
        }

        protected override void OnDraw(Canvas canvas)
        {
            base.OnDraw(canvas);
            canvas.Save();
            canvas.Translate(PosX, PosY);
            canvas.Scale(ScaleFactor, ScaleFactor);
            plan.Draw(canvas);

            if (route != null)
            {
                canvas.DrawLines(route, routePaint);
            }

            switch (marker)
            {
                case Marker.Start:
                    canvas.DrawCircle(markerPoint.X, markerPoint.Y, 10.0f * widthScale, startPointPaint);
                    break;
                case Marker.End:
                    canvas.DrawRect(markerPoint.X - 10.0f * widthScale, markerPoint.Y - 10.0f * heightScale,
                        markerPoint.X + 10.0f * widthScale, markerPoint.Y + 10.0f * heightScale, endPointPaint);
                    break;
                case Marker.None:
                    break;
            }

            canvas.Restore();
        }

        public void SetMarker(Point point, Marker marker)
        {
            this.marker = marker;

            point.X = (int)(point.X * widthScale);
            point.Y = (int)(point.Y * heightScale);

            markerPoint = point;
        }

        public void SetRoute(IList<Point> points) //TODO
        {
            var r = new List<float>();
            if (route != null)
            {
                r = route.ToList();
            }

            if (points == null)
            {
                route = new float[0];
            }
            else
            {
                var segmentsCount = points.Count - 1;

                route = new float[segmentsCount * 4];
                route[0] = points[0].X * widthScale;
                route[1] = points[0].Y * heightScale;

                int i, j;
                for (i = 1, j = 2; i < points.Count - 1; ++i, j += 4)
                {
                    route[j] = points[i].X * widthScale;
                    route[j + 1] = points[i].Y * heightScale;
                    route[j + 2] = points[i].X * widthScale;
                    route[j + 3] = points[i].Y * heightScale;
                }

                route[j] = points[i].X * widthScale;
                route[j + 1] = points[i].Y * heightScale;

                r.AddRange(route.ToList());
                route = new float[r.Count];
                route = r.ToArray();
            }
        }
    }
}
