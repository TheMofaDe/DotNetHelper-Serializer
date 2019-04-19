using System;

namespace DotNetHelper_Serializer.DataSource.Xml.Converters.Basics
{
    public sealed class XmlCustomDateTimeConverter : XmlBasicConverter<DateTime>
    {
        public XmlCustomDateTimeConverter(string format = null)
        {
            this.Format = format;
        }

        public string Format { get; }

        protected override DateTime Parse(string value, XmlSerializationContext context)
        {
            return DateTime.ParseExact(value, Format, context.Settings.Culture);
        }

        protected override string ToString(DateTime value, XmlSerializationContext context)
        {
            return value.ToString(Format, context.Settings.Culture);
        }
    }
}