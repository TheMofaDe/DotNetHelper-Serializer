using System.Xml;

namespace DotNetHelper_Serializer.DataSource.Xml.Converters.Basics
{
    public sealed class XmlSByteConverter : XmlBasicRawConverter<sbyte>
    {
        protected override sbyte Parse(string value, XmlSerializationContext context)
        {
            return XmlConvert.ToSByte(value);
        }

        protected override string ToString(sbyte value, XmlSerializationContext context)
        {
            return XmlConvert.ToString(value);
        }
    }
}