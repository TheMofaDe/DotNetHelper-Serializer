using System.Xml;

namespace DotNetHelper_Serializer.DataSource.Xml.Converters.Basics
{
    public sealed class XmlInt32Converter : XmlBasicRawConverter<int>
    {
        protected override int Parse(string value, XmlSerializationContext context)
        {
            return XmlConvert.ToInt32(value);
        }

        protected override string ToString(int value, XmlSerializationContext context)
        {
            return XmlConvert.ToString(value);
        }
    }
}