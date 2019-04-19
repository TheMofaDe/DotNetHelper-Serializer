using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using DotNetHelper_Contracts.Enum.DataSource;
using DotNetHelper_Serializer.Extension;
using Newtonsoft.Json;
using SqlColumnAttritubeMembers = DotNetHelper_Serializer.Attribute.SqlColumnAttritubeMembers;

namespace DotNetHelper_Serializer.Tools
{
    public static class DevelopersTools
    {
        public static string ToCSharpClass(IDataReader reader, string className = null, bool nullifyProperties = false)
        {
            if (reader.IsClosed)
            {
                return null;
            }
            var sb = new StringBuilder();
            try
            {
                className = string.IsNullOrEmpty(className) ? "DynamicClass" : className;
                sb.AppendLine($"public class {className} {{ ");
                for (var i = 0; i < reader.FieldCount; i++)
                {
                    sb.AppendLine(nullifyProperties ? $"public {reader.GetFieldType(i).Name}? {reader.GetName(i)} {{ get; set; }} = null "
                        : $"public {reader.GetFieldType(i).Name} {reader.GetName(i)} {{ get; set; }} ");
                }
                sb.AppendLine($"}}");
                reader.Close();
                reader.Dispose();
            }
            catch (Exception e)
            {
           
                reader.Close();
                reader.Dispose();
                throw e;

            }
            return sb.ToString();
        }

        public static string XmlTCSharpClass(string xml)
        {
            var doc = XDocument.Parse(xml); //or XDocument.Load(path)
            string jsonText = JsonConvert.SerializeXNode(doc);
            dynamic dyn = JsonConvert.DeserializeObject<ExpandoObject>(jsonText);
            var className = doc.Root.Name.LocalName;
            var props = (IDictionary<string, object>)dyn;

            var sb = new StringBuilder();
            try
            {
                className = string.IsNullOrEmpty(className) ? "DynamicClass" : className;
                sb.AppendLine($"public class {className} {{ ");
                for (var i = 0; i < props.Count; i++)
                {
                    sb.AppendLine($"public {props.Values.ToList()[i].GetType().Name} {props.Keys.ToList()[i]} {{ get; set; }} ");
                }
                sb.AppendLine($"}}");
            }
            catch (Exception)
            {

            }
            return sb.ToString();
        }
    



