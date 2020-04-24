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

namespace GHI.Glide.UI
{
    /// <summary>
    /// The Image component allows displaying images.
    /// </summary>
    public class Image : DisplayObject
    {
        private bool _pressed;

        /// <summary>
        /// Creates a new Image.
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="alpha">Alpha</param>
        /// <param name="x">X-axis position.</param>
        /// <param name="y">Y-axis position.</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        public Image(string name, ushort alpha, int x, int y, int width, int height)
        {
            Name = name;
            Alpha = alpha;
            X = x;
            Y = y;
            Width = width;
            Height = height;

            // Default
            Bitmap = new Bitmap(Width, Height);
            BitmapGraphic = System.Drawing.Graphics.FromImage(Bitmap);
            var pen = new Pen(TinyCLR2.Glide.Ext.Colors.Fuchsia);
            BitmapGraphic.DrawRectangle(pen, 0, 0, Width, Height);
            //BitmapGraphic.DrawRectangle(0, 0, 0, 0, Width, Height, 0, 0, TinyCLR2.Glide.Ext.Colors.Fuchsia.ToNativeColor(), 0, 0, TinyCLR2.Glide.Ext.Colors.Fuchsia.ToNativeColor(), 0, 0, Alpha);
        }

        /// <summary>
        /// Renders the Image onto it's parent container's graphics.
        /// </summary>
        public override void Render()
        {
            ushort alpha = (Enabled) ? Alpha : (ushort)(Alpha / 3);
            int x = Parent.X + X;
            int y = Parent.Y + Y;
            int width, height;
            float scale;

            // Is the bitmap the same size as the component?
            // Should we stretch?
            if ((Bitmap.Width != Width || Bitmap.Height != Height) && Stretch)
            {
                if (Bitmap.Height <= Bitmap.Width)
                    scale = (float)Bitmap.Height / (float)Bitmap.Width;
                else
                    scale = (float)Bitmap.Width / (float)Bitmap.Height;

                width = Width;
                Height = height = (int)(width * scale);

                Parent.Graphics.StretchImage(x, y, Bitmap, width, height, alpha);
            }
            else
            {
                width = (Bitmap.Width <= Width) ? Bitmap.Width : Width;
                height = (Bitmap.Height <= Height) ? Bitmap.Height : Height;

                Parent.Graphics.DrawImage(x, y, Bitmap, 0, 0, width, height, alpha);
            }
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
            if (Rect.Contains(e.Point))
            {
                if (_pressed)
                {
                    _pressed = false;
                    TriggerTapEvent(this);
                    e.StopPropagation();
                }
            }
            else
            {
                if (_pressed)
                    _pressed = false;
            }
            return e;
        }

        /// <summary>
        /// Disposes all disposable objects in this object.
        /// </summary>
        public override void Dispose()
        {
            Bitmap.Dispose();
        }

        /// <summary>
        /// Fills a rectangle on this image's parent container's graphics using it's Bitmap.
        /// </summary>
        /// <param name="rect"></param>
        public void FillRect(Geom.Rectangle rect)
        {
            if (Bitmap != null)
                Parent.Graphics.DrawImage(rect.X, rect.Y, Bitmap, rect.X, rect.Y, rect.Width, rect.Height);
        }

        /// <summary>
        /// Bitmap
        /// </summary>
        public Bitmap Bitmap { get; set; }
        /// <summary>
        /// Graphic for Bitmap
        /// </summary>
        public System.Drawing.Graphics BitmapGraphic { get; set; }
        /// <summary>
        /// Indicates whether or not the image should be stretched if the control is larger than the bitmap.
        /// </summary>
        public bool Stretch { get; set; }
    }
}
