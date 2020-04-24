////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (c) GHI Electronics, LLC.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;
using GHI.Glide.UI;
//using Microsoft.SPOT;
//using Microsoft.SPOT.Hardware;
//using Microsoft.SPOT.Touch;
//using Microsoft.SPOT.Presentation;
//using Microsoft.SPOT.Presentation.Media;
using GHI.Glide.Geom;

namespace GHI.Glide
{
    public enum EventCategory
    {
        Unknown = 0,
        Touch = 1,
        Gesture = 2,
        Storage = 3,
        Network = 4,
        SleepLevel = 5,
        PowerLevel = 6,
        TimeService = 7,
        LargeBuffer = 8,
        Custom = 100, // 0x00000064
    }
    public interface IEventListener
    {
        void InitializeForEventSource();

        bool OnEvent(BaseEvent ev);
    }
    /// <summary>
    /// Touch event arguments.
    /// </summary>
    public class TouchEventArgs
    {
        /// <summary>
        /// Indicates whether or not to continue processing the event.
        /// </summary>
        public bool Propagate = true;

        /// <summary>
        /// The point of contact.
        /// </summary>
        public Point Point;

        /// <summary>
        /// Creates a new TouchEventArgs.
        /// </summary>
        /// <param name="Touches">TouchInput</param>
        public TouchEventArgs(TouchInput[] Touches)
        {
            Point = new Point(Touches[0].X, Touches[0].Y);
        }

        /// <summary>
        /// Creates a new TouchEventArgs.
        /// </summary>
        /// <param name="point">Point</param>
        public TouchEventArgs(Point point)
        {
            Point = point;
        }

        /// <summary>
        /// Stops propagation.
        /// </summary>
        public void StopPropagation()
        {
            Propagate = false;
        }
    }

    /// <summary>
    /// The TouchGesture class defines gestures.
    /// </summary>
    public enum TouchGesture : uint
    {
        /// <summary>
        /// No Gesture
        /// </summary>
        /// <remarks>Can be used to represent an error gesture or unknown gesture.</remarks>
        NoGesture = 0,

        /// <summary>
        /// Begin
        /// </summary>
        /// <remarks>Used to identify the beginning of a Gesture Sequence; App can use this to highlight UIElement or some other sort of notification.</remarks>
        Begin = 1,

        /// <summary>
        /// End
        /// </summary>
        /// <remarks>Used to identify the end of a gesture sequence; Fired when last finger involved in a gesture is removed.</remarks>
        End = 2,

        /// <summary>
        /// Right
        /// </summary>
        Right = 3,

        /// <summary>
        /// Up Right
        /// </summary>
        UpRight = 4,

        /// <summary>
        /// Up
        /// </summary>
        Up = 5,

        /// <summary>
        ///  Up Left
        /// </summary>
        UpLeft = 6,

        /// <summary>
        /// Left
        /// </summary>
        Left = 7,

        /// <summary>
        /// Down Left
        /// </summary>
        DownLeft = 8,

        /// <summary>
        /// Down
        /// </summary>
        Down = 9,

        /// <summary>
        /// Down Right
        /// </summary>
        DownRight = 10,

        /// <summary>
        /// Tap
        /// </summary>
        Tap = 11,

        /// <summary>
        /// Double Tap
        /// </summary>
        DoubleTap = 12,

        /// <summary>
        /// Zoom
        /// </summary>
        /// <remarks>Equivalent to your "Pinch" gesture.</remarks>
        Zoom = 114,

        /// <summary>
        /// Pan
        /// </summary>
        /// <remarks>Equivalent to your "Scroll" gesture.</remarks>
        Pan = 115,

        /// <summary>
        /// Rotate
        /// </summary>
        Rotate = 116,

        /// <summary>
        /// Two finger tap.
        /// </summary>
        TwoFingerTap = 117,
        
        /// <summary>
        /// Rollover
        /// </summary>
        Rollover = 118,

        /// <summary>
        /// Undefined
        /// </summary>
        /// <remarks>Additional touch gestures.</remarks>
        UserDefined = 200,
    }

    /// <summary>
    /// Touch gesture event arguments.
    /// </summary>
    public class TouchGestureEventArgs
    {
        /// <summary>
        /// Indicates whether or not to continue processing the event.
        /// </summary>
        public bool Propagate = true;

        /// <summary>
        /// Time the event occured.
        /// </summary>
        public readonly DateTime Timestamp;

        /// <summary>
        /// X coordinate.
        /// </summary>
        /// <remarks>The X forms the center location of the gesture for multi-touch or the starting location for single touch.</remarks>
        public readonly int X;

        /// <summary>
        /// Y coordinate.
        /// </summary>
        /// <remarks>The Y forms the center location of the gesture for multi-touch or the starting location for single touch.</remarks>
        public readonly int Y;

        /// <summary>
        /// Indicates the gesture.
        /// </summary>
        public readonly TouchGesture Gesture;

