﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using DotNetHelper_Contracts.Enum.IO;
using DotNetHelper_Contracts.Extension;
using DotNetHelper_IO;
using DotNetHelper_Serializer.Interface;
using Newtonsoft.Json;

namespace DotNetHelper_Serializer.DataSource
{
    public sealed class DataSourceJson : IJsonSerializer
    {


        public JsonSerializerSettings Settings { get; set; }

        public Encoding Encoding { get; set; }

        public DataSourceJson(JsonSerializerSettings settings = null)
        {
            Settings = settings ?? new JsonSerializerSettings();
            Encoding = Encoding ?? Encoding.UTF8;
        }
        public DataSourceJson(Encoding encoding, JsonSerializerSettings settings = null)
        {
            Settings = settings ?? new JsonSerializerSettings();
            Encoding = encoding ?? Encoding.UTF8;
        }

        /// <inheritdoc />
        public void SerializeToStream(object obj, Type type, Stream stream, int bufferSize = 1024, bool leaveStreamOpen = false)
        {

            var serializer = JsonSerializer.Create(Settings);
            using (var sw = new StreamWriter(stream, Encoding, bufferSize, leaveStreamOpen))
            using (var jsonTextWriter = new JsonTextWriter(sw))
            {
                serializer.Serialize(jsonTextWriter, obj, type);
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// Serializes to file.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <param name="file">The file.</param>
        /// <exception cref="T:System.ArgumentNullException">obj</exception>
        public void SerializeToFile(object obj, string fullFilePath, FileOption option = FileOption.DoNothingIfExist)
        {
            var file = new FileObject(fullFilePath);
            if (option == FileOption.DoNothingIfExist)
            {
                if (file.Exist == true) return;
            }
            using (Stream stream = file.GetFileStream(option))
            {
                SerializeToStream(obj, stream);
            }

        }


        /// <inheritdoc />
        public string SerializeToString(object obj)
        {
            obj.IsNullThrow(nameof(obj));
            return JsonConvert.SerializeObject(obj, Settings);
        }
        /// <inheritdoc />
        public string SerializeToString<T>(T obj) where T : class
        {
            obj.IsNullThrow(nameof(obj));
            return JsonConvert.SerializeObject(obj, Settings);
        }
        /// <inheritdoc />
        public void SerializeToStream<T>(T obj, Stream stream, int bufferSize = 1024, bool leaveStreamOpen = false) where T : class
        {
            obj.IsNullThrow(nameof(obj));
            stream.IsNullThrow(nameof(stream));
            SerializeToStream(obj, obj.GetType(), stream, bufferSize, leaveStreamOpen);
        }
        /// <inheritdoc />
        public void SerializeToFile<T>(T obj, string file, FileOption mode) where T : class
        {
            obj.IsNullThrow(nameof(obj));
            file.IsNullThrow(nameof(file));
            mode.IsNullThrow(nameof(mode));
            SerializeToFile(obj, typeof(T), file, mode);
        }
        /// <inheritdoc />
        public void SerializeToFile<T>(IEnumerable<T> list, string fullFilePath, FileOption option) where T : class
        {
            list.IsNullThrow(nameof(list));
            fullFilePath.IsNullThrow(nameof(fullFilePath));
            option.IsNullThrow(nameof(option));
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
        /// <inheritdoc />
        public void SerializeToFile(object obj, Type type, string fullFilePath, FileOption option)
        {
            fullFilePath.IsNullThrow(nameof(fullFilePath));
            obj.IsNullThrow(nameof(obj));
            type.IsNullThrow(nameof(type));
            option.IsNullThrow(nameof(option));
            var file = new FileObject(fullFilePath);
            if (option == FileOption.DoNothingIfExist)
            {
                if (file.Exist == true) return;
            }
            SerializeToStream(obj, file.GetFileStream(option));
        }


        /// <inheritdoc />
        public object DeserializeFromFile(Type type, string fullFilePath)
        {
            type.IsNullThrow(nameof(type));
            fullFilePath.IsNullThrow(nameof(fullFilePath));

            var tempFile = new FileObject(fullFilePath);
            using (var stream = tempFile.ReadFileToStream())
            {
                return DeserializeFromStream(stream, type);
            }
        }
        /// <inheritdoc />>
        public T DeserializeFromFile<T>(string file) where T : class
        {
            file.IsNullThrow(nameof(file));
            return (T)DeserializeFromFile(typeof(T), file);
        }

        // public List<T> DeserializeFromFilet<T>(string fullFilePath) where T : IEnumerable<T>
        // {
        //     return DeserializeFromFile<IEnumerable<T>>(fullFilePath) as List<T>;
        // }

        /// <inheritdoc />
        public dynamic DeserializeFromFile(string fullFilePath)
        {

            using (var sr = new StreamReader(fullFilePath))
            using (JsonReader reader = new JsonTextReader(sr))
            {
                var serializer = new JsonSerializer();
                return serializer.Deserialize<dynamic>(reader);
            }
        }

        public IEnumerable<dynamic> DeserializeListFromFile(string fullFilePath)
        {
            using (var sr = new StreamReader(fullFilePath))
            using (JsonReader reader = new JsonTextReader(sr))
            {
                var serializer = new JsonSerializer();
                return serializer.Deserialize<List<dynamic>>(reader);
            }
        }

        public IEnumerable<dynamic> DeserializeToList(string content)
        {
            return JsonConvert.DeserializeObject<IEnumerable<dynamic>>(content);
        }

        public IEnumerable<T> DeserializeListFromFile<T>(string fullFilePath) where T : class
        {
            using (var sr = new StreamReader(fullFilePath))
            using (JsonReader reader = new JsonTextReader(sr))
            {
                var serializer = new JsonSerializer();
                return serializer.Deserialize<List<T>>(reader);
            }
        }

        /// <inheritdoc />
        public T DeserializeFromStream<T>(Stream stream)
        {
            stream.IsNullThrow(nameof(stream));
            return (T)DeserializeFromStream(stream, typeof(T));
        }
        /// <inheritdoc />
        public T DeserializeFromString<T>(string text) where T : class
        {
            text.IsNullThrow(nameof(text));
            return JsonConvert.DeserializeObject<T>(text);
        }
        /// <inheritdoc />
        public List<T> DeserializeToList<T>(string content) where T : class
        {
            content.IsNullThrow(nameof(content));
            return (List<T>)DeserializeFromString(content, typeof(List<T>));
        }
        /// <inheritdoc />
        public object DeserializeFromStream(Stream stream, Type type)
        {
            stream.IsNullThrow(nameof(stream));
            type.IsNullThrow(nameof(type));
            if (stream.Position == stream.Length) stream.ResetPosition();
            using (var sr = new StreamReader(stream))
            using (JsonReader reader = new JsonTextReader(sr))
            {
                var serializer = new JsonSerializer();
                return serializer.Deserialize(reader, type);
            }
        }
        /// <inheritdoc />
        public object DeserializeFromString(string json, Type type)
        {
            json.IsNullThrow(nameof(json));
            type.IsNullThrow(nameof(type));
            return JsonConvert.DeserializeObject(json, type);
        }

        public string DeserializeToCSharpClass(string content, string className = null)
        {
            return null;
        }

        public string SerializeToXml(string json, IXmlSerializer xmlSerializer)
        {
            var xml = JsonConvert.DeserializeXmlNode(json); // is node not note
            // or .DeserilizeXmlNode(myJsonString, "root"); // if myJsonString does not have a root
            using (var stringWriter = new StringWriter())
            {
                using (var xmlTextWriter = XmlWriter.Create(stringWriter))
                {
                    xml.WriteTo(xmlTextWriter);
                    return stringWriter.ToString();
                }

            }
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
           if (conversionType == typeof(IJsonSerializer)) return this;
           throw new NotImplementedException();
       }
    }
}
