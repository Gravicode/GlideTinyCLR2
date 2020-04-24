// Decompiled with JetBrains decompiler
// Type: System.Xml.Utility
// Assembly: System.Xml.Legacy, Version=4.3.1.0, Culture=neutral, PublicKeyToken=null
// MVID: 04A8895C-E271-4174-9A7C-9A44FF541E99
// Assembly location: C:\Program Files (x86)\Microsoft .NET Micro Framework\v4.3\Assemblies\le\System.Xml.Legacy.dll

namespace System.Xml
{
  internal abstract class Utility
  {
    internal static string ToHexDigits(uint val)
    {
      char[] chArray = new char[8];
      int startIndex = 8;
      do
      {
        chArray[--startIndex] = "0123456789ABCDEF"[(int) val & 15];
        val >>= 4;
      }
      while (val != 0U);
      return new string(chArray, startIndex, 8 - startIndex);
    }
  }
}
