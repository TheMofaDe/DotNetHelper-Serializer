using System.Xml;

namespace DotNetHelper_Serializer.DataSource.Xml.Converters.Basics
{
    public sealed class XmlUInt32Converter : XmlBasicRawConverter<uint>
    {
        protected override uint Parse(string value, XmlSerializationContext context)
        {
            return XmlConvert.ToUInt32(value);
        }

        protected override string ToString(uint value, XmlSerializationContext context)
        {
            return XmlConvert.ToString(value);
        }
    }
}