using System;
using DotNetHelper_Serializer.DataSource.Xml.Utilities;

namespace DotNetHelper_Serializer.DataSource.Xml.Contracts
{
    public class XmlContract
    {
        private XmlMember root;
        private Func<object> creator;

        public XmlContract(Type valueType, XmlName name)
        {
            if (valueType == null)
            {
                throw new ArgumentNullException(nameof(valueType));
            }

            this.ValueType = valueType;
            this.Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public Type ValueType { get; }

        public XmlName Name { get; }

        internal XmlMember Root => root ?? (root = GetDefaultMember());

        internal object CreateDefault()
        {
            if (creator == null)
            {
                creator = DynamicWrapperFactory.CreateConstructor(ValueType);
            }

            return creator();
        }

        protected virtual XmlMember GetDefaultMember()
        {
            return new XmlMember(
                ValueType,
                Name,
                XmlMappingType.Element,
                XmlTypeHandling.None,
                XmlNullValueHandling.Include,
                XmlDefaultValueHandling.Include);
        }
    }
}