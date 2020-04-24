namespace GHI.Glide.UI
{
    using System;

    public class TouchEvent : BaseEvent
    {
        public DateTime Time;
        public TouchInput[] Touches;
    }
}

