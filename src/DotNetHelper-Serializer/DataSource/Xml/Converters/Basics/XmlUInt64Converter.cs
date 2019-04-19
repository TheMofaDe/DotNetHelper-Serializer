using System.Xml;

namespace DotNetHelper_Serializer.DataSource.Xml.Converters.Basics
{
    public sealed class XmlUInt64Converter : XmlBasicRawConverter<ulong>
    {
        protected override ulong Parse(string value, XmlSerializationContext context)
        {
            return XmlConvert.ToUInt64(value);
        }

        protected override string ToString(ulong value, XmlSerializationContext context)
        {
            return XmlConvert.ToString(value);
        }
    }
}