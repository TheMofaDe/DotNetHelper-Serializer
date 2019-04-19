using System;
using System.Xml;

namespace DotNetHelper_Serializer.DataSource.Xml.Converters
{
    public interface IXmlConverter
    {
        bool CanRead(Type valueType);

        bool CanWrite(Type valueType);

        void WriteXml(XmlWriter writer, object value, XmlSerializationContext context);

        object ReadXml(XmlReader reader, XmlSerializationContext context);
    }
}