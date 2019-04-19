using System;

namespace DotNetHelper_Serializer.DataSource.Xml.Converters.Basics
{
    public sealed class XmlGuidConverter : XmlBasicRawConverter<Guid>
    {
        public XmlGuidConverter(string format = null)
        {
            this.Format = format;
        }

        public string Format { get; }

        protected override Guid Parse(string value, XmlSerializationContext context)
        {
            return Guid.Parse(value);
        }

        protected override string ToString(Guid value, XmlSerializationContext context)
        {
            return value.ToString(Format);
        }
    }
}