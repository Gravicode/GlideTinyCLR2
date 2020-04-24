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
using TinyCLR2.Glide.Ext;

namespace GHI.Glide.UI
{
    /// <summary>
    /// The TextBox component is a single-line text input.
    /// </summary>
    public class TextBox : InputBox
    {
        /// <summary>
        /// Creates a new TextBox.
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="alpha">Alpha</param>
        /// <param name="x">X-axis position.</param>
        /// <param name="y">Y-axis position.</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        public TextBox(string name, ushort alpha, int x, int y, int width, int height)
        {
            Name = name;
            Alpha = alpha;
            X = x;
            Y = y;
            Width = width;
            Height = height;

            // Default
            Text = String.Empty;
            if (TextAlign == null) TextAlign = new System.Drawing.StringFormat();
            TextAlign.Alignment = System.Drawing.StringAlignment.Near;// = Bitmaps.DT_AlignmentLeft;
            Font = FontManager.GetFont(FontManager.FontType.droid_reg12);
            FontColor = TinyCLR2.Glide.Ext.Colors.Black;
        }

        /// <summary>
        /// Renders the TextBox onto it's parent container's graphics.
        /// </summary>
        public override void Render()
        {
            int x = Parent.X + X;
            int y = Parent.Y + Y;
            ushort alpha = (Enabled) ? Alpha : (ushort)(Alpha / 3);
            Parent.Graphics.Scale9Image(x, y, Width, Height, _TextBox, 5, 5, 5, 5, alpha);
            Parent.Graphics.DrawTextInRect(ShortenText(Text), x + leftMargin, y + ((Height - Font.Height) / 2), Width - (leftMargin * 2), Height, TextAlign, FontColor, Font);
        }

        /// <summary>
        /// Disposes all disposable objects in this object.
        /// </summary>
        public override void Dispose()
        {
            _TextBox.Dispose();
        }
    }
}

