// Decompiled with JetBrains decompiler
// Type: System.Xml.XmlNamespaces
// Assembly: System.Xml.Legacy, Version=4.3.1.0, Culture=neutral, PublicKeyToken=null
// MVID: 04A8895C-E271-4174-9A7C-9A44FF541E99
// Assembly location: C:\Program Files (x86)\Microsoft .NET Micro Framework\v4.3\Assemblies\le\System.Xml.Legacy.dll

using System.Collections;

namespace System.Xml
{
  internal class XmlNamespaces
  {
    private ArrayList m_namespaceList;

    public XmlNamespaces()
    {
      this.m_namespaceList = new ArrayList();
    }

    public XmlNamespace this[string prefix]
    {
      get
      {
        int count = this.m_namespaceList.Count;
        for (int index = 0; index < count; ++index)
        {
          XmlNamespace xmlNamespace = (XmlNamespace) this.m_namespaceList[index];
          if (xmlNamespace.Prefix == prefix)
            return xmlNamespace;
        }
        return (XmlNamespace) null;
      }
    }

    public int Add(string prefix, string namespaceURI)
    {
      if (this.NewNamespaceExists(prefix, namespaceURI))
        return -1;
      return this.m_namespaceList.Add((object) new XmlNamespace(prefix, namespaceURI));
    }

    private bool NewNamespaceExists(string prefix, string namespaceURI)
    {
      int count = this.m_namespaceList.Count;
      for (int index = 0; index < count; ++index)
      {
        XmlNamespace xmlNamespace = (XmlNamespace) this.m_namespaceList[index];
        if (xmlNamespace.Prefix == prefix || xmlNamespace.NamespaceURI == namespaceURI)
          return true;
      }
      return false;
    }
  }
}