        public static string AdvanceTypeToSqlType(AdvanceMember advance, DataBaseType sqlType, bool hasMultipleKeys = false)
        {
            var str = TypeToSqlType(advance.Member.Type, sqlType);
            if (advance.Member.Type == typeof(string) && advance.SqlCustomAttritube.PrimaryKey == true)
            {
                str = str.Replace("(MAX)", "(900)"); /// SQL SERVER Doesn't allow varchar max to be primary key must be 900 bytes or less
            }

            var enumSql = System.Enum.GetValues(typeof(SqlColumnAttritubeMembers)).Cast<SqlColumnAttritubeMembers>().ToList();
            var allowIdentity = true;
            enumSql.ForEach(delegate (SqlColumnAttritubeMembers members)
            {
                switch (members)
                {
                    case SqlColumnAttritubeMembers.SetMaxColumnSize:
                        if (advance.SqlCustomAttritube.MaxColumnSize != null)
                        {
                            //  if (sqlType == DataBaseType.Sqlite)
                            str = str.Replace(" (MAX)", $"({advance.SqlCustomAttritube.MaxColumnSize})");
                        }
                        break;
                    case SqlColumnAttritubeMembers.SetNullable:
                        if (advance.SqlCustomAttritube.Nullable == false)
                        {
                            str += ($" NOT NULL ");
                        }
                        else if (advance.SqlCustomAttritube.Nullable == true)
                        {
                            str += ($" NULL ");
                        }
                        break;
                    case SqlColumnAttritubeMembers.SetAutoIncrementBy:
                    case SqlColumnAttritubeMembers.SetStartIncrementAt:
                        if (sqlType == DataBaseType.Access95) break;
                        if (sqlType == DataBaseType.Sqlite) break;
                        if (allowIdentity)
                        {
                            if (advance.SqlCustomAttritube.AutoIncrementBy != null ||
                                advance.SqlCustomAttritube.StartIncrementAt != null)
                                str +=  $" IDENTITY({advance.SqlCustomAttritube.StartIncrementAt.GetValueOrDefault(1)},{advance.SqlCustomAttritube.AutoIncrementBy.GetValueOrDefault(1)})";
                        }

                        allowIdentity = false;

                        break;
                    case SqlColumnAttritubeMembers.SetUtcDateTime:

                        break;
                    case SqlColumnAttritubeMembers.SetPrimaryKey:
                        if (sqlType == DataBaseType.Access95) break;
                        if (advance.SqlCustomAttritube.PrimaryKey != null && advance.SqlCustomAttritube.PrimaryKey == true && !hasMultipleKeys)
                            str += ($" PRIMARY KEY ");
                        break;
                    case SqlColumnAttritubeMembers.SetApiId:

                        break;
                    case SqlColumnAttritubeMembers.SetSyncTime:

                        break;
                    case SqlColumnAttritubeMembers.SetIgnore:

                        break;
                    case SqlColumnAttritubeMembers.MapTo:
                        break;
                    case SqlColumnAttritubeMembers.DefaultValue:
                        break;
                    case SqlColumnAttritubeMembers.TSQLDefaultValue:

                        if(!string.IsNullOrEmpty(advance.SqlCustomAttritube.TSQLDefaultValue))
                        str += ($" DEFAULT {advance.SqlCustomAttritube.TSQLDefaultValue} ");
                        break;
                    case SqlColumnAttritubeMembers.SetxRefTableType:
                        break;
                    case SqlColumnAttritubeMembers.xRefTableSchema:
                        break;
                    case SqlColumnAttritubeMembers.xRefTableName:
                        if (!string.IsNullOrEmpty(advance.SqlCustomAttritube.xRefTableName) && !string.IsNullOrEmpty(advance.SqlCustomAttritube.xRefJoinOnColumn))
                        {
                            str += ($" FOREIGN KEY REFERENCES {advance.SqlCustomAttritube.xRefTableName}({advance.SqlCustomAttritube.xRefJoinOnColumn}) ");
                            if (advance.SqlCustomAttritube.xRefOnDeleteCascade.GetValueOrDefault(false))
                                str += ($" ON DELETE CASCADE ");
                            if (advance.SqlCustomAttritube.xRefOnUpdateCascade.GetValueOrDefault(false))
                                str += ($" ON UPDATE CASCADE ");
                        }
                        break;
                    case SqlColumnAttritubeMembers.xRefJoinOnColumn:
                        break;
                    case SqlColumnAttritubeMembers.SetxRefOnUpdateCascade:
                        break;
                    case SqlColumnAttritubeMembers.SetxRefOnDeleteCascade:
                        break;
                    case SqlColumnAttritubeMembers.MappingIds:
                        // WILL NEVER DO ANYTHING THIS IS FOR JOIN PURPOSE ONLY
                        break;
                    case SqlColumnAttritubeMembers.SerializableType:

                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(members), members, null);
                }
            });


            return str;
        }


