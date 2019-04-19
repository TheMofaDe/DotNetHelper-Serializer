using System;

namespace DotNetHelper_Serializer.DataSource.Xml.Contracts
{
    public sealed class XmlKnownType : XmlMember
    {
        public XmlKnownType(
            Type valueType,
            XmlName name,
            XmlTypeHandling? typeHandling = null,
            XmlNullValueHandling? nullValueHandling = null,
            XmlDefaultValueHandling? defaultValueHandling = null,
            object defaultValue = null)
            : base(valueType, name, XmlMappingType.Element, typeHandling, nullValueHandling, defaultValueHandling, defaultValue)
        {
        }
    }
}