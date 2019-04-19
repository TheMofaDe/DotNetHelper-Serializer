using System;
using System.Xml;

namespace DotNetHelper_Serializer.DataSource.Xml.Converters.Basics
{
    public sealed class XmlDateTimeOffsetConverter : XmlBasicRawConverter<DateTimeOffset>
    {
        protected override DateTimeOffset Parse(string value, XmlSerializationContext context)
        {
            return XmlConvert.ToDateTimeOffset(value);
        }

        protected override string ToString(DateTimeOffset value, XmlSerializationContext context)
        {
            return XmlConvert.ToString(value);
        }
    }
}