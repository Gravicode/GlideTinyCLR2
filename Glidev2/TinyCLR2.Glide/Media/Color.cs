// Decompiled with JetBrains decompiler
// Type: GHIElectronics.TinyCLR.UI.Media.Color
// Assembly: GHIElectronics.TinyCLR.UI, Version=2.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A928C630-4833-4BD4-B83A-283D9120CEB6
// Assembly location: C:\experiment\TinyCLR2\Demo\SampleApp\SampleApp\bin\Debug\GHIElectronics.TinyCLR.UI.dll

using System.Runtime.CompilerServices;

namespace GHI.Glide.Media
{
    public struct Color
    {
        public byte A { get; set; }//[IsReadOnly]

        public byte R { get; set; }//[IsReadOnly]

        public byte G { get; set; }//[IsReadOnly]

        public byte B { get; set; }//[IsReadOnly]

        private Color(byte a, byte r, byte g, byte b)
        {
            this.A = a;
            this.R = r;
            this.G = g;
            this.B = b;
        }

        public static Color FromArgb(byte a, byte r, byte g, byte b)
        {
            return new Color(a, r, g, b);
        }

        public static Color FromRgb(byte r, byte g, byte b)
        {
            return new Color(byte.MaxValue, r, g, b);
        }

        internal uint ToNativeColor()
        {
            return (uint)((int)this.R << 16 | (int)this.G << 8) | (uint)this.B;
        }

        internal ushort ToNativeAlpha()
        {
            return (ushort)this.A;
        }
    }
}
