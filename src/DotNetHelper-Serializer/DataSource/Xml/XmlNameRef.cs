using System.Xml;
using DotNetHelper_Serializer.DataSource.Xml.Contracts;

namespace DotNetHelper_Serializer.DataSource.Xml
{
    internal struct XmlNameRef
    {
        public object LocalName;
        public object NamespaceUri;

        public XmlNameRef(XmlName name, XmlNameTable nameTable)
        {
            LocalName = nameTable.Add(name.LocalName);
            NamespaceUri = name.NamespaceUri != null ? nameTable.Add(name.NamespaceUri) : null;
        }

        public void Reset(XmlName name, XmlNameTable nameTable)
        {
            LocalName = nameTable.Add(name.LocalName);
            NamespaceUri = name.NamespaceUri != null ? nameTable.Add(name.NamespaceUri) : null;
        }

        public bool Match(XmlReader reader)
        {
            if (object.ReferenceEquals(LocalName, reader.LocalName))
            {
                return NamespaceUri == null || object.ReferenceEquals(NamespaceUri, reader.NamespaceURI);
            }

            return false;
        }
    }
}