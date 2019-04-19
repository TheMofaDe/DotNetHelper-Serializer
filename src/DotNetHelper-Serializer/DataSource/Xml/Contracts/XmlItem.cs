using System;
using System.Collections.Generic;

namespace DotNetHelper_Serializer.DataSource.Xml.Contracts
{
    public sealed class XmlItem : XmlMember
    {
        public XmlItem(Type valueType, XmlName name, XmlTypeHandling? typeHandling = null, IEnumerable<XmlKnownType> knownTypes = null)
            : base(valueType, name, typeHandling: typeHandling, knownTypes: knownTypes)
        {
        }
    }
}