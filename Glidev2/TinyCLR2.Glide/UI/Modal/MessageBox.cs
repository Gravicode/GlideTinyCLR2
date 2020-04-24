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
using TinyCLR2.Glide.Ext;

namespace GHI.Glide.UI
{
    /// <summary>
    /// The MessageBox component displays a box of text to instruct and inform the user.
    /// </summary>
    public class MessageBox : Modal
    {
        /// <summary>
        /// Creates a new MessageBox.
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="alpha">Alpha</param>
        /// <param name="x">X-axis position.</param>
        /// <param name="y">Y-axis position.</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        public MessageBox(string name, ushort alpha, int x, int y, int width, int height) : base (name, alpha, x, y, width, height)
        {
            // Nothing.
        }

        /// <summary>
        /// Renders the MessageBox onto it's parent container's graphics.
        /// </summary>
        public override void Render()
        {
            base.Render();

            int x = Parent.X + X;
            int y = Parent.Y + Y + TitlebarHeight;

            int messageHeight = Height - TitlebarHeight;
            //Bitmaps.DT_AlignmentLeft + Bitmaps.DT_WordWrap
            Parent.Graphics.DrawTextInRect(Message, x + 10, y + 5, Width - 20, messageHeight - 10, new StringFormat() { Alignment = StringAlignment.Near, Trimming = StringTrimming.EllipsisWord }, MessageFontColor, MessageFont);
        }

        /// <summary>
        /// Message
        /// </summary>
        public string Message = String.Empty;

        /// <summary>
        /// Message font.
        /// </summary>
        public Font MessageFont = Resources.GetFont(Resources.FontResources.droid_reg11);

        /// <summary>
        /// Message font color.
        /// </summary>
        public Color MessageFontColor = TinyCLR2.Glide.Ext.Colors.Black;
    }
}
