// Decompiled with JetBrains decompiler
// Type: System.Xml.ReaderPositionInfo
// Assembly: System.Xml.Legacy, Version=4.3.1.0, Culture=neutral, PublicKeyToken=null
// MVID: 04A8895C-E271-4174-9A7C-9A44FF541E99
// Assembly location: C:\Program Files (x86)\Microsoft .NET Micro Framework\v4.3\Assemblies\le\System.Xml.Legacy.dll

namespace System.Xml
{
  internal class ReaderPositionInfo : PositionInfo
  {
    private IXmlLineInfo lineInfo;

    public ReaderPositionInfo(IXmlLineInfo lineInfo)
    {
      this.lineInfo = lineInfo;
    }

    public override bool HasLineInfo()
    {
      return this.lineInfo.HasLineInfo();
    }

    public override int LineNumber
    {
      get
      {
        return this.lineInfo.LineNumber;
      }
    }

    public override int LinePosition
    {
      get
      {
        return this.lineInfo.LinePosition;
      }
    }
  }
}
