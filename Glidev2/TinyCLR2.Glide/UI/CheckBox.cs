////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (c) GHI Electronics, LLC.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

//using Microsoft.SPOT;
using GHI.Glide.Display;
using System.Drawing;
using TinyCLR2.Glide.Properties;

namespace GHI.Glide.UI
{
    /// <summary>
    /// The CheckBox component contains a small box that can be check marked.
    /// </summary>
    public class CheckBox : DisplayObject
    {
        private Bitmap _CheckBox_On = Resources.GetBitmap(Resources.BitmapResources.CheckBox_On);
        private Bitmap _CheckBox_Off = Resources.GetBitmap(Resources.BitmapResources.CheckBox_Off);
        private bool _pressed = false;
        private bool _checked = false;

        /// <summary>
        /// Creates a new CheckBox.
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="alpha">Alpha</param>
        /// <param name="x">X-axis position.</param>
        /// <param name="y">Y-axis position.</param>
        public CheckBox(string name, ushort alpha, int x, int y)
        {
            Name = name;
            Alpha = alpha;
            X = x;
            Y = y;
            Width = _CheckBox_On.Width;
            Height = _CheckBox_On.Height;
        }

        /// <summary>
        /// Renders the CheckBox onto it's parent container's graphics.
        /// </summary>
        public override void Render()
        {
            int x = Parent.X + X;
            int y = Parent.Y + Y;

            if (_checked)
                Parent.Graphics.DrawImage(x, y, _CheckBox_On, 0, 0, Width, Height, Alpha);
            else
                Parent.Graphics.DrawImage(x, y, _CheckBox_Off, 0, 0, Width, Height, Alpha);
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
                _pressed = false;
                Checked = !Checked;
                TriggerTapEvent(this);
                e.StopPropagation();
            }
            return e;
        }

        /// <summary>
        /// Disposes all disposable objects in this object.
        /// </summary>
        public override void Dispose()
        {
            _CheckBox_On.Dispose();
            _CheckBox_Off.Dispose();
        }

        /// <summary>
        /// Indicates whether or not this checkbox is checked.
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
    }
}
