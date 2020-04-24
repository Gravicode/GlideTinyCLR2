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
using System.Drawing;
namespace GHI.Glide.UI
{
    /// <summary>
    /// The Canvas component provides methods to draw.
    /// </summary>
    /// <remarks>
    /// Everytime you call a draw method on this component it's added to a queue.
    /// When this component is asked to render by it's parent the queue will be drawn onto it's parent container's graphics.
    /// This makes it a good choice for drawing organizing shapes such as fieldsets and separator lines.
    /// </remarks>
    public class Canvas : DisplayObject
    {
        private ArrayList _queue = new ArrayList();

        /// <summary>
        /// Renders all drawing requests onto it's parent container's graphics.
        /// </summary>
        public override void Render()
        {
            object[] var;
            for (int i = 0; i < _queue.Count; i++)
            {
                var = (object[])_queue[i];
                switch ((string)var[0])
                {
                    case "DrawEllipse":
                        Parent.Graphics.DrawEllipse((Color)var[1], (int)var[2], (int)var[3], (int)var[4], (int)var[5]);
                        break;

                    case "DrawImage":
                        Parent.Graphics.DrawImage((int)var[1], (int)var[2], (Bitmap)var[3], (int)var[4], (int)var[5], (int)var[6], (int)var[7], (ushort)var[8]);
                        break;

                    case "DrawLine":
                        Parent.Graphics.DrawLine((Color)var[1], (int)var[2], (int)var[3], (int)var[4], (int)var[5], (int)var[6]);
                        break;

                    case "DrawRectangle":
                        Parent.Graphics.DrawRectangle((Color)var[1], (int)var[2], (int)var[3], (int)var[4], (int)var[5], (int)var[6], (int)var[7], (int)var[8], (Color)var[9], (int)var[10], (int)var[11], (Color)var[12], (int)var[13], (int)var[14], (ushort)var[15]);
                        break;
                }
            }
        }

        /// <summary>
        /// Adds a draw ellipse request.
        /// </summary>
        /// <param name="color">Color</param>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <param name="xRadius">X Radius</param>
        /// <param name="yRadius">Y Radius</param>
        /// <returns>Index of request.</returns>
        public int DrawEllipse(Color color, int x, int y, int xRadius, int yRadius)
        {
            _queue.Add(new object[6] { "DrawEllipse", color, x, y, xRadius, yRadius });
            return _queue.Count - 1;
        }

        /// <summary>
        /// Adds a draw image request.
        /// </summary>
        /// <param name="xDst">X</param>
        /// <param name="yDst">Y</param>
        /// <param name="bitmap">Bitmap</param>
        /// <param name="xSrc">Bitmap X offset.</param>
        /// <param name="ySrc">Bitmap Y offset.</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="opacity">Opacity</param>
        /// <returns>Index of request.</returns>
        public int DrawImage(int xDst, int yDst, Bitmap bitmap, int xSrc, int ySrc, int width, int height, ushort opacity)
        {
            _queue.Add(new object[9] { "DrawImage", xDst, yDst, bitmap, xSrc, ySrc, width, height, opacity });
            return _queue.Count - 1;
        }

        /// <summary>
        /// Adds a draw line request.
        /// </summary>
        /// <param name="color">Color</param>
        /// <param name="thickness">Thickness</param>
        /// <param name="x0">Start X</param>
        /// <param name="y0">Start Y</param>
        /// <param name="x1">End X</param>
        /// <param name="y1">End Y</param>
        /// <returns>Index of request.</returns>
        public int DrawLine(Color color, int thickness, int x0, int y0, int x1, int y1)
        {
            _queue.Add(new object[7] { "DrawLine", color, thickness, x0, y0, x1, y1 });
            return _queue.Count - 1;
        }

        /// <summary>
        /// Adds a draw rectangle request.
        /// </summary>
        /// <param name="colorOutline">Color outline.</param>
        /// <param name="thicknessOutline">Thickness outline.</param>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="xCornerRadius">X corder radius.</param>
        /// <param name="yCornerRadius">Y corder radius.</param>
        /// <param name="colorGradientStart">Color gradient start.</param>
        /// <param name="xGradientStart">X gradient start.</param>
        /// <param name="yGradientStart">Y gradient start.</param>
        /// <param name="colorGradientEnd">Color gradient end.</param>
        /// <param name="xGradientEnd">X gradient end.</param>
        /// <param name="yGradientEnd">Y gradient end.</param>
        /// <param name="opacity">Opacity</param>
        /// <returns>Index of request.</returns>
        public int DrawRectangle(Color colorOutline, int thicknessOutline, int x, int y, int width, int height, int xCornerRadius, int yCornerRadius, Color colorGradientStart, int xGradientStart, int yGradientStart, Color colorGradientEnd, int xGradientEnd, int yGradientEnd, ushort opacity)
        {
            _queue.Add(new object[16] { "DrawRectangle", colorOutline, thicknessOutline, x, y, width, height, xCornerRadius, yCornerRadius, colorGradientStart, xGradientStart, yGradientStart, colorGradientEnd, xGradientEnd, yGradientEnd, opacity });
            return _queue.Count - 1;
        }

        /// <summary>
        /// Draws a fieldset around a Label component.
        /// </summary>
        /// <param name="textBlock">TextBlock component.</param>
        /// <param name="leftMargin">Left margin from label.</param>
        /// <param name="rightMargin">Right margin from label.</param>
        /// <param name="height">Height</param>
        /// <param name="borderColor">Border color.</param>
        /// <param name="borderThickness">Border thickness.</param>
        public void DrawFieldset(TextBlock textBlock, int leftMargin, int rightMargin, int height, Color borderColor, int borderThickness)
        {
            int frontX = textBlock.Parent.X + textBlock.X;
            int startY = textBlock.Parent.Y + textBlock.Y + (textBlock.Height / 2);
            int backX = frontX + textBlock.Width;

            _queue.Add(new object[7] { "DrawLine", borderColor, borderThickness, frontX - leftMargin, startY, frontX, startY });
            _queue.Add(new object[7] { "DrawLine", borderColor, borderThickness, backX, startY, backX + rightMargin, startY });
            _queue.Add(new object[7] { "DrawLine", borderColor, borderThickness, frontX - leftMargin, startY, frontX - leftMargin, startY + height });
            _queue.Add(new object[7] { "DrawLine", borderColor, borderThickness, backX + rightMargin, startY, backX + rightMargin, startY + height });
            _queue.Add(new object[7] { "DrawLine", borderColor, borderThickness, frontX - leftMargin, startY + height, backX + rightMargin, startY + height });
        }

        /// <summary>
        /// Clears all drawing requests.
        /// </summary>
        public void Clear()
        {
            _queue = new ArrayList();
        }

        /// <summary>
        /// Clear a drawing request at a specified index.
        /// </summary>
        /// <param name="index">Index to be cleared.</param>
        public void ClearAt(int index)
        {
            if (index > -1 && index < _queue.Count)
                _queue.RemoveAt(index);
        }
    }
}
