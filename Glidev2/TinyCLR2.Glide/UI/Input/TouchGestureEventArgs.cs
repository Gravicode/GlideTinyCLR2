namespace GHI.Glide.UI
{
    using System;

    public class TouchGestureEventArgs : EventArgs
    {
        public readonly DateTime Timestamp;
        public TouchGesture Gesture;
        public int X;
        public int Y;
        public ushort Arguments;

        public double Angle
        {
            get
            {
                return (double) this.Arguments;
            }
        }
    }
}

