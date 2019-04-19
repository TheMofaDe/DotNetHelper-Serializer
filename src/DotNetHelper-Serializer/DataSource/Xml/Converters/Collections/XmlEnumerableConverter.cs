using System;

namespace DotNetHelper_Serializer.DataSource.Xml.Converters.Collections
{
    internal sealed class XmlEnumerableConverter : XmlCollectionConverter
    {
        public override bool CanRead(Type valueType)
        {
            return false;
        }

        public override ICollectionProxy CreateProxy(Type valueType)
        {
            throw new XmlSerializationException("Can't deserialize anonymous enumerable type.");
        }
    }
}