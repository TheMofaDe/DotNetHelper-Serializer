using System;

namespace DotNetHelper_Serializer.DataSource.Xml.Contracts.Builders
{
    public class XmlEnumContractBuilder : XmlContractBuilder
    {
        private static readonly XmlEnumItemCollection EmptyItems = new XmlEnumItemCollection();

        public XmlEnumContractBuilder(Type valueType)
            : base(valueType)
        {
            if (!valueType.IsEnum)
            {
                throw new ArgumentException("Contract type must be Enum.", nameof(valueType));
            }
        }

        public XmlEnumItemCollection Items { get; set; }

        public static XmlEnumContractBuilder Create(XmlEnumContract contract)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            return new XmlEnumContractBuilder(contract.ValueType)
            {
                Name = contract.Name,
                Items = new XmlEnumItemCollection(contract.Items)
            };
        }

        public XmlEnumContract Build()
        {
            return new XmlEnumContract(
                ValueType,
                Name,
                Items != null ? Items : EmptyItems);
        }

        public override XmlContract BuildContract()
        {
            return Build();
        }
    }
}