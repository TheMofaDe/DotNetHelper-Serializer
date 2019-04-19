using System;
using System.Collections.Generic;

namespace DotNetHelper_Serializer.DataSource.Xml.Converters.Collections
{
    public sealed class XmlArrayConverter : XmlConverterFactory
    {
        protected override bool AcceptType(Type valueType)
        {
            return valueType.IsArray;
        }

        protected override Type GetConverterType(Type valueType)
        {
            return typeof(XmlTypedArrayConverter<>).MakeGenericType(valueType.GetElementType());
        }

        private sealed class XmlTypedArrayConverter<TItem> : XmlCollectionConverter
        {
            public override bool CanRead(Type valueType)
            {
                return valueType == typeof(TItem[]);
            }

            public override ICollectionProxy CreateProxy(Type valueType)
            {
                return new ArrayProxy();
            }

            private sealed class ArrayProxy : ICollectionProxy
            {
                private readonly List<TItem> items = new List<TItem>();

                public void Add(object value)
                {
                    items.Add((TItem)value);
                }

                public object GetResult()
                {
                    return items.ToArray();
                }
            }
        }
    }
}