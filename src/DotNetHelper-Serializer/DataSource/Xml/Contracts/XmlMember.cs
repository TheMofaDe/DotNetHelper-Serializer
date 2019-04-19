﻿using System;
using System.Collections.Generic;
using System.Linq;
using DotNetHelper_Serializer.Extension;

namespace DotNetHelper_Serializer.DataSource.Xml.Contracts
{
    public class XmlMember
    {
        private static readonly List<XmlKnownType> EmptyKnownTypes = new List<XmlKnownType>();

        private readonly List<XmlKnownType> knownTypes;

        internal XmlMember(
            Type valueType,
            XmlName name,
            XmlMappingType mappingType = XmlMappingType.Element,
            XmlTypeHandling? typeHandling = null,
            XmlNullValueHandling? nullValueHandling = null,
            XmlDefaultValueHandling? defaultValueHandling = null,
            object defaultValue = null,
            XmlItem item = null,
            IEnumerable<XmlKnownType> knownTypes = null)
        {
            if (valueType == null)
            {
                throw new ArgumentNullException(nameof(valueType));
            }

            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            var isFinalType = valueType.IsFinalType();

            this.ValueType = valueType;
            this.Name = name;
            this.TypeHandling = typeHandling;
            this.DefaultValueHandling = defaultValueHandling;
            this.DefaultValue = defaultValue;
            IsOpenType = !isFinalType && valueType.IsVisible;
            this.Item = item;
            this.NullValueHandling = nullValueHandling;
            this.MappingType = mappingType;

            if (knownTypes != null)
            {
                var count = knownTypes.Count();

                if (count > 0)
                {
                    if (mappingType != XmlMappingType.Element)
                    {
                        throw new ArgumentException("Known types may be set only for XML Element.", nameof(knownTypes));
                    }

                    if (isFinalType)
                    {
                        throw new ArgumentException("Known types cannot be set for final value type.", nameof(knownTypes));
                    }

                    this.knownTypes = new List<XmlKnownType>(count);

                    foreach (var knownType in knownTypes)
                    {
                        if (valueType == knownType.ValueType)
                        {
                            throw new ArgumentException(string.Format("Known type \"{0}\" cannot be equal to the value type.", valueType), nameof(knownTypes));
                        }

                        if (!valueType.IsAssignableFrom(knownType.ValueType))
                        {
                            throw new ArgumentException(string.Format("Known type \"{0}\" must be inherits from \"{1}\".", knownType.ValueType, valueType), nameof(knownTypes));
                        }

                        this.knownTypes.Add(knownType);
                    }
                }
            }
        }

        public Type ValueType { get; }

        public XmlName Name { get; }

        public XmlMappingType MappingType { get; }

        public XmlTypeHandling? TypeHandling { get; }

        public XmlNullValueHandling? NullValueHandling { get; }

        public XmlDefaultValueHandling? DefaultValueHandling { get; }

        public object DefaultValue { get; }

        public XmlItem Item { get; }

        public IReadOnlyList<XmlKnownType> KnownTypes => knownTypes ?? EmptyKnownTypes;

        internal bool IsOpenType { get; }

        internal XmlMember ResolveMember(Type valueType)
        {
            if (knownTypes != null)
            {
                foreach (var item in knownTypes)
                {
                    if (item.ValueType == valueType)
                    {
                        return item;
                    }
                }
            }

            return this;
        }
    }
}