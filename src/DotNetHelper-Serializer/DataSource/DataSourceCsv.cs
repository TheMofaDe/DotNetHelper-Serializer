
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;
using DotNetHelper_Contracts.Enum.IO;
using DotNetHelper_Contracts.Extension;
using DotNetHelper_IO;
using DotNetHelper_Serializer.Extension;
using ICsvSerializer = DotNetHelper_Serializer.Interface.ICsvSerializer;
using Configuration = CsvHelper.Configuration.Configuration;
namespace DotNetHelper_Serializer.DataSource
{
    /// <summary>
    /// Class DataSourceCsv. This class cannot be inherited.
    /// </summary>
    /// <seealso cref="ICsvSerializer" />
    /// <seealso cref="ICsvSerializer" />
    public sealed class DataSourceCsv : Interface.ICsvSerializer
    {
        /// <summary>
        /// Gets the CSV configuration.
        /// </summary>
        /// <value>The CSV configuration.</value>
        public Configuration Configuration { get; } = new Configuration() { UseNewObjectForNullReferenceMembers = false };
        /// <summary>
        /// Gets or sets the encoding.
        /// </summary>
        /// <value>The encoding.</value>
        public Encoding Encoding { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataSourceCsv" /> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public DataSourceCsv(Configuration configuration = null)
        {
            Configuration = configuration ?? Configuration;
            Encoding = Encoding ?? Encoding.UTF8;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="DataSourceCsv" /> class.
        /// </summary>
        /// <param name="encoding">The encoding.</param>
        /// <param name="configuration">The configuration.</param>
        public DataSourceCsv(Encoding encoding, Configuration configuration = null)
        {
            Encoding = encoding ?? Encoding.UTF8;
            Configuration = configuration ?? Configuration;
        }

        /// <inheritdoc />
        /// <summary>
        /// Serializes to stream.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">The object.</param>
        /// <param name="stream">The stream.</param>
        public void SerializeToStream<T>(T obj, Stream stream, int bufferSize = 1024, bool leaveStreamOpen = false) where T : class
        {
            if (obj is IEnumerable)
            {
                var p = obj as IEnumerable<dynamic>;
                //SerializeToStream<dynamic>(p, stream, bufferSize, leaveStreamOpen);
                using (var sw = new StreamWriter(stream, Encoding, bufferSize, leaveStreamOpen))
                {
                    using (var csv = new CsvWriter(sw, Configuration)) //new StreamWriter(stream, Encoding), Configuration))
                    {
                        csv.WriteRecords(p);
                       // csv.NextRecord();
                    }
                }
                return;
            }
            SerializeToStream(obj, obj.GetType(), stream);
        }

        /// <inheritdoc />
        /// <summary>
        /// Serializes to stream.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="type">The type.</param>
        /// <param name="stream">The stream.</param>
        public void SerializeToStream(object obj, Type type, Stream stream, int bufferSize = 1024, bool leaveStreamOpen = false)
        {
            using (var sw = new StreamWriter(stream, Encoding, bufferSize, leaveStreamOpen))
            {
                using (var csv = new CsvWriter(sw, Configuration)) //new StreamWriter(stream, Encoding), Configuration))
                {
                    PreCheck(csv, type);
                    csv.WriteRecord(obj);
                  //  csv.NextRecord();
                }
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// Serializes to file.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">The object.</param>
        /// <param name="file">The file.</param>
        /// <param name="mode">The mode.</param>
        public void SerializeToFile<T>(T obj, string file, FileOption mode) where T : class
        {
            SerializeToFile(obj, typeof(T), file, mode);
        }

        /// <inheritdoc />
        /// <summary>
        /// Serializes to file.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">The list.</param>
        /// <param name="fullFilePath">The full file path.</param>
        /// <param name="mode">The mode.</param>
        public void SerializeToFile<T>(IEnumerable<T> list, string fullFilePath, FileOption option) where T : class
        {
            list.IsNullThrow(nameof(list));
            fullFilePath.IsNullThrow(nameof(fullFilePath));
            option.IsNullThrow(nameof(option));
            var a = new FileObject(fullFilePath);
            if (option == FileOption.DoNothingIfExist)
            {
                if (a.Exist == true) return;
            }
            using (var sw = a.GetStreamWriter(false))
            {
                var csv = new CsvWriter(sw, Configuration);
                csv.WriteRecords(list);
            }

            // using (var stream = a.GetFileStream(mode))
            // {
            //     SerializeToStream(list, stream);
            // }
        }



        /// <summary>
        /// Serializes to file.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="type">The type.</param>
        /// <param name="fullFilePath">The full file path.</param>
        /// <param name="mode">The mode.</param>
        /// <exception cref="ArgumentOutOfRangeException">mode - null</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">mode - null</exception>
        /// <inheritdoc />
        public void SerializeToFile(object obj, Type type, string fullFilePath, FileOption option)
        {
            var file = new FileObject(fullFilePath);
            if (option == FileOption.DoNothingIfExist)
            {
                if (file.Exist == true) return;
            }
       //     file.PrepareForStreamUse(option);
            var stream = file.GetFileStream(option);

            SerializeToStream(obj, stream);
        }

        public void SerializeToFile(dynamic obj, string fullFilePath, FileOption option)
        {

            var file = new FileObject(fullFilePath);
            if (option == FileOption.DoNothingIfExist)
            {
                if (file.Exist == true) return;
            }
          //  file.PrepareForStreamUse(option);
            var stream = file.GetFileStream(option);
            using (var sw = new StreamWriter(stream, Encoding))
            {
                using (var csv = new CsvWriter(sw, Configuration)) //new StreamWriter(stream, Encoding), Configuration))
                {
                    csv.WriteRecord(obj);
                  //  csv.NextRecord();
                }
            }
        }

        /// <summary>
        /// Serializes to string.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>System.String.</returns>
        public string SerializeToString(object obj)
        {
            var sb = new StringBuilder();
            using (var sw = new StringWriter(sb))
            {
                using (var csv = new CsvWriter(sw, Configuration))
                {
                    PreCheck(csv, obj.GetType());
                    csv.WriteRecord(obj);
                 //   csv.NextRecord();
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Serializes to string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">The object.</param>
        /// <returns>System.String.</returns>
        public string SerializeToString<T>(T obj) where T : class
        {

            var sb = new StringBuilder();
            using (var sw = new StringWriter(sb))
            {
                using (var csv = new CsvWriter(sw, Configuration))
                {
                    PreCheck(csv, typeof(T));
                    csv.WriteRecord<T>(obj);
                  //  csv.NextRecord();
                }
            }
            return sb.ToString();

        }

        /// <inheritdoc />
        /// <summary>
        /// Deserializes from file.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="file">The file.</param>
        /// <returns>System.Object.</returns>
        public object DeserializeFromFile(Type type, string fullFilePath)
        {
            var file = new FileObject(fullFilePath);
            if (file.Exist != true) throw new FileNotFoundException(fullFilePath);
            using (TextReader fileReader = file.GetStreamReader())
            {
                using (var csv = new CsvReader(fileReader, Configuration))
                {
                    if (type.IsTypeIEnumerable())
                    {
                        return csv.GetRecords(type);
                    }
                    else
                    {
                        // PreCheck(csv);
                        csv.Read();
                        return csv.GetRecord(type);
                    }
                }
            }
        }





        /// <inheritdoc />
        /// <summary>
        /// Deserializes from file.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="file">The file.</param>
        /// <returns>T.</returns>
        public T DeserializeFromFile<T>(string fullFilePath) where T : class
        {
            if (typeof(T).IsTypeIEnumerable())
            {
             //   var realType = typeof(T).GetEnumerableItemType();
                var list = DeserializeListFromFile(fullFilePath);
                return list as T;
            }
            return (T)DeserializeFromFile(typeof(T), fullFilePath);
        }


        /// <inheritdoc />
        public IEnumerable<dynamic> DeserializeListFromFile(string fullFilePath)
        {
            var file = new FileObject(fullFilePath);
            if (file.Exist != true) throw new FileNotFoundException(fullFilePath);
            using (TextReader fileReader = file.GetStreamReader())
            {
                using (var csv = new CsvReader(fileReader, Configuration,true))
                {
                    return csv.GetRecords<dynamic>().ToList(); 
                }
            }
        }

        public IEnumerable<dynamic> DeserializeToList(string content)
        {
           content.IsNullThrow(nameof(content));
            using (TextReader fileReader = new StringReader(content))
            {
                using (var csv = new CsvReader(fileReader, Configuration, true))
                {
                    return csv.GetRecords<dynamic>().ToList();
                }
            }
        }

        public IEnumerable<T> DeserializeListFromFile<T>(string fullFilePath) where T : class
        {
            var file = new FileObject(fullFilePath);
            if (file.Exist != true) throw new FileNotFoundException(fullFilePath);
            using (TextReader fileReader = file.GetStreamReader())
            {
                using (var csv = new CsvReader(fileReader, Configuration))
                {
                    return csv.GetRecords<T>().ToList();
                }
            }
        }

        /// <inheritdoc />
        public dynamic DeserializeFromFile(string fullFilePath)
        {
            var file = new FileObject(fullFilePath);
            if (file.Exist != true) throw new FileNotFoundException(fullFilePath);
            using (TextReader fileReader = file.GetStreamReader())
            {
                using (var csv = new CsvReader(fileReader, Configuration))
                {
                    return csv.GetRecord<dynamic>();
                }
            }
        }


        /// <summary>
        /// Deserializes from stream.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream">The stream.</param>
        /// <returns>T.</returns>
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
        public object DeserializeFromStream(Stream stream, Type type)
        {
            using (var reader = new StreamReader(stream, Encoding, true))
            {
                var csv = new CsvReader(reader, Configuration);
                PreCheck(csv);
                csv.Read();
                return csv.GetRecord(type);
            }
        }

        /// <summary>
        /// Deserializes from string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="text">The text.</param>
        /// <returns>T.</returns>
        public T DeserializeFromString<T>(string text) where T : class
        {
            return (T)DeserializeFromString(text, typeof(T));
        }

        /// <summary>
        /// Deserializes to list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="content">The content.</param>
        /// <returns>List&lt;T&gt;.</returns>
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
        public object DeserializeFromString(string content, Type type)
        {
            using (var reader = new StringReader(content))
            {
                var csv = new CsvReader(reader, Configuration);
                PreCheck(csv);
                csv.Read();
                return csv.GetRecord(type);
            }
        }

        /// <summary>
        /// Deserializes to c sharp class.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="className">Name of the class.</param>
        /// <returns>System.String.</returns>
        /// <exception cref="NotImplementedException"></exception>
        /// <exception cref="System.NotImplementedException"></exception>
        public string DeserializeToCSharpClass(string content, string className = null)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Pres the check.
        /// </summary>
        /// <param name="csv">The CSV.</param>
        private void PreCheck(CsvReader csv)
        {
            // if (Configuration.HasHeaderRecord)
            // {
            //     csv.ReadHeader();
            // }
            // else
            // {
            //     
            // }
        }
        /// <summary>
        /// Checks if developer wants to include the type headers
        /// </summary>
        /// <param name="csv">The CSV.</param>
        /// <param name="type">The type.</param>
        private void PreCheck(CsvWriter csv, Type type)
        {
            if (Configuration.HasHeaderRecord)
            {
                csv.WriteHeader(type);
                csv.NextRecord();
            }
            else
            {

            }
        }

        /// <summary>
        /// Shoulds the skip record.
        /// </summary>
        /// <param name="strings">The strings.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        private bool ShouldSkipRecord(string[] strings)
        {
            return strings.IsNullOrEmpty() || strings.ToList().All(string.IsNullOrEmpty);
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
            if (conversionType == typeof(ICsvSerializer)) return this;
            throw new NotImplementedException();
        }
    }
}
