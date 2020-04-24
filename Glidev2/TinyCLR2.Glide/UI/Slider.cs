////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (c) GHI Electronics, LLC.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;
//using Microsoft.SPOT;
//using Microsoft.SPOT.Presentation.Media;
using GHI.Glide.Display;
using GHI.Glide.Geom;
using System.Drawing;
using TinyCLR2.Glide.Properties;
using System.Diagnostics;
using GHI.Glide.UI;

namespace GHI.Glide.UI
{
    /// <summary>
    /// The Slider component lets users select a value by sliding a knob along a track.
    /// </summary>
    public class Slider : DisplayObject
    {
        private Bitmap _Button_Up = Resources.GetBitmap(Resources.BitmapResources.Button_Up);
        private Bitmap _Button_Down = Resources.GetBitmap(Resources.BitmapResources.Button_Down);

        private bool _dragging = false;
        private string _direction = SliderDirection.Horizontal;
        private Geom.Rectangle _knob;
        private int _knobSize;
        private int _lineSize;
        private int _tickInterval;
        private double _tickSize;
        private int _snapInterval;
        private double _snapSize;
        private double _pixelsPerValue;
        private double _min;
        private double _max;
        private double _value;
        private Thread _touchThread;

        /// <summary>
        /// Creates a new Slider component.
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="alpha">Alpha</param>
        /// <param name="x">X-axis position.</param>
        /// <param name="y">Y-axis position.</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        public Slider(string name, ushort alpha, int x, int y, int width, int height)
        {
            Name = name;
            Alpha = alpha;
            X = x;
            Y = y;
            Width = width;
            Height = height;

            // Default
            KnobSize = 20;
            SnapInterval = 10;
            TickInterval = 10;
            Minimum = 0;
            Maximum = 100;
            Value = 0;
        }

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

        private void RenderKnob(Point globalPoint)
        {
            Point localPoint = new Point(globalPoint.X - Rect.X, globalPoint.Y - Rect.Y);

            if (_direction == SliderDirection.Horizontal)
            {
                int half = _knob.Width / 2;
                int maxX = Width - _knob.Width;

                if (localPoint.X < half)
                    _knob.X = 0;
                else if (localPoint.X - half > maxX)
                    _knob.X = maxX;
                else
                    _knob.X = localPoint.X - half;

                int interval = (int)System.Math.Round(_knob.X / _snapSize);

                if (SnapInterval > 0)
                    _knob.X = (int)(interval * _snapSize);

                Value = _knob.X / _pixelsPerValue;
            }
            else
            {
                int half = _knob.Height / 2;
                int maxY = Height - _knob.Height;

                Debug.WriteLine(localPoint.ToString());

                if (localPoint.Y < half)
                    _knob.Y = 0;
                else if (localPoint.Y - half > maxY)
                    _knob.Y = maxY;
                else
                    _knob.Y = localPoint.Y - half;

                int interval = (int)System.Math.Round(_knob.Y / _snapSize);

                if (SnapInterval > 0)
                    _knob.Y = (int)(interval * _snapSize);

                Value = _max - (_knob.Y / _pixelsPerValue);
            }

            Invalidate();
        }

