using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Dynamic;
using System.Linq;
using DotNetHelper_Contracts.Enum;
using DotNetHelper_Contracts.Extension;
using DotNetHelper_Contracts.Helpers;

using DotNetHelper_Serializer.Interface;
using FastMember;
using Newtonsoft.Json;

namespace DotNetHelper_Serializer.Extension
{
    public static class ExtDataReader
    {



        /// <summary>
        /// Reads all available bytes from reader
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        public static byte[] GetBytes(this IDataReader reader, int ordinal)
        {
            byte[] result = null;

            if (!reader.IsDBNull(ordinal))
            {
                var size = reader.GetBytes(ordinal, 0, null, 0, 0); //get the length of data 
                result = new byte[size];
                var bufferSize = 1024;
                long bytesRead = 0;
                var curPos = 0;
                while (bytesRead < size)
                {
                    bytesRead += reader.GetBytes(ordinal, curPos, result, curPos, bufferSize);
                    curPos += bufferSize;
                }
            }

            return result;
        }

        public static List<Dictionary<string, object>> ToDictionary(this IDataReader datareader)
        {
            var dataRows = new List<Dictionary<string, object>>();
            if (datareader == null || datareader.IsClosed)
            {
              
                return new List<Dictionary<string, object>>() { };
            }

            using (datareader)
            {
                while (datareader.Read())
                {
                    var dict = new Dictionary<string, object>(); // ROW => COLUMN,VALUE

                    for (var i = 0; i < datareader.FieldCount; i++)
                    {
                        dict.Add(datareader.GetName(i), datareader.IsDBNull(i) ? null : datareader.GetValue(i));

                    }
                    dataRows.Add(dict);
                }
            }
            return dataRows;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <param name="type">Null Value Will Be Converted To Null Or DBNull</param>
        /// <returns></returns>
        public static DataTable MapToDataTable<T>(this IDataReader reader, string tableName) where T : class
        {
            if (string.IsNullOrEmpty(tableName)) tableName = typeof(T).Name;
            var dt = new DataTable() { TableName = tableName };
            dt.Load(reader);
            return dt;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <param name="type">Null Value Will Be Converted To Null Or DBNull</param>
        /// <returns></returns>
        public static DataTable MapToDataTable(this IDataReader reader, string tableName)
        {
            if (string.IsNullOrEmpty(reader.GetSchemaTable()?.TableName)) tableName = reader.GetSchemaTable()?.TableName;
            var dt = new DataTable() { TableName = tableName };
            dt.Load(reader);
            return dt;
        }



        public static bool? HasRows(this IDataReader reader)
        {
            if (reader is DbDataReader a)
                return a.HasRows;
            return null;
        }




        public static void AddParameters(this IDbCommand command, List<IDbDataParameter> parameters)
        {
            if (!parameters.IsNullOrEmpty())
                parameters.ForEach(parameter =>
                {
                    command?.Parameters.Add(parameter);
                });
        }

        public static void AddParameters(this IDbCommand command, List<DbParameter> parameters)
        {
            if (!parameters.IsNullOrEmpty())
                parameters.ForEach(parameter =>
                {
                    command?.Parameters.Add(parameter);
                });
        }

  


        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static List<T> MapToList<T>(this IDataReader reader, IJsonSerializer jsonSerializer = null, IXmlSerializer xmlSerializer = null, ICsvSerializer csvSerializer = null) where T : class
        {

            if (reader == null || reader.IsClosed)
            {
              
                return new List<T>() { };
            }

          

            var accessor = TypeAccessor.Create(typeof(T), true);


            var pocoList = new List<T>();

            // Cache the field names in the reader for use in our while loop for efficiency.
            var readerFieldLookup = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase); // store name and ordinal
            for (var i = 0; i < reader.FieldCount; i++)
            {
                readerFieldLookup.Add(reader.GetName(i), i);
            }
            if (typeof(T) == typeof(string))
            {
                while (reader.Read())
                {
                    pocoList.Add(reader.GetString(0) as T);
                }
                reader.Close();
                reader.Dispose();
                return pocoList;
            }

            while (reader.Read())
            {
                var clonedPoco = TypeExtension.New<T>.Instance();
                var advanceMembers = ExtFastMember.GetAdvanceMembers(clonedPoco);

                if (advanceMembers.IsNullOrEmpty() && typeof(IDynamicMetaObjectProvider).IsAssignableFrom(typeof(T)))
                {

                    readerFieldLookup.ForEach(delegate (KeyValuePair<string, int> pair)
                    {
                        advanceMembers.Add(new AdvanceMember() { Member = new DynamicMember() { Name = pair.Key } });
                    });
                }
                advanceMembers.Where(p => readerFieldLookup.ContainsKey(p.GetActualMemberName())).ToList().ForEach(delegate (AdvanceMember p)
                {
                    SetPocoProperteyValue(clonedPoco, reader, p, accessor, jsonSerializer, xmlSerializer, csvSerializer);

                });
                pocoList.Add(clonedPoco);
            }
            reader.Close();
            reader.Dispose();
            return pocoList;
        }


        public static List<object> MapToList(this IDataReader reader, Type mapToType)
        {

            if (reader == null || reader.IsClosed)
            {  
                return new List<object>() { };
            }
            var accessor = TypeAccessor.Create(mapToType, true);


            var pocoList = new List<object>();

            // Cache the field names in the reader for use in our while loop for efficiency.
            var readerFieldLookup = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase); // store name and ordinal
            for (var i = 0; i < reader.FieldCount; i++)
            {
                readerFieldLookup.Add(reader.GetName(i), i);
            }
            if (mapToType == typeof(string))
            {
                while (reader.Read())
                {
                    pocoList.Add(reader[0] as dynamic);
                }
                reader.Close();
                reader.Dispose();
                return pocoList;
            }

            while (reader.Read())
            {

                if (!accessor.CreateNewSupported)
                {
                    if (mapToType.IsStruct())
                    {
                        pocoList.Add(reader.GetValue(reader.GetOrdinal(reader.GetName(0))));
                        continue;
                    }
                    else
                    {
                        throw new Exception($"Could Not Initialize A New Instance Of The Type {mapToType.FullName}");
                    }
                }

                var clonedPoco = accessor.CreateNew();


                accessor.GetMembers().Where(p => readerFieldLookup.ContainsKey(p.Name)) //&& reader.GetValue(reader.GetOrdinal(p.Name)) != DBNull.Value)
                    .ToList() //  .Where(p => !reader.IsDBNull(readerFieldLookup[p.Name])).ToList()
                    .ForEach(delegate (Member p)
                    {
                        var value = reader.GetValue(reader.GetOrdinal(p.Name));
                        if (value.GetType().Name != p.Type.Name && value.GetType() != typeof(DBNull))
                        {
                            var type = p.Type;
                            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                            {
                                type = Nullable.GetUnderlyingType(type);
                            }
                            if (type == null) type = p.Type;


                            if (value.ToString().IsValidJson(type))
                            {
                                value = JsonConvert.DeserializeObject(value.ToString(), type);
                            }
                            else
                            {
                                value = !type.IsEnum ? Convert.ChangeType(value, type, null) : Enum.Parse(type, value.ToString(), true);

                            }
                        }
                        try
                        {
                            accessor[clonedPoco, p.Name] = value is DBNull ? null : value; 
                        }
                        catch (Exception error)
                        {
                            Console.WriteLine(error.Message);
                        }
                    });
                pocoList.Add(clonedPoco);
            }
            reader.Close();
            reader.Dispose();
            return pocoList;
        }





