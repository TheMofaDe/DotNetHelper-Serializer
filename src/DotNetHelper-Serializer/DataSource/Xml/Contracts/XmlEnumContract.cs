using System;
using System.Collections.Generic;

namespace DotNetHelper_Serializer.DataSource.Xml.Contracts
{
    public sealed class XmlEnumContract : XmlContract
    {
        private readonly List<XmlEnumItem> items;

        public XmlEnumContract(Type valueType, XmlName name, IEnumerable<XmlEnumItem> items)
            : base(valueType, name)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            if (!valueType.IsEnum)
            {
                throw new ArgumentException("Expected enum type.", nameof(valueType));
            }

            this.items = new List<XmlEnumItem>(items);
            IsFlag = valueType.IsDefined(typeof(FlagsAttribute), false);
            UnderlyingType = System.Enum.GetUnderlyingType(valueType);
        }

        public Type UnderlyingType { get; }

        public IEnumerable<XmlEnumItem> Items => items;

        public bool IsFlag { get; }
    }
}