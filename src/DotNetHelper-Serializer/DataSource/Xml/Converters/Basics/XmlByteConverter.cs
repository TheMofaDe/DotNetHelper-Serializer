using System.Xml;

namespace DotNetHelper_Serializer.DataSource.Xml.Converters.Basics
{
    public sealed class XmlByteConverter : XmlBasicRawConverter<byte>
    {
        protected override byte Parse(string value, XmlSerializationContext context)
        {
            return XmlConvert.ToByte(value);
        }

        protected override string ToString(byte value, XmlSerializationContext context)
        {
            return XmlConvert.ToString(value);
        }
    }
}