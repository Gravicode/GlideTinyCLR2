// Decompiled with JetBrains decompiler
// Type: System.Xml.IncrementalReadDummyDecoder
// Assembly: System.Xml.Legacy, Version=4.3.1.0, Culture=neutral, PublicKeyToken=null
// MVID: 04A8895C-E271-4174-9A7C-9A44FF541E99
// Assembly location: C:\Program Files (x86)\Microsoft .NET Micro Framework\v4.3\Assemblies\le\System.Xml.Legacy.dll

namespace System.Xml
{
  internal class IncrementalReadDummyDecoder : IncrementalReadDecoder
  {
    internal override int DecodedCount
    {
      get
      {
        return -1;
      }
    }

    internal override bool IsFull
    {
      get
      {
        return false;
      }
    }

    internal override void SetNextOutputBuffer(Array array, int offset, int len)
    {
    }

    internal override int Decode(char[] chars, int startPos, int len)
    {
      return len;
    }

    internal override int Decode(string str, int startPos, int len)
    {
      return len;
    }

    internal override void Reset()
    {
    }
  }
}
