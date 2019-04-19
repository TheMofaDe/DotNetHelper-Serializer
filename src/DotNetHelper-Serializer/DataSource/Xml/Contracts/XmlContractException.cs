using System;

namespace DotNetHelper_Serializer.DataSource.Xml.Contracts
{
    public class XmlContractException : XmlSerializationException
    {
        public XmlContractException(string message)
            : base(message)
        {
        }

        public XmlContractException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}