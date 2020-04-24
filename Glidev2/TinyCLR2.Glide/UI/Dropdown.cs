////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (c) GHI Electronics, LLC.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
//using Microsoft.SPOT;
//using Microsoft.SPOT.Presentation.Media;
using GHI.Glide.Display;
using GHI.Glide.Geom;
using System.Drawing;
using TinyCLR2.Glide.Properties;
using TinyCLR2.Glide.Ext;

namespace GHI.Glide.UI
{
    /// <summary>
    /// THe Dropdown component contains a list of options in which a user can select one.
    /// </summary>
    public class Dropdown : DisplayObject
    {
        private Bitmap _DropdownText_Up = Resources.GetBitmap(Resources.BitmapResources.DropdownText_Up);
        private Bitmap _DropdownText_Down = Resources.GetBitmap(Resources.BitmapResources.DropdownText_Down);
        private Bitmap _DropdownButton_Up = Resources.GetBitmap(Resources.BitmapResources.DropdownButton_Up);
        private Bitmap _DropdownButton_Down = Resources.GetBitmap(Resources.BitmapResources.DropdownButton_Down);
        private bool _pressed = false;
        private int _leftMargin = 10;

        /// <summary>
        /// Creates a new Dropdown.
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="alpha">Alpha</param>
        /// <param name="x">X-axis position.</param>
        /// <param name="y">Y-axis position.</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        public Dropdown(string name, ushort alpha, int x, int y, int width, int height)
        {
            Name = name;
            Alpha = alpha;
            X = x;
            Y = y;
            Width = width;
            Height = height;

            // Default
            Options = new ArrayList();
            Value = null;
            Text = String.Empty;
            Font = FontManager.GetFont(FontManager.FontType.droid_reg12);
            FontColor = TinyCLR2.Glide.Ext.Colors.Black;
        }

        /// <summary>
        /// Value changed event.
        /// </summary>
        public event OnValueChanged ValueChangedEvent;

        /// <summary>
        /// Triggers a value changed event.
        /// </summary>
        /// <param name="sender">Object associated with the event.</param>
        public void TriggerValueChangedEvent(object sender)
        {
            if (ValueChangedEvent != null)
                ValueChangedEvent(sender);
        }

        /// <summary>
        /// Renders the Dropdown onto it's parent container's graphics.
        /// </summary>
        public override void Render()
        {
            int x = Parent.X + X;
            int y = Parent.Y + Y;
            int textY = y + ((Height - Font.Height) / 2);
            ushort alpha = (Enabled) ? Alpha : (ushort)(Alpha / 3);

            if (_pressed)
            {
                textY++;
                Parent.Graphics.Scale9Image(x, y, Width, Height, _DropdownText_Down, 5, 5, 5, 5, alpha);
                Parent.Graphics.DrawImage((x + Width) - _DropdownButton_Down.Width, y, _DropdownButton_Down, 0, 0, _DropdownButton_Down.Width, _DropdownButton_Down.Height, Alpha);
            }
            else
            {
                Parent.Graphics.Scale9Image(x, y, Width, Height, _DropdownText_Up, 5, 5, 5, 5, alpha);
                Parent.Graphics.DrawImage((x + Width) - _DropdownButton_Up.Width, y, _DropdownButton_Up, 0, 0, _DropdownButton_Up.Width, _DropdownButton_Up.Height, alpha);
            }
            //Bitmaps.DT_AlignmentLeft
            Parent.Graphics.DrawTextInRect(Text, x + _leftMargin, textY, Width, Height,new StringFormat() { Alignment = StringAlignment.Near }, FontColor, Font);
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
                _pressed = true;
                Invalidate();
                e.StopPropagation();
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
            if (Rect.Contains(e.Point))
            {
                if (_pressed)
                {
                    _pressed = false;
                    Invalidate();
                    TriggerTapEvent(this);
                    e.StopPropagation();
                }
            }
            else
            {
                if (_pressed)
                {
                    _pressed = false;
                    Invalidate();
                }
            }
            return e;
        }

        /// <summary>
        /// Disposes all disposable objects in this object.
        /// </summary>
        public override void Dispose()
        {
            _DropdownText_Up.Dispose();
            _DropdownText_Down.Dispose();
            _DropdownButton_Up.Dispose();
            _DropdownButton_Down.Dispose();
        }

        /// <summary>
        /// An array of objects that represent the options.
        /// </summary>
        public ArrayList Options { get; set; }

        /// <summary>
        /// Value of the selected option.
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// Text
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Font used by the text.
        /// </summary>
        public Font Font { get; set; }

        /// <summary>
        /// Font color.
        /// </summary>
        public Color FontColor { get; set; }
    }
}