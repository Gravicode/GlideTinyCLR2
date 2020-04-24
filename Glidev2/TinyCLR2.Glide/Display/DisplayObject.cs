////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (c) GHI Electronics, LLC.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Threading;
////using Microsoft.SPOT;
using GHI.Glide.Geom;
using GHI.Glide.UI;

namespace GHI.Glide.Display
{
    /// <summary>
    /// The DisplayObject is the base class for all objects.
    /// </summary>
    public class DisplayObject
    {
        private int _width = 0;
        private int _height = 0;
        private Rectangle _rect = new Rectangle();

        /// <summary>
        /// Indicates the instance name of the DisplayObject.
        /// </summary>
        public string Name;

        /// <summary>
        /// Indicates the alpha transparency value of the object specified.
        /// </summary>
        /// <remarks>Valid values are 0 (fully transparent) and 255 (fully opaque). Default value is 255.</remarks>
        public ushort Alpha = 255;

        /// <summary>
        /// Indicates whether or not the DisplayObject is visible.
        /// </summary>
        /// <remarks>Invisible objects are not drawn nor do they receive touch events.</remarks>
        public bool Visible = true;

        /// <summary>
        /// Indicates whether or not the DisplayObject is enabled.
        /// </summary>
        /// <remarks>Disabled objects do not receive touch events.</remarks>
        public bool Enabled = true;

        /// <summary>
        /// Indicates whether or not the DisplayObject is interactive.
        /// </summary>
        /// <remarks>Non-interactive objects do not receive touch events. This allows disabled objects to keep their state.</remarks>
        public bool Interactive = true;

        /// <summary>
        /// Object that contains data about the display object.
        /// </summary>
        public object Tag = null;

        /// <summary>
        /// Indicates the DisplayObjectContainer object that contains this display object.
        /// </summary>
        public DisplayObjectContainer Parent;

        /// <summary>
        /// Tap event.
        /// </summary>
        public event OnTap TapEvent;

        /// <summary>
        /// Triggers a tap event.
        /// </summary>
        /// <param name="sender">Object associated with the event.</param>
        public void TriggerTapEvent(object sender)
        {
            if (TapEvent != null)
                TapEvent(sender);
        }

        /// <summary>
        /// Is TapEvent null?
        /// </summary>
        /// <returns>True if TapEvent is null; false otherwise.</returns>
        public bool TapEventIsNull()
        {
            return (TapEvent == null);
        }

        /// <summary>
        /// Gesture event.
        /// </summary>
        public event OnGesture GestureEvent;

        /// <summary>
        /// Triggers a gesture event.
        /// </summary>
        /// <param name="sender">Object associated with the event.</param>
        /// <param name="args">Touch gesture event arguments.</param>
        public void TriggerGestureEvent(object sender, TouchGestureEventArgs args)
        {
            if (GestureEvent != null)
                GestureEvent(sender, args);
        }

        /// <summary>
        /// Renders this display object.
        /// </summary>
        public virtual void Render()
        {
            // Do something
        }

        /// <summary>
        /// Renders this display object on a specified bitmap.
        /// </summary>
        /// <param name="bitmap"></param>
        public virtual void Render(Display.Graphics bitmap)
        {
            // Do something
        }

        /// <summary>
        /// Renders this display object and flushes it to the screen.
        /// </summary>
        public virtual void Invalidate()
        {
            Render();
            Glide.Flush(this);
        }

        /// <summary>
        /// Handles touch down events.
        /// </summary>
        /// <param name="e">Touch event arguments.</param>
        /// <returns>Touch event arguments.</returns>
        public virtual TouchEventArgs OnTouchDown(TouchEventArgs e)
        {
            return e;
        }

        /// <summary>
        /// Handles touch up events.
        /// </summary>
        /// <param name="e">Touch event arguments.</param>
        /// <returns>Touch event arguments.</returns>
        public virtual TouchEventArgs OnTouchUp(TouchEventArgs e)
        {
            return e;
        }

        /// <summary>
        /// Handles touch move events.
        /// </summary>
        /// <param name="e">Touch event arguments.</param>
        /// <returns>Touch event arguments.</returns>
        public virtual TouchEventArgs OnTouchMove(TouchEventArgs e)
        {
            return e;
        }

        /// <summary>
        /// Handles touch gesture events.
        /// </summary>
        /// <param name="e">Touch event arguments.</param>
        /// <returns>Touch event arguments.</returns>
        public virtual TouchGestureEventArgs OnTouchGesture(TouchGestureEventArgs e)
        {
            return e;
        }

        /// <summary>
        /// Disposes all disposable objects in this display object.
        /// </summary>
        public virtual void Dispose()
        {
            // Do something
        }

        /// <summary>
        /// Indicates the x coordinate of the DisplayObject instance relative to the local coordinates of the parent DisplayObjectContainer.
        /// </summary>
        public int X;

        /// <summary>
        /// Indicates the y coordinate of the DisplayObject instance relative to the local coordinates of the parent DisplayObjectContainer.
        /// </summary>
        public int Y;

        /// <summary>
        /// Indicates the width of the display object, in pixels.
        /// </summary>
        public virtual int Width
        {
            get { return _width; }
            set
            {
                _width = value;
                _rect.Width = value;
            }
        }

        /// <summary>
        /// Indicates the height of the display object, in pixels.
        /// </summary>
        public virtual int Height
        {
            get { return _height; }
            set
            {
                _height = value;
                _rect.Height = value;
            }
        }

        /// <summary>
        /// Indicates the rectangle bounds of this display object.
        /// </summary>
        public Rectangle Rect
        {
            get
            {
                _rect.X = X;
                if (Parent != null)
                    _rect.X += Parent.X;

                _rect.Y = Y;
                if (Parent != null)
                    _rect.Y += Parent.Y;

                return _rect;
            }
        }

    }
}
