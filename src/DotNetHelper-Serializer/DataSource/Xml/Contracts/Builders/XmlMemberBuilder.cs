using System;

namespace DotNetHelper_Serializer.DataSource.Xml.Contracts.Builders
{
    public class XmlMemberBuilder : IXmlBuilder
    {
        public XmlMemberBuilder(Type valueType)
        {
            if (valueType == null)
            {
                throw new ArgumentNullException(nameof(valueType));
            }

            ValueType = valueType;
        }

        public Type ValueType { get; }

        public XmlName Name { get; set; }

        public XmlTypeHandling? TypeHandling { get; set; }
    }
}