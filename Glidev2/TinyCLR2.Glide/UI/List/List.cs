////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (c) GHI Electronics, LLC.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Threading;
//using Microsoft.SPOT;
using GHI.Glide.Display;
using GHI.Glide.Geom;
using System.Drawing;
using TinyCLR2.Glide.Ext;

namespace GHI.Glide.UI
{
    /// <summary>
    /// The list component provides a list of options.
    /// </summary>
    public class List : DisplayObjectContainer
    {
        private Display.Graphics _bitmap;
        private int _renderedWithNumChildren = 0;
        private int _lastPressY;
        private int _lastListY;
        private int _listY = 0;
        private int _listMaxY;
        private bool _pressed = false;
        private bool _moving = false;
        private int _ignoredTouchMoves = 0;
        private int _maxIgnoredTouchMoves = 1;

        /// <summary>
        /// Creates a new List component.
        /// </summary>
        /// <param name="options">Array of options.</param>
        /// <param name="width">Width</param>
        /// <remarks>The list cannot be smaller than 100 or greater than the LCD size. We recommend keeping the size to a minimum; only use what you need.</remarks>
        public List(ArrayList options, int width)
        {
            object[] option;
            ListItem item;

            for (int i = 0; i < options.Count; i++)
            {
                option = (object[])options[i];
                item = new ListItem(option[0].ToString(), option[1]);
                item.Y = i * item.Height;
                AddChild(item);
            }

            int itemHeight = this[0].Height;
            int numItems = (int)(System.Math.Floor(Glide.LCD.Height / itemHeight)) - 1;

            Name = "list";
            if (width < 100)
                width = 100;
            else if (width > Glide.LCD.Width)
                width = Glide.LCD.Width;
            Width = width;
            Height = numItems * itemHeight;
            X = (Glide.LCD.Width - Width) / 2;
            Y = (Glide.LCD.Height - Height) / 2;
        }

        /// <summary>
        /// Tap option event.
        /// </summary>
        public event OnTapOption TapOptionEvent;

        /// <summary>
        /// Triggers a tap option event.
        /// </summary>
        /// <param name="sender">Object associated with the event.</param>
        /// <param name="args">Tap option event arguments.</param>
        internal virtual void TriggerTapOptionEvent(object sender, TapOptionEventArgs args)
        {
            if (TapOptionEvent != null)
                TapOptionEvent(sender, args);
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
        /// Renders the List onto it's parent container's graphics.
        /// </summary>
        public override void Render()
        {
            // Only render the child bitmap if children change
            if (NumChildren > 0 && _renderedWithNumChildren < NumChildren)
            {
                _renderedWithNumChildren = NumChildren;

                _bitmap = new Display.Graphics(Width, NumChildren * this[0].Height);
                _bitmap.DrawRectangle(TinyCLR2.Glide.Ext.Colors.White,1, 0, 0, _bitmap.GetBitmap().Width, _bitmap.GetBitmap().Height, 0, 0, TinyCLR2.Glide.Ext.Colors.Transparent, 0, 0, TinyCLR2.Glide.Ext.Colors.Transparent, 0, 0, Alpha);

                for (int i = 0; i < NumChildren; i++)
                    ((ListItem)this[i]).Render(_bitmap);

                _listMaxY = _bitmap.GetBitmap().Height - Height;
            }

            Graphics.DrawRectangle(TinyCLR2.Glide.Ext.Colors.Black, 0, 0, 0, Glide.LCD.Width, Glide.LCD.Height, 0, 0, TinyCLR2.Glide.Ext.Colors.Black, 0, 0, TinyCLR2.Glide.Ext.Colors.Black, 0, 0, 0xff);
            Graphics.DrawImage(X, Y, _bitmap.GetBitmap(), 0, _listY, Width, Height);
        }

        /// <summary>
        /// Handles the touch down event.
        /// </summary>
        /// <param name="e">Touch event arguments.</param>
        /// <returns>Touch event arguments.</returns>
        public override TouchEventArgs OnTouchDown(TouchEventArgs e)
        {
            if (Rect.Contains(e.Point))
            {
                e.StopPropagation();
                _lastPressY = e.Point.Y;
                _lastListY = _listY;
                _pressed = true;
            }

            return e;
        }

        /// <summary>
        /// Handles the touch up event.
        /// </summary>
        /// <param name="e">Touch event arguments.</param>
        /// <returns>Touch event arguments.</returns>
        public override TouchEventArgs OnTouchUp(TouchEventArgs e)
        {
            if (!_pressed)
            { 
                TriggerCloseEvent(this);
                _ignoredTouchMoves = 0;
                return e;
            }

            if (!_moving)
            {
                int index = (int)System.Math.Floor(((_listY + e.Point.Y) - Y) / this[0].Height);
                if (index >= 0 && index < this.NumChildren) 
                {
                    ListItem option = (ListItem)this[index];
                    TriggerTapOptionEvent(this, new TapOptionEventArgs(index, option.Label, option.Value));
                }
            }
            else
                _moving = false;

            _pressed = false;
            _ignoredTouchMoves = 0;
            e.StopPropagation();
            return e;
        }

        /// <summary>
        /// Handles the touch move event.
        /// </summary>
        /// <param name="e">Touch event arguments.</param>
        /// <returns>Touch event arguments.</returns>
        public override TouchEventArgs OnTouchMove(TouchEventArgs e)
        {
            // The pressed state only triggers when touches occur within this UI element's boundaries.
            // This check guarantees whether we need to process move events or not.
            if (!_pressed)
                return e;

            if (!_moving)
            {
                if (_ignoredTouchMoves < _maxIgnoredTouchMoves)
                    _ignoredTouchMoves++;
                else
                    _moving = true;
            }
            else
            {
                int dragDistance = e.Point.Y - _lastPressY;
                _listY = _lastListY - dragDistance;
                _listY = GlideUtils.Math.MinMax(_listY, 0, _listMaxY);

                Graphics.DrawImage(X, Y, _bitmap.GetBitmap(), 0, _listY, Width, Height);
                Glide.Flush(this);
            }

            e.StopPropagation();
            return e;
        }

        /// <summary>
        /// Disposes all disposable objects in this object.
        /// </summary>
        public override void  Dispose()
        {
            if (_bitmap != null)
            {
                _bitmap.Dispose();
                _bitmap = null;
            }
        }
    }
}
