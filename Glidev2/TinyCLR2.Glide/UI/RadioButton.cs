////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (c) GHI Electronics, LLC.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
//using Microsoft.SPOT;
//using Microsoft.SPOT.Presentation.Media;
using GHI.Glide.Display;
using System.Drawing;
using TinyCLR2.Glide.Properties;

namespace GHI.Glide.UI
{
    /// <summary>
    /// The RadioButton component lets a user to make a single selection from a set of choices.
    /// </summary>
    public class RadioButton : DisplayObject
    {
        private Bitmap _RadioButton = Resources.GetBitmap(Resources.BitmapResources.RadioButton);
        private bool _pressed = false;
        private bool _checked = false;
        private string _value = String.Empty;

        /// <summary>
        /// Creates a new RadioButton.
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="alpha">Alpha</param>
        /// <param name="x">X-axis position.</param>
        /// <param name="y">Y-axis position.</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        public RadioButton(string name, ushort alpha, int x, int y, int width, int height)
        {
            Name = name;
            Alpha = alpha;
            X = x;
            Y = y;
            Width = width;
            Height = height;

            // Default
            GroupName = String.Empty;
        }

        /// <summary>
        /// Renders the RadioButton onto it's parent container's graphics.
        /// </summary>
        public override void Render()
        {
            int x = Parent.X + X;
            int y = Parent.Y + Y;
            ushort alpha = (Enabled) ? Alpha : (ushort)(Alpha / 2);

            if (ShowBackground)
                Parent.Graphics.Scale9Image(x, y, Width, Height, _RadioButton, 5, alpha);

            x += (Width / 2) - 1;
            y += (Height / 2) - 1;

            int xRadius = Width / 3;
            int yRadius = Height / 3;

            if (_checked)
                Parent.Graphics.DrawEllipse(SelectedOutlineColor, 1, x, y, xRadius, yRadius, SelectedColor, 0, 0, TinyCLR2.Glide.Ext.Colors.Black, 0, 0, alpha);
            else
                Parent.Graphics.DrawEllipse(OutlineColor, 1, x, y, xRadius, yRadius, Color, 0, 0, TinyCLR2.Glide.Ext.Colors.Black, 0, 0, alpha);
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
            if (Rect.Contains(e.Point) && _pressed)
            {
                Toggle();
                e.StopPropagation();
            }

            return e;
        }

        /// <summary>
        /// Disposes all disposable objects in this object.
        /// </summary>
        public override void Dispose()
        {
            _RadioButton.Dispose();
        }

        /// <summary>
        /// Toggles the radio button between selected and not selected.
        /// </summary>
        public void Toggle()
        {
            int groupCount = RadioButtonManager.GetCount(GroupName);

            // Only allow change if this button is alone or in a group and not selected
            if (groupCount <= 1 || (groupCount > 1 && !Checked))
            {
                _pressed = false;
                Checked = !Checked;
                TriggerTapEvent(this);
            }
        }

        /// <summary>
        /// The group this radio button belongs to.
        /// </summary>
        public string GroupName;

        /// <summary>
        /// Value
        /// </summary>
        public string Value
        {
            get
            {
                if (_checked)
                    return _value;
                else
                    return String.Empty;
            }
            set { _value = value; }
        }

        /// <summary>
        /// Indicates whether or not this radio button is checked.
        /// </summary>
        /// <remarks>Changing this property will trigger an Invalidate.</remarks>
        public bool Checked
        {
            get { return _checked; }
            set
            {
                if (_checked != value)
                {
                    _checked = value;

                    if (Parent != null)
                        Invalidate();
                }
            }
        }

        /// <summary>
        /// Indicates whether or not to show the background box.
        /// </summary>
        public bool ShowBackground = true;

        /// <summary>
        /// Outline color.
        /// </summary>
        public Color OutlineColor = GlideUtils.Convert.ToColor("b8b8b8");

        /// <summary>
        /// Color
        /// </summary>
        public Color Color = GlideUtils.Convert.ToColor("d4d4d4");

        /// <summary>
        /// Selected outline color.
        /// </summary>
        public Color SelectedOutlineColor = GlideUtils.Convert.ToColor("002dff");

        /// <summary>
        /// Selected color.
        /// </summary>
        public Color SelectedColor = GlideUtils.Convert.ToColor("358bf6");

    }
}