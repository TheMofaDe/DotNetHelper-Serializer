using System.Xml;

namespace DotNetHelper_Serializer.DataSource.Xml.Converters.Basics
{
    public sealed class XmlBooleanConverter : XmlBasicRawConverter<bool>
    {
        protected override bool Parse(string value, XmlSerializationContext context)
        {
            return XmlConvert.ToBoolean(value);
        }

        protected override string ToString(bool value, XmlSerializationContext context)
        {
            return XmlConvert.ToString(value);
        }
    }
}