        // 2 bytes for gesture-specific arguments.
        // TouchGesture.Zoom: Arguments = distance between fingers
        // TouchGesture.Rotate: Arguments = angle in degrees (0-360)

        /// <summary>
        /// Touch gesture arguments.
        /// </summary>
        public readonly ushort Arguments;

        /// <summary>
        /// Creates a new TouchGestureEventArgs.
        /// </summary>
        /// <param name="gesture">Touch gesture.</param>
        /// <param name="x">X-axis position.</param>
        /// <param name="y">Y-axis position.</param>
        /// <param name="arguments">Touch gesture arguments.</param>
        /// <param name="timestamp">Time the event occured.</param>
        public TouchGestureEventArgs(TouchGesture gesture, int x, int y, ushort arguments, DateTime timestamp)
        {
            Gesture = gesture;
            X = x;
            Y = y;
            Arguments = arguments;
            Timestamp = timestamp;
        }

        /// <summary>
        /// Stops propagation.
        /// </summary>
        public void StopPropagation()
        {
            Propagate = false;
        }
    }

    /// <summary>
    /// Touch event handler.
    /// </summary>
    /// <param name="sender">Object associated with the event.</param>
    /// <param name="e">Touch event arguments.</param>
    public delegate void TouchEventHandler(object sender, TouchEventArgs e);

    /// <summary>
    /// Touch gesture event handler.
    /// </summary>
    /// <param name="sender">Object associated with the event.</param>
    /// <param name="e">Touch event arguments.</param>
    public delegate void TouchGestureEventHandler(object sender, TouchGestureEventArgs e);

    /// <summary>
    /// The GlideTouch class handles all touch functionality.
    /// </summary>
    public static class GlideTouch
    {
        public static Point LastTouch;
        /// <summary>
        /// X-axis of the last touch point.
        /// </summary>
        public static int TouchX;

        /// <summary>
        /// Y-axis of the last touch point.
        /// </summary>
        public static int TouchY;

        /// <summary>
        /// Indicates whether all touch events are ignored or not.
        /// </summary>
        public static bool IgnoreAllEvents = false;

        private class TouchConnection : IEventListener
        {
            public void InitializeForEventSource() { }

            private bool wasMoveEvent = false;

            public TouchConnection()
            {
                LastTouch = new Point(0, 0);
                Calibrated = false;
            }

            public bool OnEvent(BaseEvent baseEvent)
            {
                if (IgnoreAllEvents)
                    return true;

                if (baseEvent is TouchEvent)
                {
                    TouchEvent e = (TouchEvent)baseEvent;

                    if (e.EventMessage == 3)
                    {
                        // If the same position -- do nothing
                        if (wasMoveEvent && e.Touches[0].X == TouchX && e.Touches[0].Y == TouchY)
                        {
                            // Nothing
                        }
                        else
                        {
                            wasMoveEvent = true;
                            TouchX = e.Touches[0].X;
                            TouchY = e.Touches[0].Y;
                            RaiseTouchMoveEvent(this, new TouchEventArgs(e.Touches));
                        }
                    }
                    else if (e.EventMessage == 1)
                    {
                        wasMoveEvent = false;
                        LastTouch.X = e.Touches[0].X;
                        LastTouch.Y = e.Touches[0].Y;
                        RaiseTouchDownEvent(this, new TouchEventArgs(e.Touches));
                       

                    }
                    else if (e.EventMessage == 2)
                    {
                        wasMoveEvent = false;
                        LastTouch.X = e.Touches[0].X;
                        LastTouch.Y = e.Touches[0].Y;
                        RaiseTouchUpEvent(this, new TouchEventArgs(e.Touches));
                    }
                }
                else if (baseEvent is GenericEvent)
                {
                    GenericEvent genericEvent = (GenericEvent)baseEvent;
                    if (genericEvent.EventCategory == (byte)EventCategory.Gesture)
                        RaiseTouchGestureEvent(this, new TouchGestureEventArgs((TouchGesture)genericEvent.EventMessage, genericEvent.X, genericEvent.Y, (ushort)genericEvent.EventData, genericEvent.Time));
                }

                return true;
            }
        }
        public static void GetLastTouchPoint(ref int x,ref int y)
        {
            x = LastTouch.X;
            y = LastTouch.Y;
        }
        private static TouchConnection touchConnection;

        //private static ExtendedWeakReference calWeakRef;
        private static class GlideCalibration { }

        public static void RaiseTouchDownEvent(int x, int y)
        {
            if (touchConnection != null)
            {
                touchConnection.OnEvent(new TouchEvent() { Touches = new TouchInput[] { new TouchInput() { X= x, Y=y} }, EventMessage= 1, Time = DateTime.UtcNow });
            }
        }

        public static void RaiseTouchMoveEvent(TouchInput[] inputs)
        {
            if (touchConnection != null)
            {
                touchConnection.OnEvent(new TouchEvent() { Touches = inputs, EventMessage = 3, Time = DateTime.UtcNow });
            }
        }
        public static void RaiseTouchUpEvent(int x, int y)
        {
            if (touchConnection != null)
            {
                touchConnection.OnEvent(new TouchEvent() { Touches = new TouchInput[] { new TouchInput() { X = x, Y = y } }, EventMessage = 2 , Time = DateTime.UtcNow });
            }
        }

