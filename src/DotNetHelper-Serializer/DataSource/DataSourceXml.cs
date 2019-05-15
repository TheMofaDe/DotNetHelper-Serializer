using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Xml.Linq;

using DotNetHelper_Contracts.Extension;
using DotNetHelper_IO;
using DotNetHelper_IO.Enum;
using DotNetHelper_Serializer.DataSource.Xml;
using DotNetHelper_Serializer.DataSource.Xml.Contracts;
using DotNetHelper_Serializer.Interface;
using Newtonsoft.Json;

namespace DotNetHelper_Serializer.DataSource
{
    /// <summary>
    /// Class DataSourceXml. This class cannot be inherited.
    /// </summary>
    /// <seealso cref="IXmlSerializer" />
    public sealed class DataSourceXml : IXmlSerializer
    {

        /// <summary>
        /// Enum XmlOptions
        /// </summary>
        public enum XmlOptions
        {
            /// <summary>
            /// The XML declaration
            /// </summary>
            XmlDeclaration,
            /// <summary>
            /// The namespaces
            /// </summary>
            Namespaces,
            /// <summary>
            /// The attribute name
            /// </summary>
            AttributeName
        }

        /// <summary>
        /// Enum SettingsType
        /// </summary>
        public enum SettingsType
        {
            /// <summary>
            /// The data transfer
            /// </summary>
            DataTransfer = 1,
            /// <summary>
            /// When Serializing A PreDefine Settings Will Be Used To Make The Xml Human Editable Friendly
            /// </summary>
            ManualEditing = 2,
            /// <summary>
            /// When Performance Matters But Their Are Times When You Have To Open This Sucker Up
            /// </summary>
            Both = 3
        }
        /// <summary>
        /// Gets or sets the settings.
        /// </summary>
        /// <value>The settings.</value>
        public XmlSerializerSettings Settings { get; set; }

