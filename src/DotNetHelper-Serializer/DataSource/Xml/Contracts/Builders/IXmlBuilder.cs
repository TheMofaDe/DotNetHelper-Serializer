using System;

namespace DotNetHelper_Serializer.DataSource.Xml.Contracts.Builders
{
    public interface IXmlBuilder
    {
        Type ValueType { get; }

        XmlName Name { get; set; }
    }
}