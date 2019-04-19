using System;

namespace DotNetHelper_Serializer.DataSource.Xml.Converters.Basics
{
    public sealed class XmlDateTimeConverter : XmlBasicRawConverter<DateTime>
    {
        protected override DateTime Parse(string value, XmlSerializationContext context)
        {
            return Convert.ToDateTime(value);
            //  return RfcDateTime.ParseDateTime(value);
        }

        protected override string ToString(DateTime value, XmlSerializationContext context)
        {
            return value.ToString();
            //    return RfcDateTime.ToString(value);
        }
    }
}