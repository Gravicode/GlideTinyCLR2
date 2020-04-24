// Decompiled with JetBrains decompiler
// Type: System.Xml.IncrementalReadDecoder
// Assembly: System.Xml.Legacy, Version=4.3.1.0, Culture=neutral, PublicKeyToken=null
// MVID: 04A8895C-E271-4174-9A7C-9A44FF541E99
// Assembly location: C:\Program Files (x86)\Microsoft .NET Micro Framework\v4.3\Assemblies\le\System.Xml.Legacy.dll

namespace System.Xml
{
  internal abstract class IncrementalReadDecoder
  {
    internal abstract int DecodedCount { get; }

    internal abstract bool IsFull { get; }

    internal abstract void SetNextOutputBuffer(Array array, int offset, int len);

    internal abstract int Decode(char[] chars, int startPos, int len);

    internal abstract int Decode(string str, int startPos, int len);

    internal abstract void Reset();
  }
}
