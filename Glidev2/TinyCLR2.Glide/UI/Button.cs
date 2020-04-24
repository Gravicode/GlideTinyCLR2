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
using TinyCLR2.Glide.Ext;

namespace GHI.Glide.UI
{
    /// <summary>
    /// The Button component represents a rectangular button.
    /// </summary>
    public class Button : DisplayObject
    {
        private bool _pressed = false;

        /// <summary>
        /// Creates a new Button component.
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="alpha">Alpha</param>
        /// <param name="x">X-axis position.</param>
        /// <param name="y">Y-axis position.</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        public Button(string name, ushort alpha, int x, int y, int width, int height)
        {
            Name = name;
            Alpha = alpha;
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        /// <summary>
        /// Press event.
        /// </summary>
        public event OnPress PressEvent;

        /// <summary>
        /// Triggers a press event.
        /// </summary>
        /// <param name="sender">Object associated with this event.</param>
        public void TriggerPressEvent(object sender)
        {
            if (PressEvent != null)
                PressEvent(sender);
        }

        /// <summary>
        /// Release event.
        /// </summary>
        public event OnPress ReleaseEvent;

        /// <summary>
        /// Triggers a release event.
        /// </summary>
        /// <param name="sender">Object associated with this event.</param>
        public void TriggerReleaseEvent(object sender)
        {
            if (ReleaseEvent != null)
                ReleaseEvent(sender);
        }

        /// <summary>
        /// Renders the Button onto it's parent container's graphics.
        /// </summary>
        public override void Render()
        {
            int x = Parent.X + X;
            int y = Parent.Y + Y;

            int numLines = 1;
            int i = Text.IndexOf("\n");
            while (i > -1)
            {
                numLines++;
                i = Text.Substring(i + 2, Text.Length - (i + 2)).IndexOf("\n");
            }

            int textHeight = Font.Height * numLines;
            int textY = y + ((Height - textHeight) / 2);

            ushort alpha = (Enabled) ? Alpha : (ushort)(Alpha / 2);
            Color color = (Enabled) ? FontColor : TinyCLR2.Glide.Ext.Colors.LightGray;

            if (TintAmount > 0)
            {
                Parent.Graphics.DrawLine(TintColor, 1, x + 2, y, (x + Width) - 3, y);
                Parent.Graphics.DrawLine(TintColor, 1, x + 1, y + 1, (x + Width) - 2, y + 1);
                Parent.Graphics.DrawRectangle(TinyCLR2.Glide.Ext.Colors.Black, 0, x, y + 2, Width, Height - 4, 0, 0, TintColor, 0, 0, TinyCLR2.Glide.Ext.Colors.Black, 0, 0, alpha);
                //Parent.Graphics.DrawRectangle(TinyCLR2.Glide.Ext.Colors.Black, 0, x, y + 2, Width, Height - 4, 0, 0, TintColor, 0, 0, TinyCLR2.Glide.Ext.Colors.Black, 0, 0, alpha);
                Parent.Graphics.DrawLine(TintColor, 1, x + 1, (y + Height) - 2, (x + Width) - 2, (y + Height) - 2);
                Parent.Graphics.DrawLine(TintColor, 1, x + 2, (y + Height) - 1, (x + Width) - 3, (y + Height) - 1);

                alpha -= TintAmount;
                if (alpha < 0)
                    alpha = 0;
            }

            if (_pressed)
            {
                textY++;
                Parent.Graphics.Scale9Image(x, y, Width, Height, ButtonDown, 5, alpha);
            }
            else
                Parent.Graphics.Scale9Image(x, y, Width, Height, ButtonUp, 5, alpha);
            //Bitmaps.DT_WordWrap + Bitmaps.DT_AlignmentCenter
            Parent.Graphics.DrawTextInRect(Text, x, textY, Width, Height, new StringFormat { Alignment = StringAlignment.Center,  Trimming = StringTrimming.EllipsisWord }, (Enabled) ? FontColor : DisabledFontColor, Font);
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
                TriggerPressEvent(this);
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
                    TriggerReleaseEvent(this);
                    TriggerTapEvent(this);
                    e.StopPropagation();
                }
            }
            else
            {
                if (_pressed)
                {
                    _pressed = false;
                    TriggerReleaseEvent(this);
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
            ButtonUp.Dispose();
            ButtonDown.Dispose();
        }

        /// <summary>
        /// Bitmap that represents the up state.
        /// </summary>
        public Bitmap ButtonUp = Resources.GetBitmap(Resources.BitmapResources.Button_Up);

        /// <summary>
        /// Bitmap that represents the down state.
        /// </summary>
        public Bitmap ButtonDown = Resources.GetBitmap(Resources.BitmapResources.Button_Down);

        /// <summary>
        /// Text on the button.
        /// </summary>
        public string Text = String.Empty;

        /// <summary>
        /// Font used by the text.
        /// </summary>
        /// <remarks>This can be a font from Resources or one within Glide retrieved via the FontManager class.</remarks>
        public Font Font = Resources.GetFont(Resources.FontResources.droid_reg12);

        /// <summary>
        /// Font color.
        /// </summary>
        public Color FontColor = TinyCLR2.Glide.Ext.Colors.Black;

        /// <summary>
        /// Disabled font color.
        /// </summary>
        public Color DisabledFontColor = GlideUtils.Convert.ToColor("808080");

        /// <summary>
        /// Color to tint the button.
        /// </summary>
        public Color TintColor = TinyCLR2.Glide.Ext.Colors.Black;

        /// <summary>
        /// Amount of tint to apply.
        /// </summary>
        public ushort TintAmount = 0;
    }
}
