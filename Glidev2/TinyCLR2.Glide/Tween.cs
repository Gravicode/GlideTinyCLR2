////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (c) GHI Electronics, LLC.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;
////using Microsoft.SPOT;
////using Microsoft.SPOT.Presentation.Media;
using GHI.Glide.Display;
using System.Drawing;
namespace GHI.Glide
{
    /// <summary>
    /// Holds the steps for different animations.
    /// </summary>
    public struct Steps
    {
        /// <summary>
        /// Steps to slide between windows.
        /// </summary>
        public int SlideWindow { get; set; }
    }

    /// <summary>
    /// The Tween class gives you access to methods that move, resize, and fade objects over a number of steps.
    /// </summary>
    public static class Tween
    {
        public enum ChipTypes { EMX =5, ChipworkX=6, Emulator };
        /// <summary>
        /// Indicates the number of steps an animation needs to tween from start to finish.
        /// </summary>
        public static Steps NumSteps;

        /// <summary>
        /// Initializes the Tween class.
        /// </summary>
        /// <remarks>Sets the best number of steps to tween from start to finish based on your GHI Electronics board model.</remarks>
        static Tween()
        {
            ChipTypes chipType = ChipTypes.Emulator;
            // Default is 5
            NumSteps.SlideWindow = 5;

            switch ((int)chipType)
            {
                case 5: // EMX
                    NumSteps.SlideWindow = 7;
                    break;

                case 6: // ChipworkX
                    NumSteps.SlideWindow = 50;
                    break;

                default:
                    if (Glide.IsEmulator)
                        NumSteps.SlideWindow = 100;
                    break;
            }
        }

        /// <summary>
        /// Calculates a position for each step.
        /// </summary>
        /// <param name="start">Start position.</param>
        /// <param name="end">End position.</param>
        /// <param name="steps">Number of steps.</param>
        /// <returns>An array of steps to move from start to end.</returns>
        public static int[] GetSteps(int start, int end, int steps)
        {
            float step1, step2, step3, step4;

            int[] values = new int[steps + 1];
            for (int i = 0; i < steps + 1; i++)
            {
                step1 = end - start;
                step2 = step1 / steps;
                step3 = step2 * i;
                step4 = start + step3;
                values[i] = (int)step4;
            }
            return values;
        }

        /// <summary>
        /// Slides between windows in a specified direction.
        /// </summary>
        /// <param name="fromWindow">From window.</param>
        /// <param name="toWindow">To window.</param>
        /// <param name="direction">Direction of movement.</param>
        public static void SlideWindow(Window fromWindow, Window toWindow, Direction direction)
        {
            Glide.MainWindow = null;
            int[] x1, x2, y1, y2;
            int fromY = fromWindow.Y - fromWindow.ListY;
            int toY = toWindow.Y - toWindow.ListY;
            int index;

            switch (direction)
            {
                case Direction.Left:
                    x1 = Tween.GetSteps(0, toWindow.Width, NumSteps.SlideWindow);
                    x2 = Tween.GetSteps(-fromWindow.Width, 0, NumSteps.SlideWindow);
                    index = 0;
                    while (index < x1.Length)
                    {
                        Glide.screen.DrawImage(fromWindow.Graphics.GetBitmap(), new System.Drawing.Rectangle(x1[index], fromY, fromWindow.Width, fromWindow.Height), new System.Drawing.Rectangle(  0, 0, fromWindow.Width, fromWindow.Height), System.Drawing.GraphicsUnit.Pixel);
                        Glide.screen.DrawImage(toWindow.Graphics.GetBitmap(), new System.Drawing.Rectangle(x2[index], toY , toWindow.Width, toWindow.Height), new System.Drawing.Rectangle(0, 0, toWindow.Width, toWindow.Height), System.Drawing.GraphicsUnit.Pixel);
                        Glide.screen.Flush();
                        //Glide.screen.Flush(0, 0, Glide.screen.Width, Glide.screen.Height);
                        index++;
                    }
                    Glide.MainWindow = toWindow;
                    break;

                case Direction.Right:
                    x1 = GetSteps(0, -fromWindow.Width, NumSteps.SlideWindow);
                    x2 = GetSteps(toWindow.Width, 0, NumSteps.SlideWindow);
                    index = 0;
                    while (index < x1.Length)
                    {
                        Glide.screen.DrawImage(fromWindow.Graphics.GetBitmap(), new Rectangle( x1[index], fromY, fromWindow.Width, fromWindow.Height) ,new Rectangle( 0, 0, fromWindow.Width, fromWindow.Height), GraphicsUnit.Pixel);
                        Glide.screen.DrawImage(toWindow.Graphics.GetBitmap(), new Rectangle( x2[index], toY, toWindow.Width, toWindow.Height) ,new Rectangle( 0, 0, toWindow.Width, toWindow.Height), GraphicsUnit.Pixel);
                        Glide.screen.Flush();
                        //Glide.screen.Flush(0,0,Glide.screen.Width,Glide.screen.Height);
                        index++;
                    }
                    Glide.MainWindow = toWindow;
                    break;

                case Direction.Up:
                    y1 = GetSteps(fromY, -fromWindow.Height, NumSteps.SlideWindow);
                    y2 = GetSteps(fromY + fromWindow.Height, toY, NumSteps.SlideWindow);
                    index = 0;
                    while (index < y1.Length)
                    {
                        Glide.screen.DrawImage(fromWindow.Graphics.GetBitmap(), new Rectangle (0, y1[index], fromWindow.Width, fromWindow.Height),new Rectangle( 0, 0, fromWindow.Width, fromWindow.Height),GraphicsUnit.Pixel);
                        Glide.screen.DrawImage(toWindow.Graphics.GetBitmap(), new Rectangle(0, y2[index], toWindow.Width, toWindow.Height),new Rectangle( 0, 0, toWindow.Width, toWindow.Height),GraphicsUnit.Pixel);
                        Glide.screen.Flush();
                        //Glide.screen.Flush(0,0,Glide.screen.Width,Glide.screen.Height);
                        index++;
                    }
                    Glide.MainWindow = toWindow;
                    break;

                case Direction.Down:
                    y1 = GetSteps(toY, fromWindow.Height, NumSteps.SlideWindow);
                    y2 = GetSteps(-fromWindow.Height, fromY, NumSteps.SlideWindow);
                    index = 0;
                    while (index < y1.Length)
                    {
                        Glide.screen.DrawImage(fromWindow.Graphics.GetBitmap(), new Rectangle(0, y1[index], fromWindow.Width, fromWindow.Height), new Rectangle( 0, 0, fromWindow.Width, fromWindow.Height), GraphicsUnit.Pixel);
                        Glide.screen.DrawImage(toWindow.Graphics.GetBitmap(), new Rectangle(0, y2[index], toWindow.Width, toWindow.Height),new Rectangle( 0, 0, toWindow.Width, toWindow.Height),GraphicsUnit.Pixel);
                        Glide.screen.Flush();
                        //Glide.screen.Flush(0,0,Glide.screen.Width,Glide.screen.Height);
                        index++;
                    }
                    Glide.MainWindow = toWindow;
                    break;
            }
        }
    }
}