        // public DataSourceXml(): this(new XmlSerializerSettings())
        /// <summary>
        /// Initializes a new instance of the <see cref="DataSourceXml"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <exception cref="ArgumentOutOfRangeException">type - null</exception>
        public DataSourceXml(SettingsType type)
        {
            switch (type)
            {
                case SettingsType.DataTransfer:
                    Settings = new XmlSerializerSettings() { Indent = false, NullValueHandling = XmlNullValueHandling.Ignore, OmitXmlDeclaration = true };
                    break;
                case SettingsType.ManualEditing:
                    Settings = new XmlSerializerSettings() { Indent = true, NullValueHandling = XmlNullValueHandling.Include, OmitXmlDeclaration = false };
                    break;
                case SettingsType.Both:
                    Settings = new XmlSerializerSettings() { Indent = true, NullValueHandling = XmlNullValueHandling.Ignore, OmitXmlDeclaration = false };
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataSourceXml"/> class.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <exception cref="ArgumentNullException">settings</exception>
        public DataSourceXml(XmlSerializerSettings settings)
        {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }


        //   public string RemoveFromXml(string xml, XmlOptions options)
        //   {
        //       var settings = new XmlSerializerSettings();
        //       switch (options)
        //       {
        //           case XmlOptions.XmlDeclaration:
        //               settings.OmitXmlDeclaration = true;
        //               break;
        //           case XmlOptions.Namespaces:
        //               settings.Namespaces.Clear();
        //               break;
        //           case XmlOptions.AttributeName:  
        //               break;
        //           default:
        //               throw new ArgumentOutOfRangeException(nameof(options), options, null);
        //       }
        //       var context = new XmlSerializationContext(settings);
        //       using (var stream = new StringReader(xml))
        //       {
        //           using (var reader = new XmlTextReader(stream))
        //           {
        //               context.ReadXml(reader,new XmlMember(),new XmlTypeContext()  )
        //           }
        //       }
        //     
        //       return t
        //   }
        //   public string AddToXml(string xml, XmlOptions options)
        //   {
        //       var settings = new XmlSerializerSettings();
        //       switch (options)
        //       {
        //           case XmlOptions.XmlDeclaration:
        //               settings.OmitXmlDeclaration = true;
        //               break;
        //           case XmlOptions.Namespaces:
        //               settings.Namespaces.Clear();
        //               break;
        //           case XmlOptions.AttributeName:
        //               break;
        //           default:
        //               throw new ArgumentOutOfRangeException(nameof(options), options, null);
        //       }
        //   }


        /// <summary>
        /// Determines whether this instance can serialize the specified value type.
        /// </summary>
        /// <param name="valueType">Type of the value.</param>
        /// <param name="throwOnCant">if set to <c>true</c> [throw on cant].</param>
        /// <returns><c>true</c> if this instance can serialize the specified value type; otherwise, <c>false</c>.</returns>
        /// <exception cref="SerializationException"></exception>
        /// <inheritdoc />
        public bool? CanSerialize(Type valueType, bool throwOnCant = false)
        {
            bool? result = null;
            try
            {
                result = Settings.GetTypeContext(valueType).WriteConverter != null;
                if (throwOnCant && !result.GetValueOrDefault(false))
                {
                    throw new SerializationException($"The Following Type {valueType.Name} Can Not Be Deserialize");
                }
                return result;
            }
            catch (Exception)
            {

                return result;
            }

        }
        /// <summary>
        /// Determines whether this instance can deserialize the specified value type.
        /// </summary>
        /// <param name="valueType">Type of the value.</param>
        /// <param name="throwOnCant">if set to <c>true</c> [throw on cant].</param>
        /// <returns><c>true</c> if this instance can deserialize the specified value type; otherwise, <c>false</c>.</returns>
        /// <exception cref="SerializationException"></exception>
        /// <inheritdoc />
        public bool CanDeserialize(Type valueType, bool throwOnCant = false)
        {
            var result = Settings.GetTypeContext(valueType).ReadConverter != null;
            if (throwOnCant && !result)
            {
                throw new SerializationException($"The Following Type {valueType.Name} Can Not Be Deserialize");
            }
            return result;
        }
        /// <summary>
        /// Serializes to XML writer.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="valueType">Type of the value.</param>
        /// <param name="value">The value.</param>
        /// <inheritdoc />
        public void SerializeToXmlWriter(XmlWriter writer, Type valueType, object value)
        {
            writer.IsNullThrow(nameof(writer));
            valueType.IsNullThrow(nameof(valueType));
            value.IsNullThrow(nameof(value));
            var canSerialize = CanSerialize(valueType, true);
            using (writer)
            {
                var context = new XmlSerializationContext(Settings);
                context.Serialize(writer, value, valueType);
            }
            writer.Flush();


        }
        /// <summary>
        /// Serializes to XML writer.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="writer">The writer.</param>
        /// <param name="value">The value.</param>
        /// <inheritdoc />
        public void SerializeToXmlWriter<T>(XmlWriter writer, T value) where T : class
        {
            writer.IsNullThrow(nameof(writer));

            value.IsNullThrow(nameof(value));
            var canSerialize = CanSerialize(typeof(T), true);
            using (writer)
            {
                var context = new XmlSerializationContext(Settings);
                context.Serialize(writer, value, value.GetType());
            }
            writer.Flush();
        }

        /// <summary>
        /// Serializes to text writer.
        /// </summary>
        /// <param name="output">The output.</param>
        /// <param name="valueType">Type of the value.</param>
        /// <param name="value">The value.</param>
        public void SerializeToTextWriter(TextWriter output, Type valueType, object value)
        {
            output.IsNullThrow(nameof(output));
            valueType.IsNullThrow(nameof(valueType));
            value.IsNullThrow(nameof(value));
            var canSerialize = CanSerialize(valueType, true);
            var writer = XmlWriter.Create(output, Settings.GetWriterSettings());
            SerializeToXmlWriter(writer, valueType, value);

        }
        /// <summary>
        /// Serializes to text writer.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="output">The output.</param>
        /// <param name="value">The value.</param>
        /// <inheritdoc />
        public void SerializeToTextWriter<T>(TextWriter output, T value) where T : class
        {
            SerializeToTextWriter(output, value.GetType(), value);
        }



        /// <summary>
        /// Serializes to json.
        /// </summary>
        /// <param name="xml">The XML.</param>
        /// <param name="jsonSerializer">The json serializer.</param>
        /// <returns>System.String.</returns>
        /// <inheritdoc />
        public string SerializeToJson(string xml, IJsonSerializer jsonSerializer)
        {
            var doc = new XmlDocument();
            doc.LoadXml(xml);
            return JsonConvert.SerializeXmlNode(doc, jsonSerializer.Settings.Formatting);
        }
        /// <summary>
        /// XMLs the string to x document.
        /// </summary>
        /// <param name="xml">The XML.</param>
        /// <returns>XDocument.</returns>
        /// <inheritdoc />
        public XDocument XmlStringToXDocument(string xml)
        {
            using (var stream = new StringReader(xml))
            {
                var doc = XDocument.Load(stream);
                return doc;
            }
        }
        /// <summary>
        /// XMLs the string to XML document.
        /// </summary>
        /// <param name="xml">The XML.</param>
        /// <returns>XmlDocument.</returns>
        /// <inheritdoc />
        public XmlDocument XmlStringToXmlDocument(string xml)
        {
            using (var stream = new StringReader(xml))
            {
                var doc = new XmlDocument();
                doc.Load(stream);
                return doc;
            }
        }
        /// <summary>
        /// Gets the XML from document.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <returns>System.String.</returns>
        /// <inheritdoc />
        public string GetXmlFromDocument(XmlDocument document)
        {
            using (var stringWriter = new StringWriter())
            using (var xmlTextWriter = XmlWriter.Create(stringWriter))
            {
                document.WriteTo(xmlTextWriter);
                xmlTextWriter.Flush();
                return stringWriter.GetStringBuilder().ToString();
            }
        }
        /// <summary>
        /// Gets the XML from document.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <returns>System.String.</returns>
        /// <inheritdoc />
        public string GetXmlFromDocument(XDocument document)
        {
            using (var stringWriter = new StringWriter())
            using (var xmlTextWriter = XmlWriter.Create(stringWriter))
            {
                document.WriteTo(xmlTextWriter);
                xmlTextWriter.Flush();
                return stringWriter.GetStringBuilder().ToString();
            }
        }

        /// <summary>
        /// Serializes to stream.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">The obj.</param>
        /// <param name="stream">The stream.</param>
        /// <param name="bufferSize"></param>
        /// <param name="leaveStreamOpen"></param>
        /// <inheritdoc />
        public void SerializeToStream<T>(T obj, Stream stream, int bufferSize = 1024, bool leaveStreamOpen = false) where T : class
        {

            SerializeToStream(obj, obj.GetType(), stream);
        }

        /// <summary>
        /// Serializes to stream.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <param name="type">The type.</param>
        /// <param name="stream">The stream.</param>
        /// <inheritdoc />
        public void SerializeToStream(object obj, Type type, Stream stream, int bufferSize = 1024, bool leaveStreamOpen = false)
        {
            stream.IsNullThrow(nameof(stream));
            type.IsNullThrow(nameof(type));
            var canSerialize = CanSerialize(type, true);
            var writer = XmlWriter.Create(stream, Settings.GetWriterSettings());
            SerializeToXmlWriter(writer, type, obj);
        }

        /// <summary>
        /// Serializes to file.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">The object.</param>
        /// <param name="file">The file.</param>
        /// <param name="mode">The mode.</param>
        /// <inheritdoc />
        public void SerializeToFile<T>(T obj, string file, FileOption mode) where T : class
        {
            SerializeToFile(obj, obj.GetType(), file, mode);
        }

        /// <summary>
        /// Serializes to file.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">The list.</param>
        /// <param name="fullFilePath">The full file path.</param>
        /// <param name="option">The option.</param>
        /// <inheritdoc />
        public void SerializeToFile<T>(IEnumerable<T> list, string fullFilePath, FileOption option) where T : class
        {
            list.IsNullThrow(nameof(list));
            fullFilePath.IsNullThrow(nameof(fullFilePath));
            option.IsNullThrow(nameof(option));
            var canSerialize = CanSerialize(typeof(T), true);
            var file = new FileObject(fullFilePath);
            if (option == FileOption.DoNothingIfExist)
            {
                if (file.Exist == true) return;
            }
            using (var stream = file.GetFileStream(option))
            {
                SerializeToStream(list, stream);
            }
        }

        /// <summary>
        /// Serializes to file.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="type">The type.</param>
        /// <param name="fullFilePath">The full file path.</param>
        /// <param name="option">The option.</param>
        /// <inheritdoc />
        public void SerializeToFile(object obj, Type type, string fullFilePath, FileOption option)
        {
            obj.IsNullThrow(nameof(obj));
            type.IsNullThrow(nameof(type));
            fullFilePath.IsNullThrow(nameof(fullFilePath));
            option.IsNullThrow(nameof(option));


            var canSerialize = CanSerialize(type, true);
            var file = new FileObject(fullFilePath);
            if (option == FileOption.DoNothingIfExist)
            {
                if (file.Exist == true) return;
            }

            using (var stream = file.GetFileStream(option))
            {

                SerializeToStream(obj, stream);
            }

        }

        public void SerializeToFile(dynamic obj, string fullFilePath, FileOption mode)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Serializes to string.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <returns>System.String.</returns>
        /// <inheritdoc />
        public string SerializeToString(object obj)
        {
            CanSerialize(obj.GetType(), true);
            using (var sw = new StringWriter())
            {
                var writer = XmlWriter.Create(sw, Settings.GetWriterSettings());
                using (writer)
                {
                    var context = new XmlSerializationContext(Settings);
                    context.Serialize(writer, obj, obj.GetType());
                }
                return sw.ToString();
            }
        }

        /// <summary>
        /// Serializes to string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">The obj.</param>
        /// <returns>System.String.</returns>
        public string SerializeToString<T>(T obj) where T : class
        {
            var canSerialize = CanSerialize(typeof(T), true);
            using (var sw = new StringWriter())
            {
                var writer = XmlWriter.Create(sw, Settings.GetWriterSettings());
                using (writer)
                {
                    var context = new XmlSerializationContext(Settings);
                    context.Serialize(writer, obj, typeof(T));
                }
                return sw.ToString();
            }
        }

        /// <summary>
        /// Deserializes from file.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="file">The file.</param>
        /// <returns>System.Object.</returns>
        /// <inheritdoc />
        public object DeserializeFromFile(Type type, string fullFilePath)
        {
            CanDeserialize(type, true);
            var a = new FileObject(fullFilePath);
            return DeserializeFromStream(a.ReadFileToStream(), type);

        }
        /// <summary>
        /// Deserializes from file.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="file">The file.</param>
        /// <returns>T.</returns>
        /// <inheritdoc />
        public T DeserializeFromFile<T>(string file) where T : class
        {
            return (T)DeserializeFromFile(typeof(T), file);
        }

        // public List<T> DeserializeFromFilet<T>(string fullFilePath) where T : IEnumerable<T>
        // {
        //     return DeserializeFromFile<IEnumerable<T>>(fullFilePath) as List<T>;
        // }

        public dynamic DeserializeFromFile(string fullFilePath)
        {
            var doc = XDocument.Load(fullFilePath);
            var jsonText = JsonConvert.SerializeXNode(doc);
            return JsonConvert.DeserializeObject<ExpandoObject>(jsonText);
        }

        public IEnumerable<dynamic> DeserializeListFromFile(string fullFilePath)
        {
            var doc = XDocument.Load(fullFilePath);
            var jsonText = JsonConvert.SerializeXNode(doc);
            return JsonConvert.DeserializeObject<IEnumerable<dynamic>>(jsonText);
        }

        public IEnumerable<dynamic> DeserializeToList(string xml)
        {
            var doc = XDocument.Parse(xml);
            var jsonText = JsonConvert.SerializeXNode(doc);
            return JsonConvert.DeserializeObject<IEnumerable<dynamic>>(jsonText);
        }


        public IEnumerable<T> DeserializeListFromFile<T>(string fullFilePath) where T : class
        {
            //var doc = XDocument.Load(fullFilePath);
            //var jsonText = JsonConvert.SerializeXNode(doc);
            //return JsonConvert.DeserializeObject<List<T>>(jsonText);


            CanDeserialize(typeof(T), true);
            var a = new FileObject(fullFilePath);
            var data = DeserializeFromStream(a.ReadFileToStream(), typeof(List<T>));
            return (IEnumerable<T>)data;
            //  var data2 = DeserializeFromFile(typeof(T), fullFilePath);
            //    var data3 = DeserializeFromFile(typeof(List<T>), fullFilePath);




            // return data as IEnumerable<T>;
            //var t = typeof(T);
            //if(t.IsInterface)
            //    t = t.GetType();
            ////////var a = new FileObject(fullFilePath);
            ////////return DeserializeFromStream(a.ReadFileToStream(), t) as IEnumerable<T>;

            ////////var doc = XDocument.Load(fullFilePath);
            ////////var jsonText = JsonConvert.SerializeXNode(doc);
            ////////if (typeof(T).IsInterface)
            ////////{
            ////////    var badTest = typeof(T);
            ////////    var test = typeof(T).GetType();
            ////////    return JsonConvert.DeserializeObject(jsonText,test) as IEnumerable<T>;
            ////////}


            ////////return JsonConvert.DeserializeObject<IEnumerable<T>>(jsonText);


            //var context = new XmlSerializationContext(Settings);
            //using (var fileStream = new FileObject(fullFilePath).ReadFileToStream())
            //{
            //    using (var reader = new XmlTextReader(fileStream))
            //    {
            //        yield return context.Deserialize(reader, typeof(IEnumerable<T>)) as T;
            //    }

            //}





            //    var doc = XDocument.Load(fullFilePath);
            //    using (var reader = XmlReader.Create(fullFilePath, Settings.GetReaderSettings()))
            //    {
            //        var xmlSerializer = new XmlSerializer(typeof(IEnumerable<T>));
            //        return (IEnumerable<T>)xmlSerializer.Deserialize(reader);
            //    }



        }

        /// <summary>
        /// Deserializes from stream.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream">The stream.</param>
        /// <returns>``0.</returns>
        /// <inheritdoc />
        public T DeserializeFromStream<T>(Stream stream)
        {
            return (T)DeserializeFromStream(stream, typeof(T));
        }
        /// <summary>
        /// Deserializes from stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="type">The type.</param>
        /// <returns>System.Object.</returns>
        /// <inheritdoc />
        public object DeserializeFromStream(Stream stream, Type type)
        {
            CanDeserialize(type, true);
            var context = new XmlSerializationContext(Settings);
            if (stream.Position == stream.Length) stream.ResetPosition();
            using (var reader = new XmlTextReader(stream))
            {
                return context.Deserialize(reader, type);
            }
        }
        /// <summary>
        /// Deserializes from string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="text">The text.</param>
        /// <returns>T.</returns>
        /// <inheritdoc />
        public T DeserializeFromString<T>(string text) where T : class
        {
            return (T)DeserializeFromString(text, typeof(T));
        }

        /// <inheritdoc />
        /// <summary>
        /// Deserializes from string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="content">The content.</param>
        /// <returns>List Of T</returns>
        public List<T> DeserializeToList<T>(string content) where T : class
        {
            return (List<T>)DeserializeFromString(content, typeof(List<T>));
        }

        /// <summary>
        /// Deserializes from string.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="type">The type.</param>
        /// <returns>System.Object.</returns>
        /// <inheritdoc />
        public object DeserializeFromString(string content, Type type)
        {
            CanDeserialize(type, true);
            var context = new XmlSerializationContext(Settings);
            using (var stream = new StringReader(content))
            {
                using (var reader = new XmlTextReader(stream))
                {
                    return context.Deserialize(reader, type);
                }
            }
        }

        /// <summary>
        /// Deserializes to c sharp class.
        /// </summary>
        /// <param name="xml">The XML.</param>
        /// <param name="className">Name of the class.</param>
        /// <returns>System.String.</returns>
        public string DeserializeToCSharpClass(string xml, string className = null)
        {
            var doc = XDocument.Parse(xml); //or XDocument.Load(path)
            string jsonText = JsonConvert.SerializeXNode(doc);
            dynamic dyn = JsonConvert.DeserializeObject<ExpandoObject>(jsonText);
            className = className ?? doc.Root.Name.LocalName;
            var props = (IDictionary<string, object>)dyn;

            var sb = new StringBuilder();

            className = string.IsNullOrEmpty(className) ? "DynamicClass" : className;
            sb.AppendLine($"public class {className} {{ ");
            for (var i = 0; i < props.Count; i++)
            {
                sb.AppendLine($"public {props.Values.ToList()[i].GetType().Name} {props.Keys.ToList()[i]} {{ get; set; }} ");
            }
            sb.AppendLine($"}}");

            return sb.ToString();
        }

        public TypeCode GetTypeCode()
        {
            throw new NotImplementedException();
        }

        public bool ToBoolean(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public char ToChar(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public sbyte ToSByte(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public byte ToByte(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public short ToInt16(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public ushort ToUInt16(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public int ToInt32(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public uint ToUInt32(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public long ToInt64(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public ulong ToUInt64(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public float ToSingle(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public double ToDouble(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public decimal ToDecimal(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public DateTime ToDateTime(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public string ToString(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public object ToType(Type conversionType, IFormatProvider provider)
        {
            if (conversionType == typeof(IXmlSerializer)) return this;
            throw new NotImplementedException();
        }
    }
}