        // https://www.tutorialspoint.com/sqlite/sqlite_data_types.htm
        public static string TypeToSqlType(Type type, DataBaseType sqlType)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                type = Nullable.GetUnderlyingType(type);

            }

            if (type == typeof(string) || type == typeof(object))
            {
                switch (sqlType)
                {
                    case DataBaseType.SqlServer:
                        return ($"VARCHAR (MAX)");
                    case DataBaseType.MySql:
                        return ($"VARCHAR (MAX)");
                    case DataBaseType.Sqlite:
                        return ($"TEXT");
                    case DataBaseType.Oracle:
                        break;
                    case DataBaseType.Oledb:
                        break;
                    case DataBaseType.Access95:
                        return ($"TEXT"); // VARCHARS WORKS BUT WE WON'T DEFAULT TO IT BECAUSE IT ONLY SUPPORTS UP TO 255 CHARACTERS
                    case DataBaseType.Odbc:
                        return ($"VARCHAR (MAX)");

                    default:
                        throw new ArgumentOutOfRangeException(nameof(sqlType), sqlType, null);
                }

            }
            else if (type == typeof(int) || type == typeof(System.Enum) || type == typeof(short))
            {
                switch (sqlType)
                {
                    case DataBaseType.SqlServer:
                        return ($"INT");
                    case DataBaseType.MySql:
                        return ($"INT");
                    case DataBaseType.Sqlite:
                        return ($"INTEGER");
                    case DataBaseType.Oracle:
                        break;
                    case DataBaseType.Oledb:
                        break;
                    case DataBaseType.Access95:
                        return ($"INTEGER");
                    case DataBaseType.Odbc:
                        return ($"INT");

                    default:
                        throw new ArgumentOutOfRangeException(nameof(sqlType), sqlType, null);
                }

            }
            else if (type == typeof(long))
            {
                switch (sqlType)
                {
                    case DataBaseType.SqlServer:
                        return ($"BIGINT");
                    case DataBaseType.MySql:
                        return ($"BIGINT");
                    case DataBaseType.Sqlite:
                        return ($"BIGINT");
                    case DataBaseType.Oracle:
                        break;
                    case DataBaseType.Oledb:
                        break;
                    case DataBaseType.Access95:
                        return ($"INTEGER");
                    case DataBaseType.Odbc:
                        return ($"BIGINT");

                    default:
                        throw new ArgumentOutOfRangeException(nameof(sqlType), sqlType, null);
                }

            }


            else if (type == typeof(DateTime))
            {

                switch (sqlType)
                {

                    case DataBaseType.SqlServer:
                        return ($"DATETIME");
                    case DataBaseType.MySql:
                        return ($"DATETIME");
                    case DataBaseType.Sqlite:
                        return ($"TEXT");
                    case DataBaseType.Oracle:
                        break;
                    case DataBaseType.Oledb:
                        break;
                    case DataBaseType.Access95:
                        return ($"DATETIME");
                    case DataBaseType.Odbc:
                        return ($"DATETIME");

                    default:
                        throw new ArgumentOutOfRangeException(nameof(sqlType), sqlType, null);
                }
            }

            else if (type == typeof(DateTimeOffset)) // TODO :: NEED TO VALIDATE OTHER DATEBASE TYPE OTHER THAN SQLSERVER 
            {

                switch (sqlType)
                {

                    case DataBaseType.SqlServer:
                        return ($"DATETIMEOFFSET");
                    case DataBaseType.MySql:
                        return ($"DATETIMEOFFSET");
                    case DataBaseType.Sqlite:
                        return ($"TEXT");
                    case DataBaseType.Oracle:
                        break;
                    case DataBaseType.Oledb:
                        break;
                    case DataBaseType.Access95:
                        return ($"DATETIMEOFFSET");
                    case DataBaseType.Odbc:
                        return ($"DATETIMEOFFSET");

                    default:
                        throw new ArgumentOutOfRangeException(nameof(sqlType), sqlType, null);
                }
            }
            else if (type == typeof(bool))
            {
                switch (sqlType)
                {
                    case DataBaseType.SqlServer:
                        return ($"BIT");
                    case DataBaseType.MySql: // SUPPORTS MySQL 5.0.3 & HIGHER ONLY 
                        return ($"BIT");
                    case DataBaseType.Sqlite:
                        return ($"BIT");
                    case DataBaseType.Oracle:
                        break;
                    case DataBaseType.Oledb:
                        break;
                    case DataBaseType.Access95:
                        return ($"BIT");
                    case DataBaseType.Odbc:
                        return ($"BIT");

                    default:
                        throw new ArgumentOutOfRangeException(nameof(sqlType), sqlType, null);
                }
            }
            else if (type == typeof(double))
            {

                switch (sqlType)
                {
                    case DataBaseType.SqlServer:
                        return ($"FLOAT");
                    case DataBaseType.MySql:
                        return ($"DOUBLE");
                    case DataBaseType.Sqlite:
                        return ($"REAL");
                    case DataBaseType.Oracle:
                        break;
                    case DataBaseType.Oledb:
                        break;
                    case DataBaseType.Access95:
                        return ($"DOUBLE");
                    case DataBaseType.Odbc:
                        return ($"FLOAT");

                    default:
                        throw new ArgumentOutOfRangeException(nameof(sqlType), sqlType, null);
                }
            }
            else if (type == typeof(decimal))
            {

                switch (sqlType)
                {
                    case DataBaseType.SqlServer:
                        return ($"DECIMAL (18,2)");
                    case DataBaseType.MySql:
                        return ($"DECIMAL (18,2)");
                    case DataBaseType.Sqlite:
                        return ($"NUMERIC");
                    case DataBaseType.Oracle:
                        break;
                    case DataBaseType.Oledb:
                        break;
                    case DataBaseType.Access95:
                        return ($"CURRENCY");
                    case DataBaseType.Odbc:
                        return ($"DECIMAL (18,2)");

                    default:
                        throw new ArgumentOutOfRangeException(nameof(sqlType), sqlType, null);
                }
            }
            else if (type == typeof(byte))
            {

                switch (sqlType)
                {
                    case DataBaseType.SqlServer:
                        return ($"TINYINT");
                    case DataBaseType.MySql:
                        return ($"TINYINT");
                    case DataBaseType.Sqlite:
                        return ($"TINYINT");
                    case DataBaseType.Oracle:
                        break;
                    case DataBaseType.Oledb:
                        break;
                    case DataBaseType.Access95:
                        return ($"SMALLINT");
                    case DataBaseType.Odbc:
                        return ($"TINYINT");

                    default:
                        throw new ArgumentOutOfRangeException(nameof(sqlType), sqlType, null);
                }
            }
            else if (type == typeof(byte[]))
            {

                switch (sqlType)
                {
                    case DataBaseType.SqlServer:
                        return ($"VARBINARY (MAX)");
                    case DataBaseType.MySql:
                        return ($"BLOB");
                    case DataBaseType.Sqlite:
                        return ($"BLOB");
                    case DataBaseType.Oracle:
                        break;
                    case DataBaseType.Oledb:
                        break;
                    case DataBaseType.Access95:
                        return ($"VARBINARY");
                    case DataBaseType.Odbc:
                        return ($"VARBINARY (MAX)");

                    default:
                        throw new ArgumentOutOfRangeException(nameof(sqlType), sqlType, null);
                }
            }
            else if (type == typeof(Guid))
            {

                switch (sqlType)
                {
                    case DataBaseType.SqlServer:
                        return ($"uniqueidentifier");
                    case DataBaseType.MySql:
                        return ($"CHAR(16)");
                    case DataBaseType.Sqlite:
                        break;
                    case DataBaseType.Oracle:
                        break;
                    case DataBaseType.Oledb:
                        break;
                    case DataBaseType.Access95:
                        return ($"VARCHAR(32)");
                    case DataBaseType.Odbc:
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(sqlType), sqlType, null);
                }
            }
            else if (type == typeof(char))
            {

                switch (sqlType)
                {
                    case DataBaseType.SqlServer:
                        return ($"CHARACTER");
                    case DataBaseType.MySql:
                        return ($"CHAR");
                    case DataBaseType.Sqlite:
                        return ($"TEXT");
                    case DataBaseType.Oracle:
                        break;
                    case DataBaseType.Oledb:
                        break;
                    case DataBaseType.Access95:
                        return ($"CHAR");
                    case DataBaseType.Odbc:
                        return ($"CHAR");
                    default:
                        throw new ArgumentOutOfRangeException(nameof(sqlType), sqlType, null);
                }
            }

            else
            {

                switch (sqlType)
                {
                    case DataBaseType.SqlServer:
                        return ($"VARCHAR (MAX)");
                    case DataBaseType.MySql:
                        return ($"VARCHAR (MAX)");
                    case DataBaseType.Sqlite:
                        return ($"TEXT");
                    case DataBaseType.Oracle:
                        break;
                    case DataBaseType.Oledb:
                        break;
                    case DataBaseType.Access95:
                        return ($"TEXT");
                    case DataBaseType.Odbc:
                        return ($"VARCHAR(50000)");
                    default:
                        throw new ArgumentOutOfRangeException(nameof(sqlType), sqlType, null);
                }
            }


            return null;
        }





       




    }
}