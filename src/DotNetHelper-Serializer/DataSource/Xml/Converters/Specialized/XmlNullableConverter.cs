﻿using System;
using System.Xml;
using DotNetHelper_Serializer.DataSource.Xml.Contracts;
using DotNetHelper_Serializer.Extension;

namespace DotNetHelper_Serializer.DataSource.Xml.Converters.Specialized
{
    public sealed class XmlNullableConverter : IXmlConverter
    {
        public bool CanRead(Type valueType)
        {
            return valueType.IsNullable().Item1;
        }

        public bool CanWrite(Type valueType)
        {
            return valueType.IsNullable().Item1;
        }

        public void WriteXml(XmlWriter writer, object value, XmlSerializationContext context)
        {
            var underlyingType = context.ValueType.GetUnderlyingNullableType();
            context.SerializeBody(writer, value, underlyingType);
        }

        public object ReadXml(XmlReader reader, XmlSerializationContext context)
        {
            var member = context.Member;
            var underlyingType = member.ValueType.GetUnderlyingNullableType();

            if (member.MappingType == XmlMappingType.Element)
            {
                if (!context.ReadValueType(reader, ref underlyingType))
                {
                    reader.Skip();
                    return null;
                }
            }

            return context.Deserialize(reader, underlyingType);
        }
    }
}