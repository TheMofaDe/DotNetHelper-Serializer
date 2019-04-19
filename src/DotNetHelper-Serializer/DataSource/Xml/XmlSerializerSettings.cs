using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;
using DotNetHelper_Serializer.DataSource.Xml.Contracts;
using DotNetHelper_Serializer.DataSource.Xml.Converters;
using DotNetHelper_Serializer.DataSource.Xml.Converters.Basics;
using DotNetHelper_Serializer.DataSource.Xml.Converters.Collections;
using DotNetHelper_Serializer.DataSource.Xml.Converters.Objects;
using DotNetHelper_Serializer.DataSource.Xml.Converters.Specialized;
using DotNetHelper_Serializer.DataSource.Xml.TypeResolvers;

namespace DotNetHelper_Serializer.DataSource.Xml
{
    public sealed class XmlSerializerSettings
    {
        private static readonly XmlConverterCollection DefaultConverters;

        private readonly ConcurrentDictionary<Type, XmlTypeContext> typeContextCache;
        private readonly XmlConverterCollection converters;
        private readonly List<XmlNamespace> namespaces;
        private bool omitXmlDeclaration;
        private bool indent;
        private string indentChars;
        private Encoding encoding;
        private IXmlTypeResolver typeResolver;
        private IXmlContractResolver contractResolver;
        private CultureInfo cultureInfo;
        private XmlName typeAttributeName;
        private XmlName nullAttributeName;
        private XmlReaderSettings readerSettings;
        private XmlWriterSettings writerSettings;

        static XmlSerializerSettings()
        {
            DefaultConverters = new XmlConverterCollection
            {
                new XmlStringConverter(),
                new XmlBooleanConverter(),
                new XmlCharConverter(),
                new XmlByteConverter(),
                new XmlSByteConverter(),
                new XmlInt16Converter(),
                new XmlUInt16Converter(),
                new XmlInt32Converter(),
                new XmlUInt32Converter(),
                new XmlInt64Converter(),
                new XmlUInt64Converter(),
                new XmlSingleConverter(),
                new XmlDoubleConverter(),
                new XmlDecimalConverter(),
                new XmlEnumConverter(),
                new XmlGuidConverter(),
                new XmlDateTimeConverter(),
                new XmlTimeSpanConverter(),
                new XmlDateTimeOffsetConverter(),
                new XmlArrayConverter(),
                new XmlListConverter(),
                new XmlDictionaryConverter(),
                new XmlKeyValuePairConverter(),
                new XmlNullableConverter(),
                new XmlEnumerableConverter(),
                new XmlObjectConverter()
            };
        }

        public XmlSerializerSettings()
        {
            converters = new XmlConverterCollection();
            converters.CollectionChanged += (sender, ea) => typeContextCache.Clear();
            typeContextCache = new ConcurrentDictionary<Type, XmlTypeContext>();
            typeResolver = new XmlTypeResolver();
            contractResolver = new XmlContractResolver();
            cultureInfo = CultureInfo.InvariantCulture;
            typeAttributeName = new XmlName("type", XmlNamespace.Xsi);
            nullAttributeName = new XmlName("nil", XmlNamespace.Xsi);
            encoding = Encoding.UTF8;
            TypeHandling = XmlTypeHandling.Auto;
            NullValueHandling = XmlNullValueHandling.Ignore;
            DefaultValueHandling = XmlDefaultValueHandling.Include;
            omitXmlDeclaration = false;
            indentChars = "  ";
            indent = false;
            namespaces = new List<XmlNamespace>
            {
                new XmlNamespace("xsi", XmlNamespace.Xsi)
            };
        }

        public XmlTypeHandling TypeHandling { get; set; }

        public XmlNullValueHandling NullValueHandling { get; set; }

        public XmlDefaultValueHandling DefaultValueHandling { get; set; }

