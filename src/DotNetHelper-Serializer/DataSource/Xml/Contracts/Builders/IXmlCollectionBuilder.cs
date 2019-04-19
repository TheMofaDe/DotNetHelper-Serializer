namespace DotNetHelper_Serializer.DataSource.Xml.Contracts.Builders
{
    public interface IXmlCollectionBuilder : IXmlBuilder
    {
        XmlItemBuilder Item { get; set; }
    }
}