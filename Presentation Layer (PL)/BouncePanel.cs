/// ---------------------------
/// Author: Szilveszter Dezsi
/// Created: 2019-11-04
/// Modified: n/a
/// ---------------------------

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PL
{
    /// <summary>
    /// Panel class that defines an area where nested bouncer class objects can be added as children.
    /// Bouncers are animated and bounce within the area defined by width and height of panel.
    /// Inherits behavior from Canvas and Panel classes.
    /// </summary>
    public class BouncePanel : Canvas
    {
        private List<Bouncer> bouncers;
        private Point max;

        /// <summary>
        /// Constructor that initalizes handler for adding new bouncers by left mouse click.
        /// Also initalizes list for storing bouncers as well as adds an inital bouncer.
        /// </summary>
        public BouncePanel()
        {
            MouseLeftButtonDown += Click;
            bouncers = new List<Bouncer>();
            Add(0,0);
        }

        /// <summary>
        /// Override of panel object method that draws the content of the drawing context object.
        /// Purpose of override is to obtain actual dimention of the panel after it has been rendered.
        /// A transparent rectangle is alos drawn on the background in order to be able to detect mouse click events.
        /// </summary>
        /// <param name="dc">Drawing context.</param>
        protected override void OnRender(DrawingContext dc)
        {
            max = new Point(ActualWidth, ActualHeight);
            dc.DrawRectangle(Brushes.Transparent, null, new Rect(new Size(ActualWidth, ActualHeight)));
        }

        /// <summary>
        /// Increments each bouncer object in bouncer list.
        /// Detects when a bouncer object coordinates reach borders of the area defined by
        /// width and height of panel and inverts delta x and/or y (speed).
        /// </summary>
        public void Step()
        {
            if (bouncers.Count > 0)
            {
                bouncers.ForEach(b =>
                {
                    b.X += b.dX * 5;
                    b.Y += b.dY * 5;
                    b.g.Margin = new Thickness(b.X, b.Y, 0, 0);
                    if (b.X >= max.X - b.g.Width)  { b.X = max.X - b.g.Width;  b.dX = -b.dX; }
                    if (b.Y >= max.Y - b.g.Height) { b.Y = max.Y - b.g.Height; b.dY = -b.dY; }
                    if (b.X <= 0) { b.X = 0; b.dX = -b.dX; }
                    if (b.Y <= 0) { b.Y = 0; b.dY = -b.dY; }
                }); 
            }
        }

        /// <summary>
        /// Detects a mouse click event and adds a new bouncer object to the panel at click coordinates.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Click(object sender, MouseButtonEventArgs e)
        {
            Add(e.GetPosition(sender as IInputElement).X, e.GetPosition(sender as IInputElement).Y);
        }

        /// <summary>
        /// Adds a new bouncer object to the panel at parameter coordinates with random delta x and y (speed).
        /// </summary>
        /// <param name="x">x-coordinate.</param>
        /// <param name="y">y-coordinate.</param>
        public void Add(double x, double y)
        {
            Random random = new Random();
            Bouncer bouncer = new Bouncer(x, y, (random.NextDouble() * 2) - 1, (random.NextDouble() * 2) - 1);
            bouncer.g.Margin = new Thickness(bouncer.X, bouncer.Y, 0, 0);
            bouncers.Add(bouncer);
            Children.Add(bouncer.g);
        }
    }

    /// <summary>
    /// Nested class that handles a bouncer object.
    /// </summary>
    public class Bouncer
    {
        public double X;
        public double Y;
        public double dX;
        public double dY;
        public Ellipse g;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="x">x-coordinate.</param>
        /// <param name="y">y-coordinate.</param>
        /// <param name="dx">delta x (x-speed)</param>
        /// <param name="dy">delta y (y-speed).</param>
        public Bouncer(double x, double y, double dx, double dy)
        {
            X = x;
            Y = y;
            dX = dx;
            dY = dy;
            g = new Ellipse() { Fill = Brushes.Black, Width = 10, Height = 10 };
        }
    }
}