        private static void SetPocoProperteyValue<T>(T poco, IDataReader reader, AdvanceMember p, TypeAccessor accessor, IJsonSerializer jsonSerializer = null, IXmlSerializer xmlSerializer = null, ICsvSerializer csvSerializer = null)
        {
            var isDynamic = (typeof(IDynamicMetaObjectProvider).IsAssignableFrom(typeof(T)));



            var value = reader.GetValue(reader.GetOrdinal(p.GetActualMemberName()));

            if (isDynamic)
            {
                DynamicObjectHelper.AddProperty(poco as ExpandoObject, p.GetActualMemberName(), value);
                return;
            }

            if (value.GetType().Name != p.Member.Type.Name && value.GetType() != typeof(DBNull))
            {

                var type = p.Member.Type;
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    type = Nullable.GetUnderlyingType(type);
                }
                if (type == null) type = p.Member.Type;


                if (p?.SqlCustomAttritube?.SerializableType != null && p?.SqlCustomAttritube?.SerializableType != SerializableType.NONE)
                {
                    switch (p.SqlCustomAttritube.SerializableType)
                    {
                        case SerializableType.XML:
                            value = xmlSerializer?.DeserializeFromString(value.ToString(), type);
                            break;
                        case SerializableType.JSON:
                            value = jsonSerializer?.DeserializeFromString(value.ToString(), type);
                            break;
                        case SerializableType.CSV:
                            value = csvSerializer?.DeserializeFromString(value.ToString(), type);
                            break;
                        case SerializableType.NONE:
                            value = !type.IsEnum ? Convert.ChangeType(value, type, null) : Enum.Parse(type, value.ToString(), true);
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    value = !type.IsEnum ? Convert.ChangeType(value, type, null) : Enum.Parse(type,value.ToString(),true);
                }
            }
            try
            {
                accessor[poco, p.Member.Name] = value is DBNull ? null : value;
            }
            catch (Exception error)
            {
                // this property may not be settable
                     Console.WriteLine(error.Message);
            }
        }


    }
}
