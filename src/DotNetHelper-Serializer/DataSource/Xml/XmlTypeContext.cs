using System;
using System.Xml;
using DotNetHelper_Serializer.DataSource.Xml.Contracts;
using DotNetHelper_Serializer.DataSource.Xml.Converters;

namespace DotNetHelper_Serializer.DataSource.Xml
{
    public class XmlTypeContext
    {
        public XmlTypeContext(XmlContract contract, IXmlConverter readConverter, IXmlConverter writeConverter)
        {
            this.Contract = contract;
            this.ReadConverter = readConverter;
            this.WriteConverter = writeConverter;
            ReadXml = readConverter != null ? readConverter.ReadXml : NotReadable(contract.ValueType);
            WriteXml = writeConverter != null ? writeConverter.WriteXml : NotWritable(contract.ValueType);
        }

        public XmlContract Contract { get; }

        public IXmlConverter ReadConverter { get; }

        public IXmlConverter WriteConverter { get; }

        public Func<XmlReader, XmlSerializationContext, object> ReadXml { get; }

        public Action<XmlWriter, object, XmlSerializationContext> WriteXml { get; }

        private static Func<XmlReader, XmlSerializationContext, object> NotReadable(Type valueType)
        {
            return (r, c) => throw new XmlSerializationException(string.Format("Readable converter for the type \"{0}\" is not found.", valueType));
        }

        private static Action<XmlWriter, object, XmlSerializationContext> NotWritable(Type valueType)
        {
            return (w, v, c) => throw new XmlSerializationException(string.Format("Writable converter for the type \"{0}\" is not found.", valueType));
        }
    }
}