        /// <summary>
        /// Renders the Button onto it's parent container's graphics.
        /// </summary>
        public override void Render()
        {
            int x = Rect.X;
            int y = Rect.Y;

            // HACK: To prevent image/color retention.
            Parent.Graphics.DrawRectangle(Rect, TinyCLR2.Glide.Ext.Colors.Black, 255);

            ((Window)Parent).FillRect(Rect);

            if (_direction == SliderDirection.Horizontal)
            {
                int lineY = y + (Height / 2);
                int offsetX = _knob.Width / 2;
                int knobY = Height - _knob.Height;
                int tickX;
                int tickHeight = (int)System.Math.Ceiling(Height * 0.05);

                Parent.Graphics.DrawLine(TickColor, 1, x + offsetX, lineY, x + offsetX + _lineSize, lineY);

                if (TickInterval > 1)
                {
                    for (int i = 0; i < TickInterval + 1; i++)
                    {
                        tickX = x + offsetX + (int)(i * _tickSize);
                        Parent.Graphics.DrawLine(TickColor, 1, tickX, y, tickX, y + tickHeight);
                    }
                }

                if (_dragging)
                    Parent.Graphics.Scale9Image(x + _knob.X, y + knobY, _knob.Width, _knob.Height, _Button_Down, 5, Alpha);
                else
                    Parent.Graphics.Scale9Image(x + _knob.X, y + knobY, _knob.Width, _knob.Height, _Button_Up, 5, Alpha);
            }
            else
            {
                int lineX = x + (Width / 2);
                int offsetY = _knob.Height / 2;
                int knobX = Width - _knob.Width;
                int tickY;
                int tickWidth = (int)System.Math.Ceiling(Width * 0.05);

                Parent.Graphics.DrawLine(TickColor, 1, lineX, y + offsetY, lineX, y + offsetY + _lineSize);

                if (TickInterval > 1)
                {
                    for (int i = 0; i < TickInterval + 1; i++)
                    {
                        tickY = y + offsetY + (int)(i * _tickSize);
                        Parent.Graphics.DrawLine(TickColor, 1, x, tickY, x + tickWidth, tickY);
                    }
                }

                if (_dragging)
                    Parent.Graphics.Scale9Image(x + knobX, y + _knob.Y, _knob.Width, _knob.Height, _Button_Down, 5, Alpha);
                else
                    Parent.Graphics.Scale9Image(x + knobX, y + _knob.Y, _knob.Width, _knob.Height, _Button_Up, 5, Alpha);
            }
        }

