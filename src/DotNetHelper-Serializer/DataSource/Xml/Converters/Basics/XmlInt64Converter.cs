using System.Xml;

namespace DotNetHelper_Serializer.DataSource.Xml.Converters.Basics
{
    public sealed class XmlInt64Converter : XmlBasicRawConverter<long>
    {
        protected override long Parse(string value, XmlSerializationContext context)
        {
            return XmlConvert.ToInt64(value);
        }

        protected override string ToString(long value, XmlSerializationContext context)
        {
            return XmlConvert.ToString(value);
        }
    }
}