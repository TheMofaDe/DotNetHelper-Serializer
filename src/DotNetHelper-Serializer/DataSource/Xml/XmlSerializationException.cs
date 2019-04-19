using System;

namespace DotNetHelper_Serializer.DataSource.Xml
{
    public class XmlSerializationException : Exception
    {
        public XmlSerializationException(string message)
            : base(message)
        {
        }

        public XmlSerializationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}