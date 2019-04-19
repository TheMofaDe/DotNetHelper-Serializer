using System.Xml;

namespace DotNetHelper_Serializer.DataSource.Xml.Converters.Basics
{
    public sealed class XmlInt16Converter : XmlBasicRawConverter<short>
    {
        protected override short Parse(string value, XmlSerializationContext context)
        {
            return XmlConvert.ToInt16(value);
        }

        protected override string ToString(short value, XmlSerializationContext context)
        {
            return XmlConvert.ToString(value);
        }
    }
}
