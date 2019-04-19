using System;

namespace DotNetHelper_Serializer.DataSource.Xml.TypeResolvers
{
    public interface IXmlTypeResolver
    {
        string GetTypeName(Type valueType);

        Type ResolveTypeName(Type rootType, string typeName);
    }
}