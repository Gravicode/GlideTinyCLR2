namespace GHI.Glide.UI
{
    using System;

    public enum TouchGesture : uint
    {
        NoGesture = 0,
        Begin = 1,
        End = 2,
        Right = 3,
        UpRight = 4,
        Up = 5,
        UpLeft = 6,
        Left = 7,
        DownLeft = 8,
        Down = 9,
        DownRight = 10,
        Tap = 11,
        DoubleTap = 12,
        Zoom = 0x72,
        Pan = 0x73,
        Rotate = 0x74,
        TwoFingerTap = 0x75,
        Rollover = 0x76,
        UserDefined = 200
    }
}

