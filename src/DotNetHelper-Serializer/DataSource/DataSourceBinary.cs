
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using DotNetHelper_Contracts.Enum.IO;
using DotNetHelper_Contracts.Extension;
using DotNetHelper_IO;
using DotNetHelper_IO.Interface;

namespace DotNetHelper_Serializer.DataSource
{
    public class DataSourceBinary : ISerializer
    {


        public BinaryFormatter Formatter { get; }
        /// <summary>
        /// Gets or sets the encoding.
        /// </summary>
        /// <value>The encoding.</value>
        public Encoding Encoding { get; set; }

        public DataSourceBinary(Encoding encoding, BinaryFormatter b = null)
        {
            Formatter = b ?? new BinaryFormatter();
            Encoding = encoding ?? Encoding.UTF8;
        }
        public DataSourceBinary()
        {
            Formatter = new BinaryFormatter();
            Encoding = Encoding.UTF8;
        }


        /// <inheritdoc />
        public void SerializeToStream(object obj, Type type, Stream stream, int bufferSize = 1024, bool leaveStreamOpen = false)
        {
            Formatter.Serialize(stream, obj);
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
            using (Stream stream = new MemoryStream())
            {
                Formatter.Serialize(stream, obj);
                var reader = new StreamReader(stream);
                return reader.ReadToEnd();
            }

        }
        /// <inheritdoc />
        public string SerializeToString<T>(T obj) where T : class
        {
            obj.IsNullThrow(nameof(obj));
            using (Stream stream = new MemoryStream())
            {
                Formatter.Serialize(stream, obj);
                var reader = new StreamReader(stream);
                return reader.ReadToEnd();
            }
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
            {
                return Formatter.Deserialize(sr.BaseStream);
            }
        }

        public IEnumerable<dynamic> DeserializeListFromFile(string fullFilePath)
        {
            using (var sr = new StreamReader(fullFilePath))
            {
                return Formatter.Deserialize(sr.BaseStream) as IEnumerable<dynamic>;
            }
        }

        public IEnumerable<dynamic> DeserializeToList(string content)
        {
            //   return Formatter.Deserialize(new StringBuilder(content).);
            throw new InvalidOperationException();
        }

        public IEnumerable<T> DeserializeListFromFile<T>(string fullFilePath) where T : class
        {
            using (var sr = new StreamReader(fullFilePath))
                throw new InvalidOperationException();
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
            using (Stream stream = new MemoryStream(Encoding.GetBytes(text)))
            {
                return Formatter.Deserialize(stream) as T;
            }
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
            return Formatter.Deserialize(stream);
        }
        /// <inheritdoc />
        public object DeserializeFromString(string json, Type type)
        {
            json.IsNullThrow(nameof(json));
            type.IsNullThrow(nameof(type));
            using (Stream stream = new MemoryStream(Encoding.GetBytes(json)))
            {
                return Formatter.Deserialize(stream);
            }
        }

        public string DeserializeToCSharpClass(string content, string className = null)
        {
            return null;
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
            throw new NotImplementedException();
        }
    }
}
