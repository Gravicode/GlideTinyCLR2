namespace System.Runtime.InteropServices {
    using System;
    /** Used in the StructLayoutAttribute class
     **/
    [Serializable()]
    public enum LayoutKind {
        Sequential = 0, // 0x00000008,
        Explicit = 2, // 0x00000010,
        Auto = 3, // 0x00000000,
    }
}


