using System;

namespace DotNetHelper_Serializer.DataSource.Xml.Contracts.Builders
{
    public sealed class XmlEnumContractBuilder<T> : XmlEnumContractBuilder
        where T : struct, IConvertible
    {
        public XmlEnumContractBuilder()
            : base(typeof(T))
        {
        }
    }
}