        /// <summary>
        /// Initializes the touch panel.
        /// </summary>
        public static void Initialize()
        {
            touchConnection = new TouchConnection();
            
            /*
            Touch.Initialize(touchConnection);
            
            TouchCollectorConfiguration.CollectionMode = CollectionMode.InkAndGesture;
            TouchCollectorConfiguration.CollectionMethod = CollectionMethod.Native;
            TouchCollectorConfiguration.SamplingFrequency = 50;

            calWeakRef = ExtendedWeakReference.RecoverOrCreate(typeof(GlideCalibration), 0, ExtendedWeakReference.c_SurvivePowerdown);
            calWeakRef.Priority = (Int32)ExtendedWeakReference.PriorityLevel.Important;
            CalSettings = (CalibrationSettings)calWeakRef.Target;

            if (CalSettings != null)
            {
                Touch.ActiveTouchPanel.SetCalibration(CalSettings.Points.Length, CalSettings.SX, CalSettings.SY, CalSettings.CX, CalSettings.CY);
                Calibrated = true;
            }*/
        }

        internal static void SaveCalibration(CalibrationSettings settings)
        {
            //calWeakRef.Target = settings;
        }

        /// <summary>
        /// Raises a touch down event.
        /// </summary>
        /// <param name="sender">Object associated with the event.</param>
        /// <param name="e">Touch event arguments.</param>
        /// <remarks>
        /// Raises a touch down event. This event is handled by the Window currently assigned to Glide.MainWindow.
        /// Once this Window receives a touch down event, it's passed on to it's children.
        /// The first child to handle the event stops propagation to the remaining children.
        /// </remarks>
        public static void RaiseTouchDownEvent(object sender, TouchEventArgs e) { TouchDownEvent(sender, e); }

        /// <summary>
        /// Raises a touch up event.
        /// </summary>
        /// <param name="sender">Object associated with the event.</param>
        /// <param name="e">Touch event arguments.</param>
        /// <remarks>
        /// Raises a touch up event. This event is handled by the Window currently assigned to Glide.MainWindow.
        /// Once this Window receives a touch up event, it's passed on to it's children.
        /// The first child to handle the event stops propagation to the remaining children.
        /// </remarks>
        public static void RaiseTouchUpEvent(object sender, TouchEventArgs e) { TouchUpEvent(sender, e); }

        /// <summary>
        /// Raises a touch move event.
        /// </summary>
        /// <param name="sender">Object associated with the event.</param>
        /// <param name="e">Touch event arguments.</param>
        /// <remarks>
        /// Raises a touch move event. This event is handled by the Window currently assigned to Glide.MainWindow.
        /// Once this Window receives a touch move event, it's passed on to it's children.
        /// The first child to handle the event stops propagation to the remaining children.
        /// </remarks>
        public static void RaiseTouchMoveEvent(object sender, TouchEventArgs e) { TouchMoveEvent(sender, e); }

        /// <summary>
        /// Raises a touch gesture event.
        /// </summary>
        /// <param name="sender">Object associated with the event.</param>
        /// <param name="e">Touch event arguments.</param>
        public static void RaiseTouchGestureEvent(object sender, TouchGestureEventArgs e) { TouchGestureEvent(sender, e); }

        /// <summary>
        /// Touch down event handler.
        /// </summary>
        public static event TouchEventHandler TouchDownEvent = delegate { };

        /// <summary>
        /// Touch up event handler.
        /// </summary>
        public static event TouchEventHandler TouchUpEvent = delegate { };

        /// <summary>
        /// Touch move event handler.
        /// </summary>
        public static event TouchEventHandler TouchMoveEvent = delegate { };

        /// <summary>
        /// Touch gesture event handler.
        /// </summary>
        public static event TouchGestureEventHandler TouchGestureEvent = delegate { };

        /// <summary>
        /// Indicates whether or not the panel has been calibrated.
        /// </summary>
        public static bool Calibrated { get; internal set; }

        /// <summary>
        /// Current calibration settings if set othwerwise null.
        /// </summary>
        public static CalibrationSettings CalSettings;

      
    }

    /// <summary>
    /// Calibration Settings
    /// </summary>
    [Serializable]
    public sealed class CalibrationSettings
    {
        /// <summary>
        /// Calibration Points
        /// </summary>
        public Point[] Points { get; set; }

        /// <summary>
        /// Screen X Buffer
        /// </summary>
        public short[] SX { get; set; }

        /// <summary>
        /// Screen Y Buffer
        /// </summary>
        public short[] SY { get; set; }

        /// <summary>
        /// Uncalibrated X Buffer
        /// </summary>
        public short[] CX { get; set; }

        /// <summary>
        /// Uncalibrated Y Buffer
        /// </summary>
        public short[] CY { get; set; }
    }
}
