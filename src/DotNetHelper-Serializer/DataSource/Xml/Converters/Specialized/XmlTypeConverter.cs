using System;

namespace DotNetHelper_Serializer.DataSource.Xml.Converters.Specialized
{
    public sealed class XmlTypeConverter : XmlBasicConverter<Type>
    {
        protected override Type Parse(string value, XmlSerializationContext context)
        {
            return Type.GetType(value, true, false);
        }

        protected override string ToString(Type value, XmlSerializationContext context)
        {
            return value.ToString();
        }
    }
}