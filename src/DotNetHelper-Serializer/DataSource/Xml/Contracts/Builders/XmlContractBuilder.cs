using System;

namespace DotNetHelper_Serializer.DataSource.Xml.Contracts.Builders
{
    public abstract class XmlContractBuilder : IXmlBuilder
    {
        protected XmlContractBuilder(Type valueType)
        {
            if (valueType == null)
            {
                throw new ArgumentNullException(nameof(valueType));
            }

            ValueType = valueType;
        }

        public Type ValueType { get; }

        public XmlName Name { get; set; }

        public abstract XmlContract BuildContract();
    }
}