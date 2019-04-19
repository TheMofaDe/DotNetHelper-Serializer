﻿namespace DotNetHelper_Serializer.DataSource.Xml.Contracts.Builders
{
    public interface IXmlObjectBuilder : IXmlBuilder
    {
        XmlNullValueHandling? NullValueHandling { get; set; }

        XmlDefaultValueHandling? DefaultValueHandling { get; set; }

        object DefaultValue { get; set; }
    }
}