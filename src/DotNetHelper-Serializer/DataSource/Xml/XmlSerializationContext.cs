using System;
using System.Collections.Generic;
using System.Xml;
using DotNetHelper_Serializer.DataSource.Xml.Contracts;

namespace DotNetHelper_Serializer.DataSource.Xml
{
    public sealed class XmlSerializationContext
    {
        private bool initialState;
        private Dictionary<string, object> properties;
        private XmlNameRef typeNameRef;
        private XmlNameRef _nullNameRef;
        private XmlReader lastUsedReader;

        public XmlSerializationContext(XmlSerializerSettings settings)
        {
            this.Settings = settings ?? throw new ArgumentNullException("settings");
            initialState = true;
        }

        internal XmlSerializationContext(XmlSerializerSettings settings, XmlMember member, XmlContract contract)
            : this(settings)
        {
            Contract = contract ?? throw new ArgumentNullException("contract");
            Member = member ?? throw new ArgumentNullException("member");
            initialState = false;
        }

        public Type ValueType => Contract.ValueType;

        public XmlContract Contract { get; private set; }

        public XmlMember Member { get; private set; }

        public IDictionary<string, object> Properties => properties ?? (properties = new Dictionary<string, object>());

        public XmlSerializerSettings Settings { get; }

        public XmlContract GetTypeContract(Type valueType)
        {
            return Settings.GetTypeContext(valueType).Contract;
        }

        public void Serialize(XmlWriter writer, object value, Type valueType)
        {
            if (valueType == null)
            {
                throw new ArgumentNullException("valueType");
            }

            Serialize(writer, value, valueType, null);
        }

        public void Serialize(XmlWriter writer, object value, XmlMember member)
        {
            if (member == null)
            {
                throw new ArgumentNullException("memberInfo");
            }

            Serialize(writer, value, member.ValueType, member);
        }

        public object Deserialize(XmlReader reader, Type valueType)
        {
            return Deserialize(reader, valueType, null);
        }

        public object Deserialize(XmlReader reader, XmlMember member)
        {
            return Deserialize(reader, member.ValueType, member);
        }

        public void SerializeBody(XmlWriter writer, object value, Type valueType)
        {
            if (valueType == null)
            {
                throw new ArgumentNullException("valueType");
            }

            SerializeBody(writer, value, valueType, null);
        }

        public void SerializeBody(XmlWriter writer, object value, XmlMember member)
        {
            if (member == null)
            {
                throw new ArgumentNullException("member");
            }

            SerializeBody(writer, value, member.ValueType, member);
        }

        internal void WriteTypeName(XmlWriter writer, Type valueType)
        {
            var typeName = Settings.TypeResolver.GetTypeName(valueType);
            writer.WriteAttributeString(Settings.TypeAttributeName, typeName);
        }

        internal void WriteNull(XmlWriter writer, Type valueType, XmlMember member)
        {
            var nullValueHandling = Settings.NullValueHandling;

            if (member != null)
            {
                if (member.MappingType == XmlMappingType.Attribute)
                {
                    return;
                }

                nullValueHandling = member.NullValueHandling ?? nullValueHandling;
            }

            if (nullValueHandling != XmlNullValueHandling.Ignore)
            {
                if (member == null)
                {
                    member = Settings.GetTypeContext(valueType).Contract.Root;
                }

                writer.WriteStartElement(member.Name);

                if (initialState)
                {
                    initialState = false;
                    WriteNamespaces(writer);
                }

                writer.WriteAttributeString(Settings.NullAttributeName, "true");
                writer.WriteEndElement();
            }
        }

        internal bool ReadValueType(XmlReader reader, ref Type valueType)
        {
            if (reader.AttributeCount > 0)
            {
                if (!object.ReferenceEquals(lastUsedReader, reader))
                {
                    typeNameRef.Reset(Settings.TypeAttributeName, reader.NameTable);
                    _nullNameRef.Reset(Settings.NullAttributeName, reader.NameTable);
                    lastUsedReader = reader;
                }

                if (reader.MoveToFirstAttribute())
                {
                    do
                    {
                        if (_nullNameRef.Match(reader))
                        {
                            return false;
                        }
                        else if (typeNameRef.Match(reader))
                        {
                            valueType = Settings.TypeResolver.ResolveTypeName(valueType, reader.Value);
                        }
                    }
                    while (reader.MoveToNextAttribute());

                    reader.MoveToElement();
                }
            }

            return true;
        }

