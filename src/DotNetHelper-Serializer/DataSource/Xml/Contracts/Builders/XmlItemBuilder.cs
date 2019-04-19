using System;
using DotNetHelper_Serializer.Extension;

namespace DotNetHelper_Serializer.DataSource.Xml.Contracts.Builders
{
    public class XmlItemBuilder : XmlMemberBuilder
    {
        public XmlItemBuilder(Type valueType)
            : base(valueType)
        {
        }

        public XmlKnownTypeBuilderCollection KnownTypes { get; set; }

        public static XmlItemBuilder Create(XmlItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            return new XmlItemBuilder(item.ValueType)
            {
                Name = item.Name,
                TypeHandling = item.TypeHandling,
                KnownTypes = item.KnownTypes != null ? XmlKnownTypeBuilderCollection.Create(item.KnownTypes) : null
            };
        }

        public XmlItem Build()
        {
            return new XmlItem(
                ValueType,
                Name ?? ValueType.GetShortName(),
                TypeHandling,
                KnownTypes?.Build());
        }
    }
}