        public bool OmitXmlDeclaration
        {
            get => omitXmlDeclaration;

            set
            {
                omitXmlDeclaration = value;
                readerSettings = null;
            }
        }

        public bool Indent
        {
            get => indent;

            set
            {
                indent = value;
                readerSettings = null;
            }
        }

        public string IndentChars
        {
            get => indentChars;

            set
            {
                indentChars = value ?? throw new ArgumentNullException(nameof(value));
                readerSettings = null;
            }
        }

        public XmlName TypeAttributeName
        {
            get => typeAttributeName;

            set => typeAttributeName = value ?? throw new ArgumentNullException(nameof(value));
        }

        public XmlName NullAttributeName
        {
            get => nullAttributeName;

            set => nullAttributeName = value ?? throw new ArgumentNullException(nameof(value));
        }

        public CultureInfo Culture
        {
            get => cultureInfo;

            set => cultureInfo = value ?? throw new ArgumentNullException(nameof(value));
        }

        public IXmlTypeResolver TypeResolver
        {
            get => typeResolver;

            set
            {
                typeResolver = value ?? throw new ArgumentNullException(nameof(value));
                typeContextCache.Clear();
            }
        }

        public IXmlContractResolver ContractResolver
        {
            get => contractResolver;

            set
            {
                contractResolver = value ?? throw new ArgumentNullException(nameof(value));
                typeContextCache.Clear();
            }
        }

        public Encoding Encoding
        {
            get => encoding;

            set
            {
                encoding = value ?? throw new ArgumentNullException(nameof(value));
                readerSettings = null;
            }
        }

        public ICollection<XmlNamespace> Namespaces => namespaces;

        public ICollection<IXmlConverter> Converters => converters;

        public XmlWriterSettings GetWriterSettings()
        {
            var settings = writerSettings;

            if (settings == null)
            {
                settings = new XmlWriterSettings
                {
                    OmitXmlDeclaration = OmitXmlDeclaration,
                    Indent = Indent,
                    Encoding = Encoding,
                    IndentChars = IndentChars,
                    CloseOutput = false
                };

                writerSettings = settings;
            }

            return settings;
        }

        public XmlReaderSettings GetReaderSettings()
        {
            var settings = readerSettings;

            if (settings == null)
            {
                settings = new XmlReaderSettings
                {
                    IgnoreComments = true,
                    IgnoreProcessingInstructions = true,
                    IgnoreWhitespace = true,
                    CloseInput = false
                };

                readerSettings = settings;
            }

            return settings;
        }

        public XmlTypeContext GetTypeContext(Type valueType)
        {

            if (!typeContextCache.TryGetValue(valueType, out XmlTypeContext context))
            {
                context = CreateTypeContext(valueType, context);
            }

            return context;
        }

        private static IXmlConverter GetConverter(XmlContract contract, IXmlConverter converter)
        {
            if (converter == null)
            {
                return null;
            }


            if (converter is IXmlConverterFactory factory)
            {
                converter = factory.CreateConverter(contract);
            }

            return converter;
        }

        private XmlTypeContext CreateTypeContext(Type valueType, XmlTypeContext context)
        {
            IXmlConverter readConverter = null;
            IXmlConverter writeConverter = null;

            foreach (var converter in converters.Concat(DefaultConverters))
            {
                if (readConverter == null && converter.CanRead(valueType))
                {
                    readConverter = converter;

                    if (writeConverter != null)
                    {
                        break;
                    }
                }

                if (writeConverter == null && converter.CanWrite(valueType))
                {
                    writeConverter = converter;

                    if (readConverter != null)
                    {
                        break;
                    }
                }
            }

            var contract = contractResolver.ResolveContract(valueType);

            readConverter = GetConverter(contract, readConverter);
            writeConverter = GetConverter(contract, writeConverter);

            context = new XmlTypeContext(contract, readConverter, writeConverter);
            typeContextCache.TryAdd(valueType, context);
            return context;
        }
    }
}