        /// <summary>
        /// Handles the touch down event.
        /// </summary>
        /// <param name="e">Touch event arguments.</param>
        /// <returns>Touch event arguments.</returns>
        public override TouchEventArgs OnTouchDown(TouchEventArgs e)
        {
            // Global coordinates to local coordinates
            Point localPoint = new Point(e.Point.X - Rect.X, e.Point.Y - Rect.Y);

            if (_knob.Contains(localPoint))
            {
                _dragging = true;
                Invalidate();

                if (_touchThread == null || (_touchThread != null && !_touchThread.IsAlive))
                {
                    GlideTouch.IgnoreAllEvents = true;
                    _touchThread = new Thread(TouchThread);
                    _touchThread.Priority = ThreadPriority.Highest;
                    _touchThread.Start();
                }

                e.StopPropagation();
                return e;
            }
            
            if (Rect.Contains(e.Point))
            {
                RenderKnob(e.Point);
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
            Point localPoint = new Point(e.Point.X - Rect.X, e.Point.Y - Rect.Y);

            if (_knob.Contains(localPoint))
            {
                if (_dragging)
                {
                    _dragging = false;
                    Invalidate();
                    e.StopPropagation();
                }
            }
            else
            {
                if (_dragging)
                {
                    _dragging = false;
                    Invalidate();
                }
            }
            return e;
        }

        /// <summary>
        /// Handles the touch move event.
        /// </summary>
        /// <param name="e">Touch event arguments.</param>
        /// <returns>Touch event arguments.</returns>
        public override TouchEventArgs OnTouchMove(TouchEventArgs e)
        {
            if (_dragging)
                RenderKnob(e.Point);
            return e;
        }

        private void Init()
        {
            if (_direction == SliderDirection.Horizontal)
            {
                _knob = new Geom.Rectangle(0, 0, _knobSize, (int)((double)Height / 1.2));
                _lineSize = Width - _knobSize;
            }
            else
            {
                _knob = new Geom.Rectangle(0, 0, (int)((double)Width / 1.2), _knobSize);
                _lineSize = Height - _knobSize;
            }

            if (_tickInterval < 0)
                _tickInterval = 0;
            else if (_tickInterval > _lineSize)
                _tickInterval = _lineSize;

            if (_tickInterval > 0)
                _tickSize = (double)_lineSize / TickInterval;
            else
                _tickSize = (double)_lineSize / _lineSize;

            if (_snapInterval < 0)
                _snapInterval = 0;
            else if (_snapInterval > _lineSize)
                _snapInterval = _lineSize;

            if (_snapInterval > 0)
                _snapSize = (double)_lineSize / _snapInterval;
            else
                _snapSize = (double)_lineSize / _lineSize;

            if (_max > 0)
                _pixelsPerValue = _lineSize / (_max - _min);
        }

        private void TouchThread()
        {
            // These are used for the touch up event
            int lastX = 0;
            int lastY = 0;

            // These store the current X and Y
            int x = 0;
            int y = 0;

            // Keeps track of whether the panel was touched or not
            bool isTouched = false;

            // Create touch inputs that are used as arguments
            TouchInput[] touches = new TouchInput[] { new TouchInput() };

            // Begin touch panel polling
            while (_dragging)
            {
                GlideTouch.GetLastTouchPoint(ref x, ref y);

                if (x >= 0 && x <= Glide.LCD.Width && y >= 0 && y <= Glide.LCD.Height)
                {
                    if (isTouched == false)
                    {
                        // Touch down
                        touches[0].X = x;
                        touches[0].Y = y;
                        GlideTouch.RaiseTouchDownEvent(this, new TouchEventArgs(touches));

                        lastX = x;
                        lastY = y;
                        isTouched = true;
                    }
                    else
                    {
                        // Filter finger movements to avoid spamming
                        if (System.Math.Abs(x - lastX) > 2 || System.Math.Abs(y - lastY) > 2)
                        {
                            // Touch move
                            touches[0].X = x;
                            touches[0].Y = y;
                            GlideTouch.RaiseTouchMoveEvent(this, new TouchEventArgs(touches));

                            lastX = x;
                            lastY = y;
                        }
                    }
                }
                else
                {
                    if (isTouched == true)
                    {
                        // Touch up
                        touches[0].X = lastX;
                        touches[0].Y = lastY;
                        GlideTouch.RaiseTouchUpEvent(this, new TouchEventArgs(touches));

                        isTouched = false;
                    }
                }

                Thread.Sleep(30);
            }

            // Allow other threads to run so we dont get double touch events
            // once the message box closes.
            Thread.Sleep(0);

            GlideTouch.IgnoreAllEvents = false;
        }

        /// <summary>
        /// Direction of the slider; horizontal or vertical.
        /// </summary>
        public string Direction
        {
            get { return _direction; }
            set
            {
                if (value != _direction)
                {
                    _direction = value.ToLower();
                    Init();
                }
            }
        }

        /// <summary>
        /// Size of the knob.
        /// </summary>
        public int KnobSize
        {
            get { return _knobSize; }
            set
            {
                _knobSize = value;
                Init();
            }
        }

        /// <summary>
        /// Maximum value.
        /// </summary>
        public double Maximum
        {
            get { return _max; }
            set
            {
                _max = value;
                Init();
            }
        }

        /// <summary>
        /// Minimum value.
        /// </summary>
        public double Minimum
        {
            get { return _min; }
            set
            {
                _min = value;
                Init();
            }
        }

        /// <summary>
        /// Increment by which the value is increased or decreased as the user slides the knob.
        /// </summary>
        public int SnapInterval
        {
            get { return _snapInterval; }
            set
            {
                _snapInterval = value;
                Init();
            }
        }

        /// <summary>
        /// Tick mark spacing relative to the maximum value.
        /// </summary>
        public int TickInterval
        {
            get { return _tickInterval; }
            set
            {
                _tickInterval = value;
                Init();
            }
        }

        /// <summary>
        /// Gets or sets the current value.
        /// </summary>
        public double Value
        {
            get { return _value; }
            set
            {
                double oldValue = _value;
                _value = value;

                if (oldValue != _value)
                    TriggerValueChangedEvent(this);

                if (_direction == SliderDirection.Horizontal)
                    _knob.X = (int)((_value - _min) * _pixelsPerValue);
                else
                    _knob.Y = _lineSize - (int)((_value - _min) * _pixelsPerValue);
            }
        }

        /// <summary>
        /// Tick color.
        /// </summary>
        public Color TickColor = TinyCLR2.Glide.Ext.Colors.Black;
    }

    /// <summary>
    /// The orientation of the Slider component.
    /// </summary>
    public struct SliderDirection
    {
        /// <summary>
        /// Horizontal
        /// </summary>
        public const string Horizontal = "horizontal";

        /// <summary>
        /// Vertical
        /// </summary>
        public const string Vertical = "vertical";
    }
}

