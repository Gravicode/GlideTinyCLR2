// Decompiled with JetBrains decompiler
// Type: System.Xml.PositionInfo
// Assembly: System.Xml.Legacy, Version=4.3.1.0, Culture=neutral, PublicKeyToken=null
// MVID: 04A8895C-E271-4174-9A7C-9A44FF541E99
// Assembly location: C:\Program Files (x86)\Microsoft .NET Micro Framework\v4.3\Assemblies\le\System.Xml.Legacy.dll

namespace System.Xml
{
  internal class PositionInfo : IXmlLineInfo
  {
    public virtual bool HasLineInfo()
    {
      return false;
    }

    public virtual int LineNumber
    {
      get
      {
        return 0;
      }
    }

    public virtual int LinePosition
    {
      get
      {
        return 0;
      }
    }

    public static PositionInfo GetPositionInfo(object o)
    {
      IXmlLineInfo lineInfo = o as IXmlLineInfo;
      if (lineInfo != null)
        return (PositionInfo) new ReaderPositionInfo(lineInfo);
      return new PositionInfo();
    }
  }
}
