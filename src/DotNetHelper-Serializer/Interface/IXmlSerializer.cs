using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using DotNetHelper_IO.Interface;
using DotNetHelper_Serializer.DataSource.Xml;

namespace DotNetHelper_Serializer.Interface
{
    public interface IXmlSerializer : ISerializer
    {

        XmlSerializerSettings Settings { get; set; }

        bool? CanSerialize(Type valueType, bool throwOnCant = false);
        bool CanDeserialize(Type valueType, bool throwOnCant = false);
        void SerializeToXmlWriter(XmlWriter writer, Type valueType, object value);
        void SerializeToXmlWriter<T>(XmlWriter writer, T value) where T : class;
        void SerializeToTextWriter(TextWriter output, Type valueType, object value);
        void SerializeToTextWriter<T>(TextWriter output, T value) where T : class;
        string SerializeToJson(string xml, IJsonSerializer jsonSerializer);
        XDocument XmlStringToXDocument(string xml);
        XmlDocument XmlStringToXmlDocument(string xml);
        string GetXmlFromDocument(XmlDocument document);
        string GetXmlFromDocument(XDocument document);


    }
}
