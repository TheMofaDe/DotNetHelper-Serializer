namespace DotNetHelper_Serializer.DataSource.Xml.Converters.Collections
{
    public interface ICollectionProxy
    {
        void Add(object value);

        object GetResult();
    }
}