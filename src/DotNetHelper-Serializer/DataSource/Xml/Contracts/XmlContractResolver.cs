﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using DotNetHelper_Serializer.DataSource.Xml.Contracts.Builders;
using DotNetHelper_Serializer.Extension;

namespace DotNetHelper_Serializer.DataSource.Xml.Contracts
{
    public class XmlContractResolver : IXmlContractResolver
    {
        private readonly Func<string, string> nameResolver;
        private readonly bool ignoreSystemAttributes;

        public XmlContractResolver(bool ignoreSystemAttributes = false)
            : this(NamingConventions.Ignore, ignoreSystemAttributes)
        {
        }

        public XmlContractResolver(Func<string, string> nameResolver, bool ignoreSystemAttributes = false)
        {
            this.ignoreSystemAttributes = ignoreSystemAttributes;
            this.nameResolver = nameResolver ?? throw new ArgumentNullException(nameof(nameResolver));
        }

        public virtual XmlContract ResolveContract(Type valueType)
        {
            if (valueType == null)
            {
                throw new ArgumentNullException(nameof(valueType));
            }

            var name = ResolveContractName(valueType);

            if (IsBasicContract(valueType))
            {
                return new XmlContract(valueType, name);
            }
            else if (valueType.IsEnum)
            {
                return new XmlEnumContract(valueType, name, ResolveEnumItems(valueType));
            }

            var properties = GetProperties(valueType)
                .Select(x => ResolveProperty(x))
                .Where(x => x != null);

            return new XmlObjectContract(valueType, name, properties, null, ResolveItem(valueType));
        }

        protected virtual XmlName ResolveName(Type valueType)
        {
            return ResolveName(valueType, valueType.GetShortName());
        }

        protected virtual XmlName ResolveName(Type valueType, string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (name.Length == 0)
            {
                throw new ArgumentException("Name is empty.", nameof(name));
            }

            return new XmlName(GetLocalName(name));
        }

        protected virtual XmlName ResolveContractName(Type valueType)
        {
            if (!ignoreSystemAttributes)
            {
                var rootAttribute = valueType
                    .GetCustomAttributes(typeof(XmlRootAttribute), false)
                    .Cast<XmlRootAttribute>().FirstOrDefault();

                if (rootAttribute != null)
                {
                    return new XmlName(rootAttribute.ElementName, rootAttribute.Namespace);
                }
            }

            return ResolveName(valueType);
        }

        protected virtual string GetLocalName(string name)
        {
            return nameResolver(name);
        }

        protected virtual string GetEnumItemName(Type valueType, string name)
        {
            return nameResolver(name);
        }

        protected virtual IEnumerable<PropertyInfo> GetProperties(Type valueType)
        {
            return valueType
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(x => x.GetIndexParameters().Length == 0);
        }

        protected virtual XmlItem ResolveItem(Type valueType)
        {
            if (valueType == typeof(string))
            {
                return null;
            }

            var itemType = valueType.GetEnumerableItemType();

            if (itemType == null)
            {
                return null;
            }

            return new XmlItem(itemType, ResolveName(itemType));
        }

        protected virtual IEnumerable<XmlEnumItem> ResolveEnumItems(Type valueType)
        {
            var fields = valueType.GetFields(BindingFlags.Public | BindingFlags.Static);
            var count = fields.Length;
            var items = new XmlEnumItem[count];

            for (int i = 0; i < count; i++)
            {
                var field = fields[i];
                var name = GetEnumItemName(valueType, field.Name);
                var value = Convert.ToInt64(field.GetRawConstantValue());

                if (!ignoreSystemAttributes)
                {
                    var xmlEnum = field
                        .GetCustomAttributes(typeof(XmlEnumAttribute), false)
                        .Cast<XmlEnumAttribute>()
                        .FirstOrDefault();

                    if (xmlEnum != null)
                    {
                        name = xmlEnum.Name;
                    }
                }

                items[i] = new XmlEnumItem(value, name);
            }

            return items;
        }

