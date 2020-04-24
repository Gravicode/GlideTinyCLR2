////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (c) GHI Electronics, LLC.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
//using Microsoft.SPOT;
using GHI.Glide.Display;
using TinyCLR2.Glide.Properties;

namespace GHI.Glide.UI
{
    /// <summary>
    /// The ProgressBar component displays the progress of content that is being loaded.
    /// </summary>
    public class ProgressBar : DisplayObject
    {
        private System.Drawing.Bitmap _ProgressBar = Resources.GetBitmap(Resources.BitmapResources.ProgressBar);
        private System.Drawing.Bitmap _ProgressBar_Fill = Resources.GetBitmap(Resources.BitmapResources.ProgressBar_Fill);

        /// <summary>
        /// Creates a new ProgressBar.
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="alpha">Alpha</param>
        /// <param name="x">X-axis position.</param>
        /// <param name="y">Y-axis position.</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        public ProgressBar(string name, ushort alpha, int x, int y, int width, int height)
        {
            Name = name;
            Alpha = alpha;
            X = x;
            Y = y;
            Width = width;
            Height = height;

            // Default
            Direction = Direction.Right;
            MaxValue = 100;
            Value = 0;
        }

        /// <summary>
        /// Renders the ProgressBar onto it's parent container's graphics.
        /// </summary>
        public override void Render()
        {
            int x = Parent.X + X;
            int y = Parent.Y + Y;
            ushort alpha = (Enabled) ? Alpha : (ushort)(Alpha / 3);

            Parent.Graphics.Scale9Image(x, y, Width, Height, _ProgressBar, 4, 5, 4, 5, alpha);

            x += 1;
            y += 1;
            int width = Width;
            int height = Height;
            float size = (float)Value / (float)MaxValue;

            if (Direction == Direction.Right || Direction == Direction.Left)
            {
                width = (int)((Width - 2) * size);
                height = Height - 2;
            }
            else if (Direction == Direction.Up || Direction == Direction.Down)
            {
                width = Width - 2;
                height = (int)((Height - 2) * size);
            }

            if (Direction == Direction.Left)
            {
                x += (Width - 2) - width;
            }
            else if (Direction == Direction.Up)
            {
                y += (Height - 2) - height;
            }

            Parent.Graphics.Scale9Image(x, y, width, height, _ProgressBar_Fill, 4, 5, 4, 5, alpha);
        }

        /// <summary>
        /// Disposes all disposable objects in this object.
        /// </summary>
        public override void Dispose()
        {
            _ProgressBar.Dispose();
            _ProgressBar_Fill.Dispose();
        }

        /// <summary>
        /// Direction in which the load bar expands.
        /// </summary>
        public Direction Direction { get; set; }

        /// <summary>
        /// Maximum value
        /// </summary>
        public int MaxValue { get; set; }

        /// <summary>
        /// Value
        /// </summary>
        public int Value { get; set; }
    }
}
