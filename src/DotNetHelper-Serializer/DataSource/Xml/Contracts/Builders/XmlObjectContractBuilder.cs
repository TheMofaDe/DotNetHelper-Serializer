using System;
using DotNetHelper_Serializer.Extension;

namespace DotNetHelper_Serializer.DataSource.Xml.Contracts.Builders
{
    public class XmlObjectContractBuilder : XmlContractBuilder, IXmlCollectionBuilder
    {
        public XmlObjectContractBuilder(Type valueType)
            : base(valueType)
        {
        }

        public XmlTypeHandling? TypeHandling { get; set; }

        public XmlPropertyBuilderCollection Properties { get; set; }

        public XmlItemBuilder Item { get; set; }

        public static XmlObjectContractBuilder Create(XmlObjectContract contract)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            return new XmlObjectContractBuilder(contract.ValueType)
            {
                Name = contract.Name,
                TypeHandling = contract.TypeHandling,
                Properties = XmlPropertyBuilderCollection.Create(contract.Properties),
                Item = contract.Item != null ? XmlItemBuilder.Create(contract.Item) : null
            };
        }

        public XmlObjectContract Build()
        {
            return new XmlObjectContract(
                ValueType,
                Name ?? ValueType.GetShortName(),
                Properties?.Build(),
                TypeHandling,
                Item?.Build());
        }

        public override XmlContract BuildContract()
        {
            return Build();
        }
    }
}