        internal bool TryResolveValueType(object value, ref XmlMember member, out Type valueType)
        {
            if (member.IsOpenType)
            {
                var typeHandling = member.TypeHandling ?? Settings.TypeHandling;

                if (typeHandling != XmlTypeHandling.None)
                {
                    valueType = value.GetType();
                    member = member.ResolveMember(valueType);
                    return typeHandling == XmlTypeHandling.Always || valueType != member.ValueType;
                }
            }

            valueType = member.ValueType;

            return false;
        }

        internal void WriteXml(XmlWriter writer, object value, XmlMember member, XmlTypeContext typeContext)
        {
            var lastMember = Member;
            var lastContract = Contract;

            Member = member;
            Contract = typeContext.Contract;

            typeContext.WriteXml(writer, value, this);

            Member = lastMember;
            Contract = lastContract;
        }

        internal object ReadXml(XmlReader reader, XmlMember member, XmlTypeContext typeContext)
        {
            var lastMember = Member;
            var lastContract = Contract;

            Member = member;
            Contract = typeContext.Contract;

            var value = typeContext.ReadXml(reader, this);

            Member = lastMember;
            Contract = lastContract;

            return value;
        }

        private void SerializeBody(XmlWriter writer, object value, Type memberType, XmlMember member)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }

            if (value == null)
            {
                WriteNull(writer, memberType, member);
            }
            else
            {
                var typeContext = Settings.GetTypeContext(memberType);
                WriteXml(writer, value, member ?? typeContext.Contract.Root, typeContext);
            }
        }

        private void Serialize(XmlWriter writer, object value, Type memberType, XmlMember member)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }

            if (value == null)
            {
                WriteNull(writer, memberType, member);
                return;
            }

            XmlTypeContext context = null;

            if (member == null)
            {
                context = Settings.GetTypeContext(memberType);
                member = context.Contract.Root;
            }

            var shouldWriteTypeName = TryResolveValueType(value, ref member, out Type valueType);

            if (member.DefaultValue != null)
            {
                var defaultValueHandling = member.DefaultValueHandling ?? Settings.DefaultValueHandling;

                if (defaultValueHandling == XmlDefaultValueHandling.Ignore && value.Equals(member.DefaultValue))
                {
                    return;
                }
            }

            if (context == null || context.Contract.ValueType != member.ValueType)
            {
                context = Settings.GetTypeContext(valueType);
            }

            switch (member.MappingType)
            {
                case XmlMappingType.Element:
                    writer.WriteStartElement(member.Name);

                    if (initialState)
                    {
                        initialState = false;
                        WriteNamespaces(writer);
                    }

                    if (shouldWriteTypeName)
                    {
                        WriteTypeName(writer, valueType);
                    }

                    WriteXml(writer, value, member, context);
                    writer.WriteEndElement();
                    break;

                case XmlMappingType.Attribute:
                    writer.WriteStartAttribute(member.Name);
                    WriteXml(writer, value, member, context);
                    writer.WriteEndAttribute();
                    break;

                case XmlMappingType.InnerText:
                    WriteXml(writer, value, member, context);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private object Deserialize(XmlReader reader, Type valueType, XmlMember member)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            if (valueType == null)
            {
                throw new ArgumentNullException("valueType");
            }

            if (initialState && reader.NodeType == XmlNodeType.None)
            {
                initialState = false;

                while (reader.NodeType != XmlNodeType.Element)
                {
                    if (!reader.Read())
                    {
                        return null;
                    }
                }
            }

            if (reader.NodeType == XmlNodeType.Element)
            {
                if (!ReadValueType(reader, ref valueType))
                {
                    reader.Skip();
                    return null;
                }
            }

            var typeInfo = Settings.GetTypeContext(valueType);

            if (member == null)
            {
                member = typeInfo.Contract.Root;
            }

            return ReadXml(reader, member, typeInfo);
        }

        private void WriteNamespaces(XmlWriter writer)
        {
            foreach (var item in Settings.Namespaces)
            {
                writer.WriteNamespace(item);
            }
        }
    }
}