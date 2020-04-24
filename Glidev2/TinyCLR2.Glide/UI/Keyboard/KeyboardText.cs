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
    /// The KeyboardText component displays multiline wrapping text.
    /// </summary>
    internal class KeyboardText : DisplayObject
    {
        private Color _color = TinyCLR2.Glide.Ext.Colors.White;
        private Font _font = FontManager.GetFont(FontManager.FontType.droid_reg14);

        /// <summary>
        /// Creates a new KeyboardText.
        /// </summary>
        /// <param name="x">X-axis position.</param>
        /// <param name="y">Y-axis position.</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        public KeyboardText(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;

            // Default
            Text = String.Empty;
        }

        /// <summary>
        /// Renders the KeyboardText on it's parent container's graphics.
        /// </summary>
        public override void Render()
        {
            if (Parent == null)
                return;

            int x = Parent.X + X;
            int y = Parent.Y + Y;

            Parent.Graphics.DrawRectangle(TinyCLR2.Glide.Ext.Colors.Black, 0, x, y, Width, Height, 0, 0, TinyCLR2.Glide.Ext.Colors.Black, 0, 0, TinyCLR2.Glide.Ext.Colors.Black, 0, 0, Alpha);

            int i;
            int line = 0;
            string word;
            Size size = new Size();
            string newline = String.Empty;
            string str;

            if (IsPassword)
            {
                str = String.Empty;
                for (i = 0; i < Text.Length; i++)
                {
                    if (i < Text.Length - 1)
                        str += '*';
                    else
                        str += Text[i];
                }
            }
            else
            {
                str = Text;
            }

            string[] words = str.Split(' ');


            for (i = 0; i < words.Length; i++)
            {
                if (line * _font.Height >= Height) break;

                word = words[i];

                size = FontManager.GetSize(_font, newline + ((newline != "") ? " " : String.Empty) + word);

                if (size.Width <= Width)
                {
                    newline += ((newline != "") ? " " : String.Empty) + word;
                }
                else
                {
                    Parent.Graphics.DrawText(newline, _font, _color, x, y + (line * _font.Height));
                    newline = String.Empty;
                    i--;
                    line++;
                }
            }

            if (newline != String.Empty)
                Parent.Graphics.DrawText(newline, _font, _color, x, y + (line * _font.Height));

            // Draw the caret
            Parent.Graphics.DrawLine(
                TinyCLR2.Glide.Ext.Colors.Gray,
                2,
                x + size.Width + 4,
                y + (line * _font.Height),
                x + size.Width + 4,
                y + (line * _font.Height) + _font.Height);
        }

        /// <summary>
        /// A string of text.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Indicates whether or not this is a password.
        /// </summary>
        public bool IsPassword { get; set; }
    }
}
