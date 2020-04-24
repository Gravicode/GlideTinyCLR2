// Decompiled with JetBrains decompiler
// Type: System.Xml.XmlNameTable
// Assembly: System.Xml.Legacy, Version=4.3.1.0, Culture=neutral, PublicKeyToken=null
// MVID: 04A8895C-E271-4174-9A7C-9A44FF541E99
// Assembly location: C:\Program Files (x86)\Microsoft .NET Micro Framework\v4.3\Assemblies\le\System.Xml.Legacy.dll

namespace System.Xml
{
  public abstract class XmlNameTable
  {
    public abstract string Get(char[] array, int offset, int length);

    public abstract string Get(string array);

    public abstract string Add(char[] array, int offset, int length);

    public abstract string Add(string array);
  }
}
