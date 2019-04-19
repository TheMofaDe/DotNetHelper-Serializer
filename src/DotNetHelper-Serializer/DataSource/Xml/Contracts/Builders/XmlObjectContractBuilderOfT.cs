namespace DotNetHelper_Serializer.DataSource.Xml.Contracts.Builders
{
    public class XmlObjectContractBuilder<T> : XmlObjectContractBuilder
    {
        public XmlObjectContractBuilder()
            : base(typeof(T))
        {
        }
    }
}