        protected virtual XmlProperty ResolveProperty(PropertyInfo propertyInfo)
        {
            var propertyBuilder = new XmlPropertyBuilder(propertyInfo)
            {
                Name = ResolveName(propertyInfo.PropertyType, propertyInfo.Name)
            };

            if (!SetPropertyAttributes(propertyBuilder))
            {
                return null;
            }

            return propertyBuilder.Build();
        }

        private static bool IsBasicContract(Type valueType)
        {
            return valueType.IsPrimitive || valueType == typeof(string);
        }

        private bool SetPropertyAttributes(XmlPropertyBuilder propertyBuilder)
        {
            if (ignoreSystemAttributes)
            {
                return true;
            }

            var attributes = XmlPropertyAttributes.GetAttributes(propertyBuilder.PropertyInfo);

            if (attributes.Ignore != null)
            {
                return false;
            }

            var propertyName = propertyBuilder.Name;
            var propertyType = propertyBuilder.PropertyInfo.PropertyType;
            var item = ResolveItem(propertyType);

            if (attributes.Elements != null)
            {
                foreach (var attribute in attributes.Elements)
                {
                    var name = propertyName.Create(attribute.ElementName, attribute.Namespace);

                    if (attribute.Type == null || attribute.Type == propertyType)
                    {
                        propertyBuilder.SetName(name)
                            .SetMappingType(XmlMappingType.Element)
                            .SetNullable(attribute.IsNullable)
                            .SetOrder(attribute.Order);
                    }
                    else if (propertyType.IsAssignableFrom(attribute.Type))
                    {
                        propertyBuilder.SetKnownType(
                            attribute.Type,
                            x => x.SetName(name).SetNullable(attribute.IsNullable));
                    }

                    if (item != null)
                    {
                        name = item.Name.Create(attribute.ElementName, attribute.Namespace);

                        if (propertyBuilder.Item == null)
                        {
                            propertyBuilder.SetItem(item.ValueType, item.Name);
                        }

                        if (attribute.Type == null || attribute.Type == item.ValueType)
                        {
                            propertyBuilder.Item.SetName(name);
                        }
                        else if (item.ValueType.IsAssignableFrom(attribute.Type))
                        {
                            propertyBuilder.Item.SetKnownType(attribute.Type, name);
                        }

                        propertyBuilder.SetCollection();
                    }
                }
            }
            else if (attributes.Attributes != null)
            {
                foreach (var attribute in attributes.Attributes)
                {
                    if (attribute.Type == null || attribute.Type == propertyType)
                    {
                        var name = propertyName.Create(attribute.AttributeName, attribute.Namespace);
                        propertyBuilder.SetName(name).SetMappingType(XmlMappingType.Attribute);
                        break;
                    }
                }
            }
            else if (attributes.Text != null)
            {
                propertyBuilder.SetMappingType(XmlMappingType.InnerText);
            }
            else if (attributes.Array != null)
            {
                var name = propertyName.Create(attributes.Array.ElementName, attributes.Array.Namespace);
                propertyBuilder.SetName(name).SetNullable(attributes.Array.IsNullable).SetCollection(false);
            }

            if (attributes.Default != null)
            {
                propertyBuilder.SetDefaultValue(attributes.Default.Value);
            }

            if (attributes.ArrayItems != null && item != null && !propertyBuilder.IsCollection)
            {
                foreach (var attribute in attributes.ArrayItems)
                {
                    var name = item.Name.Create(attribute.ElementName, attribute.Namespace);

                    if (propertyBuilder.Item == null)
                    {
                        propertyBuilder.SetItem(item.ValueType, item.Name);
                    }

                    if (attribute.Type == null || attribute.Type == item.ValueType)
                    {
                        propertyBuilder.Item.SetName(name);
                    }
                    else if (item.ValueType.IsAssignableFrom(attribute.Type))
                    {
                        propertyBuilder.Item.SetKnownType(attribute.Type, name);
                    }
                }
            }

            return true;
        }
    }
}