using System;

namespace DotNetHelper_Serializer.DataSource.Xml.Contracts
{
    public interface IXmlContractResolver
    {
        XmlContract ResolveContract(Type valueType);
    }
}