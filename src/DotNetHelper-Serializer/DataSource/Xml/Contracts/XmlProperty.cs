using System;
using System.Collections.Generic;
using System.Reflection;
using DotNetHelper_Serializer.DataSource.Xml.Utilities;
using DotNetHelper_Serializer.Extension;

namespace DotNetHelper_Serializer.DataSource.Xml.Contracts
{
    public class XmlProperty : XmlMember
    {
        private Action<object, object> setter;
        private Func<object, object> getter;

        public XmlProperty(
            PropertyInfo propertyInfo,
            XmlName name,
            XmlMappingType mappingType = XmlMappingType.Element,
            bool isRequired = false,
            XmlTypeHandling? typeHandling = null,
            XmlNullValueHandling? nullValueHandling = null,
            XmlDefaultValueHandling? defaultValueHandling = null,
            object defaultValue = null,
            XmlItem item = null,
            IEnumerable<XmlKnownType> knownTypes = null,
            bool isCollection = false,
            int order = -1)
            : base(propertyInfo.PropertyType, name, mappingType, typeHandling, nullValueHandling, defaultValueHandling, defaultValue, item, knownTypes)
        {
            if (isCollection)
            {
                if (!propertyInfo.PropertyType.IsTypeIEnumerable()) // 
                {
                    throw new ArgumentException("Collection flag is available only for the IEnumerable type.");
                }

                this.IsCollection = true;
            }

            this.PropertyInfo = propertyInfo;
            this.IsRequired = isRequired;
            this.Order = order;
            HasGetterAndSetter = propertyInfo.CanRead && propertyInfo.CanWrite;
        }

        public PropertyInfo PropertyInfo { get; }

        public string PropertyName => PropertyInfo.Name;

        public bool IsRequired { get; }

        public bool IsCollection { get; }

        public int Order { get; }

        internal bool HasGetterAndSetter { get; }

        internal object GetValue(object target)
        {
            if (getter == null)
            {
                getter = DynamicWrapperFactory.CreateGetter(PropertyInfo);
            }

            return getter(target);
        }

        internal void SetValue(object target, object value)
        {
            if (setter == null)
            {
                setter = DynamicWrapperFactory.CreateSetter(PropertyInfo);
            }

            setter(target, value);
        }
    }
}