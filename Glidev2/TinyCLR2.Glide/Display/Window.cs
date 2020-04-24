////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (c) GHI Electronics, LLC.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

////using Microsoft.SPOT;
////using Microsoft.SPOT.Presentation.Media;
using System.Drawing;
using Color = System.Drawing.Color;

namespace GHI.Glide.Display
{
    /// <summary>
    /// The Window class inherits from: DisplayObjectContainer, DisplayObject
    /// </summary>
    /// <remarks>The Window represents a single screen within an application.</remarks>
    public class Window : DisplayObjectContainer
    {
        private bool _handleEvents = false;
        private bool _moving = false;
        private int _ignoredTouchMoves = 0;
        private int _maxIgnoredTouchMoves = 1;
        private int _lastPressY;
        private int _lastListY;
        private int _listMaxY;

        /// <summary>
        /// Creates a new Window.
        /// </summary>
        /// <remarks>If you're using this empty constructor (to extend for example), you'll need to initialize the window's graphics object yourself. <code>myWindow.Graphics = new Graphics(width, height);</code></remarks>
        public Window() { }

        /// <summary>
        /// Value changed event.
        /// </summary>
        public event OnRendered RenderedEvent;

        /// <summary>
        /// Triggers a rendered event.
        /// </summary>
        /// <param name="sender">Object associated with the event.</param>
        public void TriggerOnRenderedEvent(object sender)
        {
            if (RenderedEvent != null)
                RenderedEvent(sender);
        }

        /// <summary>
        /// Creates a new Window.
        /// </summary>
        /// <param name="name">Name of the Window.</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        public Window(string name, int width, int height)
        {
            Name = name;
            X = 0;
            Y = 0;
            Width = width;
            Height = height;

            Graphics = new Graphics(width, height);

            ListY = 0;
            AutoHeight = true;
        }

        /// <summary>
        /// Close event.
        /// </summary>
        public event OnClose CloseEvent;

        /// <summary>
        /// Triggers a close event.
        /// </summary>
        /// <param name="sender">Object associated with the event.</param>
        public void TriggerCloseEvent(object sender)
        {
            if (CloseEvent != null)
                CloseEvent(sender);
        }

        /// <summary>
        /// Tells this window to handle touch events.
        /// </summary>
        public void HandleEvents()
        {
            _handleEvents = true;
            GlideTouch.TouchDownEvent += new TouchEventHandler(TouchDownEvent);
            GlideTouch.TouchUpEvent += new TouchEventHandler(TouchUpEvent);
            GlideTouch.TouchMoveEvent += new TouchEventHandler(TouchMoveEvent);
            GlideTouch.TouchGestureEvent += new TouchGestureEventHandler(TouchGestureEvent);
        }

        /// <summary>
        /// Tells this window to ignore touch events.
        /// </summary>
        public void IgnoreEvents()
        {
            _handleEvents = false;
            GlideTouch.TouchDownEvent -= new TouchEventHandler(TouchDownEvent);
            GlideTouch.TouchUpEvent -= new TouchEventHandler(TouchUpEvent);
            GlideTouch.TouchMoveEvent -= new TouchEventHandler(TouchMoveEvent);
            GlideTouch.TouchGestureEvent -= new TouchGestureEventHandler(TouchGestureEvent);

            _moving = false;
            _ignoredTouchMoves = 0;
        }

        /// <summary>
        /// Draws this window and it's children on it's graphics.
        /// </summary>
        public override void Render()
        {
            // HACK: To fix image/color retention.
            Graphics.DrawRectangle(Rect, TinyCLR2.Glide.Ext.Colors.Black, 255);

            if (BackImage != null)
                Graphics.DrawImage(0, 0, BackImage, 0, 0, Width, Height);
            else
                Graphics.DrawRectangle(Rect, BackColor, Alpha);

            base.Render();
            TriggerOnRenderedEvent(this);
        }

