using System.Xml;

namespace DotNetHelper_Serializer.DataSource.Xml.Converters.Basics
{
    public sealed class XmlDecimalConverter : XmlBasicRawConverter<decimal>
    {
        protected override decimal Parse(string value, XmlSerializationContext context)
        {
            return XmlConvert.ToDecimal(value);
        }

        protected override string ToString(decimal value, XmlSerializationContext context)
        {
            return XmlConvert.ToString(value);
        }
    }
}