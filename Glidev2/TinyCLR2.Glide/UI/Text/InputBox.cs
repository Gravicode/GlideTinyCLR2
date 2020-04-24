////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (c) GHI Electronics, LLC.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
//using Microsoft.SPOT;
//using Microsoft.SPOT.Presentation.Media;
using GHI.Glide.Display;
using GHI.Glide.Geom;
using System.Drawing;
using TinyCLR2.Glide.Properties;

namespace GHI.Glide.UI
{
    /// <summary>
    /// The InputBox is the base class for TextBox and PasswordBox.
    /// </summary>
    public class InputBox : DisplayObject
    {
        internal string text = String.Empty;
        internal bool pressed;
        internal int leftMargin = 5;
        internal Bitmap _TextBox = Resources.GetBitmap(Resources.BitmapResources.TextBox);

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

        internal string ShortenText(string str)
        {
            int maxWidth = Width - (leftMargin * 2);
            Size size = FontManager.GetSize(Font, str);

            string newstr = "";

            for (int i = 0; i < str.Length; i++)
            {
                size = FontManager.GetSize(Font, str.Substring(0, i + 1));
                if (size.Width <= maxWidth)
                    newstr = str.Substring(0, i + 1);
                else
                {
                    //newstr += "...";
                    break;
                }
            }

            return newstr;
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
                pressed = true;
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
                if (pressed)
                {
                    pressed = false;
                    Invalidate();
                    TriggerTapEvent(this);
                    e.StopPropagation();
                }
            }
            else
            {
                if (pressed)
                {
                    pressed = false;
                    Invalidate();
                }
            }
            return e;
        }

        /// <summary>
        /// A string containing the text.
        /// </summary>
        public string Text
        {
            get { return text; }
            set
            {
                text = value;
                TriggerValueChangedEvent(this);
            }
        }

        /// <summary>
        /// Indicates the text alignment.
        /// </summary>
        //public uint TextAlign { get; set; }
        public StringFormat TextAlign { get; set; }

        /// <summary>
        /// Font used by the text.
        /// </summary>
        public Font Font { get; set; }

        /// <summary>
        /// Indicates the font color.
        /// </summary>
        public Color FontColor { get; set; }
    }
}