        /// <summary>
        /// Renders the window and flushes it to the screen.
        /// </summary>
        // Since windows need to flush we must override the default invalidate.
        public override void Invalidate()
        {
            Render();
            Glide.screen.DrawImage(Graphics.GetBitmap(), new System.Drawing.Rectangle(X, Y, Width, Height), new System.Drawing.Rectangle(0, ListY, Width, Height), GraphicsUnit.Pixel);

            //Glide.screen.DrawImage(X, Y, Graphics.GetBitmap(), 0, ListY, Width, Height, 0xff);
            Glide.screen.Flush();
            //Glide.screen.Flush(0,0,Glide.screen.Width,Glide.screen.Height);
        }

        /// <summary>
        /// Disposes this container and all it's children.
        /// </summary>
        public override void Dispose()
        {
            if (BackImage != null)
                BackImage.Dispose();

            base.Dispose();
        }

        /// <summary>
        /// Pass touch down events to the children.
        /// </summary>
        /// <param name="sender">Object associated with the event.</param>
        /// <param name="e">Touch event arguments.</param>
        private void TouchDownEvent(object sender, TouchEventArgs e)
        {
            // Make sure this window is handling events.
            if (_handleEvents)
            {
                _lastPressY = e.Point.Y;
                _lastListY = ListY;

                // Adjust the point with the scroll offset
                e.Point.Y += ListY;

                OnTouchDown(e);
            }
        }

        /// <summary>
        /// Pass touch up events to the children.
        /// </summary>
        /// <param name="sender">Object associated with the event.</param>
        /// <param name="e">Touch event arguments.</param>
        private void TouchUpEvent(object sender, TouchEventArgs e)
        {
            if (_handleEvents)
            {
                _moving = false;

                e.Point.Y += ListY;

                OnTouchUp(e);
            }
        }

        /// <summary>
        /// Pass touch move events to the children.
        /// </summary>
        /// <param name="sender">Object associated with the event.</param>
        /// <param name="e">Touch event arguments.</param>
        private void TouchMoveEvent(object sender, TouchEventArgs e)
        {
            if (_handleEvents)
            {
                OnTouchMove(e);

                if (!_moving)
                {
                    if (_ignoredTouchMoves < _maxIgnoredTouchMoves)
                        _ignoredTouchMoves++;
                    else
                    {
                        _ignoredTouchMoves = 0;
                        _moving = true;
                    }
                }
                else
                {
                    int dragDistance = e.Point.Y - _lastPressY;
                    ListY = _lastListY - dragDistance;

                    if (Height > Glide.LCD.Height)
                        _listMaxY = Height - Glide.LCD.Height;

                    ListY = GlideUtils.Math.MinMax(ListY, 0, _listMaxY);

                    Glide.screen.DrawImage(Graphics.GetBitmap(),new System.Drawing.Rectangle( X, Y, Width,Height),new System.Drawing.Rectangle( 0, ListY, Width, Height),GraphicsUnit.Pixel);
                    //Glide.screen.DrawImage(X, Y, Graphics.GetBitmap(), 0, ListY, Width, Height, 0xff);
                    Glide.screen.Flush();
                    //Glide.screen.Flush(0, 0, Glide.screen.Width, Glide.screen.Height);
                }
            }
        }

        /// <summary>
        /// Pass touch gesture events to the children.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TouchGestureEvent(object sender, TouchGestureEventArgs e)
        {
            if (_handleEvents)
                OnTouchGesture(e);
        }

        /// <summary>
        /// Fills a rectangle on this window's graphics using it's background image (if it exists) or the background color.
        /// </summary>
        /// <param name="rect">Rectangle object.</param>
        public void FillRect(Geom.Rectangle rect)
        {
            if (BackImage != null)
                Graphics.DrawImage(rect.X, rect.Y, BackImage, rect.X, rect.Y, rect.Width, rect.Height, Alpha);
            else
                Graphics.DrawRectangle(Color.Black, 0, rect.X, rect.Y, rect.Width, rect.Height, 0, 0, BackColor, 0, 0, BackColor, 0, 0, Alpha);
        }

        /// <summary>
        /// Background color.
        /// </summary>
        public Color BackColor { get; set; }

        /// <summary>
        /// Background image.
        /// </summary>
        public Bitmap BackImage { get; set; }

        /// <summary>
        /// List Y-axis position.
        /// </summary>
        public int ListY { get; set; }
    }
}