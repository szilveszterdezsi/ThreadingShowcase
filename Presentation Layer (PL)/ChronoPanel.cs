/// ---------------------------
/// Author: Szilveszter Dezsi
/// Created: 2019-11-04
/// Modified: n/a
/// ---------------------------

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PL
{
    /// <summary>
    /// Panel class that draws an analog timepeice that can be configured as a clock (chronometer) or stop watch (chronograph).
    /// Face and hands are uniformly scaled within the area defined by width and height of panel (whichever is lesser).
    /// Inherits behavior from Canvas and Panel classes.
    /// </summary>
    public class ChronoPanel : Canvas
    {
        private Point center;
        private Point scale;
        private ChronoType ct;
        private TickType tt;
        private Brush bg;
        private Brush fg;
        private Rectangle bigHand;
        private Rectangle smallHand;
        private Rectangle tinyHand;
        private double bigHandAngle;
        private double smallHandAngle;
        private double tinyHandAngle;

        /// <summary>
        /// Constructor that configures timepeice and tick type as well as background and foreground colors.
        /// </summary>
        /// <param name="clockType">ChronoType: Meter or Graph.</param>
        /// <param name="tickType">TickType: Soft or Hard.</param>
        /// <param name="background">Background brush.</param>
        /// <param name="foreground">Foreground brush.</param>
        public ChronoPanel(ChronoType clockType, TickType tickType, Brush background, Brush foreground)
        {
            ct = clockType;
            tt = tickType;
            bg = background;
            fg = foreground;
            bigHand = new Rectangle() { Fill = fg };
            smallHand = new Rectangle() { Fill = fg };
            tinyHand = new Rectangle() { Fill = fg };
            bigHandAngle = 0;
            smallHandAngle = 0;
            tinyHandAngle = 0;
            Children.Add(bigHand);
            Children.Add(smallHand);
            Children.Add(tinyHand);
        }

        /// <summary>
        /// Override of panel object method that draws the content of the drawing context object.
        /// Purpose of override is to obtain actual dimention of the panel after it has been rendered.
        /// Face and hands are uniformly scaled within the area defined by width and height of panel (whichever is lesser).
        /// </summary>
        /// <param name="dc">Drawing context.</param>
        protected override void OnRender(DrawingContext dc)
        {
            center = new Point(ActualWidth / 2, ActualHeight / 2);
            scale = ActualWidth > ActualHeight ? new Point(ActualHeight / 2, ActualHeight / 2) : new Point(ActualWidth / 2, ActualWidth / 2);
            dc.DrawEllipse(bg, new Pen(fg, scale.X * 0.01), center, scale.X * 0.95, scale.Y * 0.95);
            dc.DrawEllipse(bg, new Pen(fg, scale.X * 0.01), center, scale.X * 0.90, scale.Y * 0.90);
            dc.DrawEllipse(fg, new Pen(fg, scale.X * 0.01), center, scale.X * 0.03, scale.Y * 0.03);
            for (int i = -78; i <= 276; i += 6)
            {
                double x = scale.X * Math.Cos(i * Math.PI / 180);
                double y = scale.Y * Math.Sin(i * Math.PI / 180);
                dc.DrawLine(new Pen(fg, i % 30 == 0 ? scale.X * 0.02 : scale.X * 0.01), new Point(center.X + (x * 0.95), center.Y + (y * 0.95)), new Point(center.X + (x * 0.90), center.Y + (y * 0.90)));
                if (i % 30 == 0)
                {
                    FormattedText formattedText = new FormattedText((ct == ChronoType.Meter ? (i / 30) + 3 : (i / 6) + 15).ToString(), CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Arial"), scale.X > 0 ? scale.X*0.18 : 0.00001, fg, 1.5);
                    Point textLocation = new Point((x * 0.75) + center.X - formattedText.WidthIncludingTrailingWhitespace / 2, (y * 0.75) + center.Y - formattedText.Height/2);
                    dc.DrawText(formattedText, textLocation);
                }
            }
            bigHand.Margin = new Thickness(center.X - scale.X * 0.03 / 2, center.Y - scale.Y * 0.60, 0, 0);
            bigHand.Width = scale.X * 0.03;
            bigHand.Height = scale.Y * 0.75;
            smallHand.Margin = new Thickness(center.X - scale.X * 0.02 / 2, center.Y - scale.Y * 0.85, 0, 0);
            smallHand.Width = scale.X * 0.02;
            smallHand.Height = scale.Y;
            tinyHand.Margin = new Thickness(center.X - scale.X * 0.01 / 2, center.Y - scale.Y * 0.85, 0, 0);
            tinyHand.Width = scale.X * 0.01;
            tinyHand.Height = scale.Y;
            UpdateHands();
        }

        /// <summary>
        /// Updates/refreshes current time when configured as clock (chronometer).
        /// Designed to be used with DateTime-object.
        /// </summary>
        /// <param name="hour">Current hour (DateTime.Now.Hour)</param>
        /// <param name="min">Current minute (DateTime.Now.Minute)</param>
        /// <param name="sec">Current second (DateTime.Now.Second).</param>
        /// <param name="ms">Current millisecond (DateTime.Now.Millisecond)</param>
        public void ChronometerTick(double hour, double min, double sec, double ms)
        {
            if (ct == ChronoType.Meter)
            {
                tinyHandAngle = (sec + (tt == TickType.Soft ? (ms / 1000) : 0)) * 6;
                smallHandAngle = (min + (tt == TickType.Soft ? (sec / 60) : 0)) * 6;
                bigHandAngle = (hour + (tt == TickType.Soft ? (min / 60) : 0)) * 30;
                UpdateHands();
            }
        }

        /// <summary>
        /// Updates/refreshes elapsed time when configured as stop watch (chronograph).
        /// Designed to be used with StopWatch-object.
        /// </summary>
        /// <param name="min">Total minutes (TimeSpan.TotalMinutes of StopWatch.Elsapsed)</param>
        /// <param name="sec">Total seconds (TimeSpan.TotalSeconds of StopWatch.Elsapsed)</param>
        public void ChonographTick(double min, double sec)
        {
            if (ct == ChronoType.Graph)
            {
                bigHandAngle = (tt == TickType.Soft ? min : (int)min) * 6 % 360;
                smallHandAngle = (tt == TickType.Soft ? sec : (int)sec) * 6 % 360;
                tinyHandAngle = sec * 360 % 360;
                UpdateHands();
            }
        }

        /// <summary>
        /// Updates/refreshes hand angles.
        /// </summary>
        public void UpdateHands()
        {
            bigHand.RenderTransform = new RotateTransform(bigHandAngle, scale.X * 0.03 / 2, scale.Y * 0.60);
            smallHand.RenderTransform = new RotateTransform(smallHandAngle, scale.X * 0.02 / 2, scale.Y * 0.85);
            tinyHand.RenderTransform = new RotateTransform(tinyHandAngle, scale.X * 0.01 / 2, scale.Y * 0.85);
        }

        /// <summary>
        /// Sets tick type.
        /// </summary>
        /// <param name="tickType">TickType: Soft or Hard.</param>
        public void SetTickType(TickType tickType)
        {
            tt = tickType;
        }

        /// <summary>
        /// Timepiece types.
        /// </summary>
        public enum ChronoType
        {
            Meter,
            Graph
        }

        /// <summary>
        /// Tick types.
        /// </summary>
        public enum TickType
        {
            Hard,
            Soft
        }
    }
}
