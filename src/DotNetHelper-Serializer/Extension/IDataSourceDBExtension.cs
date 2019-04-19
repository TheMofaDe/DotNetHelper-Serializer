using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using DotNetHelper_Contracts.Extension;
using DotNetHelper_Contracts.CustomException;
using DotNetHelper_Contracts.Enum;
using DotNetHelper_Serializer.DataSource;
using DotNetHelper_Serializer.Helper;
using DotNetHelper_Serializer.Interface;
using Newtonsoft.Json;

namespace DotNetHelper_Serializer.Extension
{
    public static class IDataSourceDBExtension
    {
        /// <summary>
        /// Builds the SQL parameter list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="poco">The poco.</param>
        /// <returns>List&lt;DbParameter&gt;.</returns>
        public static List<DbParameter> BuildDbParameterList<T>(this IDataSourceDb database, T poco) where T : class
        {
            DataSourceDb db = database is DataSourceDb sourceDb ? sourceDb : new DataSourceDb(database.DBTYPE);
           
            var list = new List<DbParameter>() { };

            ExtFastMember.GetAdvanceMembers(poco).ForEach(delegate (AdvanceMember p)
            {
                var validation = DataValidation.IsValidBasedOnSqlColumnAttributes(p);
                if (!validation.Item1)
                {
                    throw new InvalidDataException(string.Join(Environment.NewLine, validation.Item2));
                }

                object parameterValue = DBNull.Value;
                if (p.SqlCustomAttritube.SerializableType != SerializableType.NONE)
                {
                    if (p.Member.Type == typeof(string))
                    {
                        // assuming the string is already serialize and the developer don't know how this library works smh
                        parameterValue = db.ObjectSqlHelper.ConvertToDatabaseValue(p.Member, p.Value);
                    }
                    else
                    {
                        if (p.Value == null)
                        {
                            parameterValue = DBNull.Value;

                        }
                        else
                        {
                            switch (p.SqlCustomAttritube.SerializableType)
                            {
                                case SerializableType.XML:
                                    database.XmlSerializer.IsNullThrow(nameof(database.XmlSerializer),
                                        new BadCodeException(
                                            $"YOU FOOL!!! Your claiming the property {p.Member.Name} in the type {p.Member.Type.FullName} value is in XML Format but your not specifying how to deserialize/serialize it )"));
                                    parameterValue = database.XmlSerializer.SerializeToString(p.Value);
                                    break;
                                case SerializableType.JSON:
                                    database.JsonSerializer.IsNullThrow(nameof(JsonSerializer),
                                        new BadCodeException(
                                            $"YOU FOOL!!! Your claiming the property {p.Member.Name} in the type {p.Member.Type.FullName} value is in JSON Format but your not specifying how to deserialize/serialize it )"));
                                    parameterValue = database.JsonSerializer.SerializeToString(p.Value);
                                    break;
                                case SerializableType.CSV:
                                    database.CsvSerializer.IsNullThrow(nameof(database.CsvSerializer),
                                        new BadCodeException(
                                            $"YOU FOOL!!! Your claiming the property {p.Member.Name} in the type {p.Member.Type.FullName} value is in CSV Format but your not specifying how to deserialize/serialize it )"));
                                    parameterValue = database.CsvSerializer.SerializeToString(p.Value);
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }

                            //parameterValue = $"'{parameterValue.ToString().Replace("'", @"\'")}'";
                        }
                    }

                }
                else
                {
                    parameterValue = db.ObjectSqlHelper.ConvertToDatabaseValue(p.Member, p.Value);
                }
                // var command = GetNewCommand();

                list.Add(db.GetNewParameter($"@{p.Member.Name}", parameterValue));

            });
            return list;
        }

    }
}
