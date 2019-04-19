using System;
using System.Collections.Generic;

namespace DotNetHelper_Serializer.DataSource.Xml.Contracts
{
    public sealed partial class XmlObjectContract : XmlContract
    {
        private static readonly List<XmlProperty> EmptyProperties = new List<XmlProperty>();

        private readonly List<XmlProperty> properties;

        public XmlObjectContract(
            Type valueType,
            XmlName name,
            IEnumerable<XmlProperty> properties = null,
            XmlTypeHandling? typeHandling = null,
            XmlItem item = null)
            : base(valueType, name)
        {
            var elementCount = 0;

            if (properties == null)
            {
                properties = EmptyProperties;
            }

            this.Item = item;
            this.TypeHandling = typeHandling;
            this.properties = new List<XmlProperty>(properties);

            foreach (var property in this.properties)
            {
                if (property == null)
                {
                    throw new ArgumentNullException("properties.property");
                }

                if (valueType != property.PropertyInfo.ReflectedType)
                {
                    throw new ArgumentException("Property must be declared in contract type.", "properties.property");
                }

                if (property.IsRequired || property.DefaultValue != null)
                {
                    HasRequiredOrDefaultsProperties = true;
                }

                if (property.MappingType == XmlMappingType.Element)
                {
                    elementCount++;
                }
                else if (property.MappingType == XmlMappingType.InnerText)
                {
                    if (InnerTextProperty != null)
                    {
                        throw new XmlSerializationException("Contract must have only one innerText property.");
                    }

                    InnerTextProperty = property;
                }
            }

            if (InnerTextProperty != null && elementCount > 0)
            {
                throw new XmlSerializationException("Contract must not contain elements, if it contains innerText property.");
            }

            this.properties.Sort(XmlPropertyComparer.Instance);
        }

        public IReadOnlyList<XmlProperty> Properties => properties;

        public XmlItem Item { get; }

        public XmlTypeHandling? TypeHandling { get; }

        internal bool HasRequiredOrDefaultsProperties { get; }

        internal XmlProperty InnerTextProperty { get; }

        protected override XmlMember GetDefaultMember()
        {
            return new XmlMember(
                ValueType,
                Name,
                XmlMappingType.Element,
                TypeHandling,
                null,
                null,
                null,
                Item);
        }
    }
}