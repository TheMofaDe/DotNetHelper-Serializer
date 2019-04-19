using DotNetHelper_Serializer.DataSource.Xml.Contracts;

namespace DotNetHelper_Serializer.DataSource.Xml.Converters
{
    public interface IXmlConverterFactory
    {
        IXmlConverter CreateConverter(XmlContract contract);
    }
}