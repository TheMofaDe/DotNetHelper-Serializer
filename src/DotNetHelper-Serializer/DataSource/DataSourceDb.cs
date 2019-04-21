using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using DotNetHelper_Contracts.CustomException;
using DotNetHelper_Contracts.Enum.DataSource;
using DotNetHelper_Contracts.Enum.IO;
using DotNetHelper_Contracts.EventHandler;
using DotNetHelper_Contracts.Extension;
using DotNetHelper_Contracts.Helpers.LinqToSQL;
using DotNetHelper_Contracts.Helpers.OneOffs;
using DotNetHelper_Contracts.Xml;
using DotNetHelper_IO;
using DotNetHelper_Serializer.Extension;
using DotNetHelper_Serializer.Helper;
using DotNetHelper_Serializer.Interface;
using DotNetHelper_Serializer.Model;
using DotNetHelper_Serializer.Tools;
using AdvanceMember = DotNetHelper_Serializer.Extension.AdvanceMember;
using ExtFastMember = DotNetHelper_Serializer.Extension.ExtFastMember;
using SqlColumnAttritube = DotNetHelper_Serializer.Attribute.SqlColumnAttritube;
using SqlTableAttritube = DotNetHelper_Serializer.Attribute.SqlTableAttritube;

// ReSharper disable  InheritdocConsiderUsage
namespace DotNetHelper_Serializer.DataSource
{



    public class DataSourceDb : IDataSourceDb
    {

        /// <summary>
        /// Sometimes you don't want users to see the exact message. Setting this to true will throw a non-informant exception
        /// </summary>
        /// <value><c>true</c> if [throw custom exceptions]; otherwise, <c>false</c>.</value>
        public bool ThrowCustomExceptions { get; set; }
        /// <summary>
        /// Gets or sets the timeout for sql command not the connection itself.
        /// </summary>
        /// <value>The timeout.</value>
        public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(10);
        /// <summary>
        /// Database To Connect TO
        /// </summary>
        /// <value>The database.</value>
        public string Database { get; set; } = "";
        /// <summary>
        /// Connection String That Will Used To Connect To Server
        /// </summary>
        /// <value>The connection string.</value>
        public string ConnectionString { get; set; } = null;
        /// <summary>
        /// Server Name Or IP thats hosting An Sql Instance
        /// </summary>
        /// <value>The server.</value>
        public string Server { get; set; } = "";
        /// <summary>
        /// Use Integrated Security AKA Windows Authentication
        /// </summary>
        /// <value><c>true</c> if [integrated security]; otherwise, <c>false</c>.</value>
        public bool IntegratedSecurity { get; set; } = false;

        /// <summary>
        /// if T1 is true Bulk Insert Will Always be used when the object counts is greater than or equal to  T2
        /// </summary>
        public (bool UseBulkInsert, int minimumRecordRequiredToUse) AlwaysUseBulkInsert { get; set; } = (false, 0);

        /// <summary>
        /// Username that will be used if connection string is null or empty
        /// </summary>
        /// <value>The name of the user.</value>
        public string UserName { get; set; } = "";
        /// <summary>
        /// Password that will be used if connection string is null or empty
        /// </summary>
        /// <value>The password.</value>
        public string Password { get; set; } = "";
        /// <summary>
        /// Set true if Instance is SQLExpress
        /// </summary>
        /// <value><c>true</c> if this instance is SQL express; otherwise, <c>false</c>.</value>
        public bool IsSqlExpress { get; set; } = false;
        /// <summary>
        /// TableSchema To Append to table name
        /// </summary>
        /// <value>The table schema.</value>
        public string TableSchema { get; set; } = "";


        /// <summary>
        ///  Only Used By OLEDB Connections
        /// </summary>
        public bool PersistSecurityInfo { get; set; } = false;

        /// <summary>
        ///  Only Used By OLEDB Connections  
        /// </summary>
        public string JetOledbSystemDatabase { get; set; }

        /// <summary> 
        /// Only Used BY OLEDB Connections 
        /// </summary>
        public string Provider { get; set; }

        /// <summary>
        /// Create Table If It Doesn't Exist When Excuting Queries Only Works For Dynamic Execute Methods
        /// </summary>
        /// <value><c>true</c> if [create tables if not exist]; otherwise, <c>false</c>.</value>
        public bool CreateTablesIfNotExist { get; set; } = false;
        /// <summary>
        /// Create Schema If It Doesn't Exist When Excuting Queries
        /// </summary>
        /// <value><c>true</c> if [create schema if not exist]; otherwise, <c>false</c>.</value>
        public bool CreateSchemaIfNotExist { get; set; } = false;
        /// <summary>
        /// Rollback Any Batch Data If One Record Fails
        /// </summary>
        /// <value><c>true</c> if [enable transaction rollback]; otherwise, <c>false</c>.</value>
        public bool EnableTransactionRollback { get; set; } = false;
        /// <summary>
        /// Gets or sets the port.
        /// </summary>
        /// <value>The port.</value>
        public int? Port { get; set; } = null;



        // ReSharper enable  InconsistentNaming
        /// <summary>
        /// Gets or sets the QueryBucketManger.
        /// </summary>
        /// <value>The settings.</value>
        public QueryBucketManager QueryBucketManager { get; set; }

        /// <summary>
        /// Full File Name To The .MDF File
        /// </summary>
        /// <value>The full name of the file.</value>
        public string FullFileName { get; set; }
        /// <summary>
        /// Gets or sets the dbtype.
        /// </summary>
        /// <value>The dbtype.</value>
        public DataBaseType DBTYPE { get; }

        /// <summary>
        /// Gets or sets the properties.
        /// </summary>
        /// <value>The properties.</value>
        public DataSourceProperties Properties { get; set; } = new DataSourceProperties() { };

        /// <summary>
        /// Gets or sets the Json Serializer for columns that stores its value as json
        /// </summary>
        public IJsonSerializer JsonSerializer { get; set; } = new DataSourceJson();
        /// <summary>
        /// Gets or sets the Xml Serializer for columns that stores its value as xml
        /// </summary>
        public IXmlSerializer XmlSerializer { get; set; } = new DataSourceXml(DataSourceXml.SettingsType.DataTransfer);
        /// <summary>
        /// Gets or sets the Csv Serializer for columns that stores its value as csv
        /// </summary>
        public ICsvSerializer CsvSerializer { get; set; } = new DataSourceCsv();


        public ObjectToSqlHelper ObjectSqlHelper { get; }

        public SqlSyntaxHelper SqlSyntaxHelper { get; }


        public DateTime? LastConnectionOpenTime { get; set; }


        public delegate void SqlExceptionEventHandler(object sender, SqlExceptionEventArgs e);
        public event EventHandler OnSqlException;
        protected virtual void OnSqlExeception(SqlExceptionEventArgs e)
        {
            OnSqlException?.Invoke(this, e);
        }

        /// <summary>
        /// DEFAULTS TO SQLSERVER
        /// </summary>
        /// <param name="dbcommand"></param>
        /// <param name="options">The options.</param>
        /// <param name="dbConnection"></param>
        public DataSourceDb(DataBaseType dbType = DataBaseType.SqlServer, QueryBucketManager options = null)
        {

            QueryBucketManager = options ?? new QueryBucketManager();
            DBTYPE = dbType;
            //if (dbType == DataBaseType.Sqlite)
            //    throw new NotImplementedException("Please Use This Code For All Sqlite Usage : var db = new DataSourceSqlite(); ");


            ObjectSqlHelper = new ObjectToSqlHelper(dbType);
            SqlSyntaxHelper = new SqlSyntaxHelper(dbType);
        }






        #region Dynamically Build Connection String

        /// <summary>
        /// Build a SqlConnection String Based On DataSource Properties Will AutoBuild A Connection String If An Connection String Is Not Already Defined
        /// </summary>
        /// <returns>connection string</returns>
        public virtual string BuildConnectionString()
        {
            return DBHelper.BuildConnectionString(this);
        }





        #endregion



        #region Table Names




        private string AddBrackets(string content)
        {

            if (!content.StartsWith(SqlSyntaxHelper.GetTableOpenChar()))
            {
                content = $"{SqlSyntaxHelper.GetTableOpenChar()}{content}";
            }
            if (!content.EndsWith(SqlSyntaxHelper.GetTableClosedChar()))
            {
                content = $"{content}{SqlSyntaxHelper.GetTableClosedChar()}";
            }

            return content;
        }

        private string RemoveBrackets(string content)
        {

            if (content.StartsWith(SqlSyntaxHelper.GetTableOpenChar()))
            {
                content = content.ReplaceFirstOccurrance(SqlSyntaxHelper.GetTableOpenChar(), string.Empty, StringComparison.Ordinal);
            }
            if (content.EndsWith(SqlSyntaxHelper.GetTableClosedChar()))
            {
                content = content.ReplaceLastOccurrance(SqlSyntaxHelper.GetTableClosedChar(), string.Empty, StringComparison.Ordinal);
            }

            return content;
        }


        internal TableNameParseObject GetParseObject(string table, bool includeBrackets)
        {
            var tableNameParseObject = new TableNameParseObject();
            if (table.Contains("."))
            {
                var splits = table.Split('.');
                if (splits.Length == 3) // database.schema.table
                {
                    tableNameParseObject.DatabaseName = includeBrackets ? AddBrackets(splits[0]) : RemoveBrackets(splits[0]);
                    tableNameParseObject.SchemaName = includeBrackets ? AddBrackets(splits[1]) : RemoveBrackets(splits[1]);
                    tableNameParseObject.TableName = includeBrackets ? AddBrackets(splits[2]) : RemoveBrackets(splits[2]);
                }
                else if (splits.Length == 2) // schema.table
                {
                    tableNameParseObject.SchemaName = includeBrackets ? AddBrackets(splits[0]) : RemoveBrackets(splits[0]);
                    tableNameParseObject.TableName = includeBrackets ? AddBrackets(splits[1]) : RemoveBrackets(splits[1]);

                }
                else if (splits.Length == 1) // .table
                {
                    tableNameParseObject.TableName = includeBrackets ? AddBrackets(splits[0]) : RemoveBrackets(splits[0]);

                }
            }
            else
            {
                tableNameParseObject.TableName = table;
            }
            return tableNameParseObject;

        }


        ///// <summary>
        ///// Gets the full name of the table.
        ///// </summary>
        ///// <param name="tableName">Name of the table.</param>
        ///// <param name="includeSchema"></param>
        ///// <param name="includeBrackets"></param>
        ///// <param name="includeDatabase"></param>
        ///// <returns>System.String.</returns>
        private string FormatTableNameString(string tableName, bool includeDatabase, bool includeSchema, bool includeBrackets)
        {
            if (string.IsNullOrEmpty(tableName)) return "";

            var obj = GetParseObject(tableName, includeBrackets);
            var db = string.IsNullOrEmpty(obj.DatabaseName) ? "" : $"{obj.DatabaseName}.";
            var schema = string.IsNullOrEmpty(obj.SchemaName) ? "" : $"{obj.SchemaName}.";
            var table = string.IsNullOrEmpty(obj.TableName) ? "" : $"{obj.TableName}";
            if (includeDatabase && includeSchema)
            {
                return $"{db}{schema}{table}";
            }
            else if (!includeDatabase && includeSchema)
            {
                return $"{schema}{table}";
            }
            else if (includeDatabase)
            {
                return $"{db}{table}";
            }
            else
            {
                return $"{obj.TableName}";
            }


        }


        public virtual string GetDefaultTableName<T>(string tableName, bool includeDatabase = true, bool includeSchema = true, bool includeBrackets = true)
        {

            if (!string.IsNullOrEmpty(tableName)) // developer passed in table name
            {
                return FormatTableNameString(tableName, includeDatabase, includeSchema, includeBrackets);
            }
            var type = typeof(T);
            while (type.IsEnumerable())
            {
                type = type.GetEnumerableItemType();
            }


            var attr = type.CustomAttributes.Where(data => data.AttributeType == typeof(SqlTableAttritube));
            if (!attr.IsNullOrEmpty())
            {

                var value = attr.First().NamedArguments.First(arg => arg.MemberName == "TableName").TypedValue.Value.ToString();
                return FormatTableNameString(value, includeDatabase, includeSchema, includeBrackets);
            }
            else
            {
                //  return typeof(T).Name;

                return FormatTableNameString(typeof(T).Name, includeDatabase, includeSchema, includeBrackets);
            }
        }

        public virtual string GetDefaultTableName(Type type, string tableName, bool includeDatabase = true, bool includeSchema = true, bool includeBrackets = true)
        {

            if (!string.IsNullOrEmpty(tableName)) // developer passed in table name
            {
                return FormatTableNameString(tableName, includeDatabase, includeSchema, includeBrackets);
            }


            while (type.IsEnumerable())
            {
                type = type.GetEnumerableItemType();
            }

            var attr = type.CustomAttributes.Where(data => data.AttributeType == typeof(SqlTableAttritube));
            if (!attr.IsNullOrEmpty())
            {

                var value = attr.First().NamedArguments.First(arg => arg.MemberName == "TableName").TypedValue.Value?.ToString();
                if (string.IsNullOrEmpty(value))
                    return FormatTableNameString(type.Name, includeDatabase, includeSchema, includeBrackets);
                return FormatTableNameString(value, includeDatabase, includeSchema, includeBrackets);
            }
            else
            {
                //  return typeof(T).Name;
                return FormatTableNameString(type.Name, includeDatabase, includeSchema, includeBrackets);
            }
        }

        public virtual string GetDefaultTableName(string tableName, bool includeDatabase = true, bool includeSchema = true, bool includeBrackets = true)
        {

            if (!string.IsNullOrEmpty(tableName)) // developer passed in table name
            {
                return FormatTableNameString(tableName, includeDatabase, includeSchema, includeBrackets);
            }
            throw new BadCodeException("Table Name is missing");
        }


        #endregion





        public virtual DbParameter GetNewParameter(string parmeterName, object value)
        {

            return DBHelper.GetNewParameter(this, parmeterName, value ?? DBNull.Value);

        }
        public virtual IDbConnection GetNewConnection(bool openConnection, bool throwOnFailOpenConnection)
        {
            var conn = DBHelper.GetDbConnection(this);
            if (!openConnection)
            {

                return conn;
            }
            if (throwOnFailOpenConnection)
            {
                conn.Open();
                LastConnectionOpenTime = DateTime.Now;
                return conn;
            }

            try
            {
                conn.Open();
                LastConnectionOpenTime = DateTime.Now;
                return conn;
            }
            catch (Exception)
            {
                // callback for failed to open connection to sql
                return null;
            }
        }
        public virtual IDbCommand GetNewCommand(string cmdText = null, IDbConnection connection = null, IDbTransaction dbTransaction = null)
        {
            return DBHelper.GetDbCommand(this, cmdText, connection, dbTransaction);
        }

        public Tuple<string, string, string> GetDataSourceMetaDataQueries()
        {
            switch (DBTYPE)
            {
                case DataBaseType.SqlServer:
                    var getDBsQuery = " SELECT name, dbid, crdate as CreatedDate, filename, version FROM sys.sysdatabases";
                    var getDbTablesQuery = "SELECT name as TableName, schema_id as SchemaId, parent_object_id as parentobjectid, type as TableType, type_desc as TypeDesc, create_date as CreatedDate, modify_date as ModifiedDate FROM sys.tables";
                    var getDbSchemasQuery = " SELECT name, schema_id as SchemaId, principal_id as PrincipalID FROM sys.schemas";
                    return new Tuple<string, string, string>(getDBsQuery, getDbTablesQuery, getDbSchemasQuery);
                case DataBaseType.MySql:
                    break;
                case DataBaseType.Sqlite:
                    var getDBQuery = "SELECT name FROM sqlite_master WHERE type='table';";
                    return new Tuple<string, string, string>(getDBQuery, getDBQuery, getDBQuery);
                case DataBaseType.Oracle:
                    break;
                case DataBaseType.Oledb:
                    break;
                case DataBaseType.Access95:
                    var Access95GetDBs = " ";
                    var Access95GetTables = "SELECT name as TableName, ParentId as ParentObjectId, DateCreate as CreatedDate, DateUpdate as ModifiedDate FROM MSysObjects Where Type = 1";
                    var Access95GetSchemas = "";
                    return new Tuple<string, string, string>(Access95GetDBs, Access95GetTables, Access95GetSchemas);
                case DataBaseType.Odbc:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return new Tuple<string, string, string>("", "", "");
        }



        /// <inheritdoc />
        /// <summary>
        /// Returns true if the refresh was succesful
        /// </summary>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool RefreshDataSourceProperties()
        {

            var queries = GetDataSourceMetaDataQueries();
            var getDBsQuery = queries.Item1;
            var getDbTablesQuery = queries.Item2;
            var getDbSchemasQuery = queries.Item3;
            try
            {
                using (var connection = GetNewConnection(true, true))
                {

                    var bucket = LogConnectionTime(connection, getDBsQuery);
                    var bucket1 = LogConnectionTime(connection, getDbTablesQuery);
                    var bucket2 = LogConnectionTime(connection, getDbSchemasQuery);
                    using (var cmd = GetNewCommand(getDBsQuery, connection))
                    {
                        var reader = cmd.ExecuteReader();
                        bucket.ExecutedSuccesfully = true;
                        Properties.DataBases = reader.MapToList<Db>();
                    }
                    using (var cmd = GetNewCommand(getDbTablesQuery, connection))
                    {
                        var reader = cmd.ExecuteReader();
                        bucket1.ExecutedSuccesfully = true;
                        Properties.Tables = reader.MapToList<DbTable>();
                    }
                    using (var cmd = GetNewCommand(getDbTablesQuery, connection))
                    {
                        var reader = cmd.ExecuteReader();
                        bucket2.ExecutedSuccesfully = true;
                        Properties.Schemas = reader.MapToList<DbSchema>();
                    }
                    Properties.LastSyncTime = DateTime.Now;
                    return true;
                }
            }
            catch (Exception error)
            {
                throw ErrorHandling(error);
            }

        }








        /// <summary>
        /// Determines whether this instance can connect.
        /// </summary>
        /// <returns><c>true</c> if this instance can connect; otherwise, <c>false</c>.</returns>
        public bool CanConnect()
        {
            return GetNewConnection(true, false) != null;
        }




        public bool RecordExist<T>(T obj, string tableName, Expression<Func<T, object>> overrideKey = null) where T : class
        {
            var sqlBuilder = new StringBuilder();
            var keyFields = ObjectSqlHelper.GetKeyFields<T>(obj);
            // Build If Exists statement
            sqlBuilder.Append($"IF EXISTS ( SELECT * FROM {tableName} ");
            ObjectSqlHelper.BuildWhereClause(sqlBuilder, keyFields, true);
            sqlBuilder.Append(" ) BEGIN SELECT TRUE ");

            // Build Update or Insert statement
            ObjectSqlHelper.BuildUpdateQuery(sqlBuilder, tableName, obj, overrideKey);
            sqlBuilder.Append(" END ELSE BEGIN SELECT FALSE ");

            sqlBuilder.Append(" END");
            var reader = ExecuteManualQuery(sqlBuilder.ToString());
            if (reader.HasRows().HasValue)
            {
                using (reader)
                {
                    return reader.GetBoolean(0);
                }
            }
            else
            {
                if (reader == null || reader.IsClosed) return false;
                using (reader)
                {
                    return reader.GetBoolean(0);
                }
            }


        }















        #region Handling Properties 



        /// <summary>
        /// Gets the non key fields.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>List&lt;AdvanceMember&gt;.</returns>
        private List<AdvanceMember> GetSubTables<T>() where T : class
        {
            if (typeof(IDynamicMetaObjectProvider).IsAssignableFrom(typeof(T)))
            {
                // Get non primary key fields - the ones we want to update.
                return ExtFastMember.GetAdvanceMembersForDynamic<T>().Where(m => m.SqlCustomAttritube.Ignore != true && m.SqlTableAttritube != null).ToList();
            }
            else
            {
                // Get non primary key fields - the ones we want to update.
                return ExtFastMember.GetAdvanceMembers<T>().Where(m => m.SqlCustomAttritube.Ignore != true && m.SqlTableAttritube != null).ToList();
            }

        }

        /// <summary>
        /// Gets the non key fields.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>List&lt;AdvanceMember&gt;.</returns>
        private List<AdvanceMember> GetSubTables(Type type)
        {
            if (typeof(IDynamicMetaObjectProvider).IsAssignableFrom(type))
            {
                // Get non primary key fields - the ones we want to update.
                return ExtFastMember.GetAdvanceMembersForDynamic(type).Where(m => m.SqlCustomAttritube.Ignore != true && m.SqlTableAttritube != null).ToList();
            }
            else
            {
                // Get non primary key fields - the ones we want to update.
                return ExtFastMember.GetAdvanceMembers(type).Where(m => m.SqlCustomAttritube.Ignore != true && m.SqlTableAttritube != null).ToList();
            }

        }




        #endregion



        #region Excuting Methods


        /// <summary>
        /// Gets the specified table name.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="whereClause">The where clause.</param>
        /// <returns>IDataReader.</returns>
        public IDataReader Get(string tableName, string whereClause = null)
        {
            return ExecuteQuery(tableName, whereClause, ObjectSqlHelper.BuildGetQuery);
            //   return ExecuteQuery(tableName, whereClause, BuildGetQuery).MapToList<T>(NullType.Null);
        }


        ///// <summary>
        ///// Gets the specified table name.
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="tableName">Name of the table.</param>
        ///// <param name="whereClause">The where clause.</param>
        ///// <returns>List&lt;T&gt;.</returns>
        //public List<T> Get<T>(string tableName = null, string whereClause = null) where T : class
        //{
        //    tableName  = GetDefaultTableName<T>(tableName);
        //    if (CreateTablesIfNotExist)
        //        CreateTableFromClass<T>(tableName);  // Create Table If Doesn't Exist 
        //    return ExecuteQuery(tableName, whereClause, BuildGetQuery).MapToList<T>();
        //}


        /// <summary>
        /// Get the Top 1 record where the primary key(s) matches if matching record exist then return default value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">only use for key mapping against table</param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public T GetTopOne<T>(T obj, string tableName = null) where T : class
        {
            tableName = GetDefaultTableName<T>(tableName);
            if (CreateTablesIfNotExist)
                CreateTableFromClass<T>(tableName);
            var keysFields = ObjectSqlHelper.GetKeyFields(obj).Where(b => b.SqlCustomAttritube.PrimaryKey == true).ToList();
            var sb = new StringBuilder();
            var parameters = ObjectSqlHelper.BuildWhereClauseAndGeDbParameters(GetNewParameter, sb, keysFields, true);

            return ExecuteManualQuery($"SELECT TOP 1 * FROM {tableName} {sb}", parameters).MapToList<T>().FirstOrDefault();
        }




        #region WORK IN PROGRESS

        /// <summary>
        /// Gets the specified table name.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="whereClause">The where clause.</param>
        /// <returns>List&lt;T&gt;.</returns>
        public List<T> Get<T>(string tableName = null, string whereClause = null) where T : class
        {
            tableName = GetDefaultTableName<T>(tableName);
            if (CreateTablesIfNotExist)
                CreateTableFromClass<T>(tableName);  // Create Table If Doesn't Exist 
                                                     //// TODO :: COME BACK THEMOFADE AND IMPLEMENT THIS
                                                     //     var advanceMembers = ExtFastMember.GetAdvanceMembers<T>();
                                                     //     
                                                     //     var R = BuildJoinString<T>(tableName).ToString();
                                                     //     // Logic to populate a property that is actually a SQL Table
                                                     //     if (!advanceMembers.Where(a => a.SqlTableAttritube != null).IsNullOrEmpty()) // We have secondary Tables
                                                     //     {
                                                     //         var preData = ExecuteQuery(tableName, whereClause, BuildGetQuery<T>).MapToList<T>();
                                                     //     
                                                     //         try
                                                     //         {
                                                     //     
                                                     //             var keysToJoinOn = advanceMembers.Where(mm => !mm.SqlCustomAttritube.MappingIds.IsNullOrEmpty()).ToList(); // Keys For Main Table Join
                                                     //             if (keysToJoinOn.IsNullOrEmpty()) throw new BadCodeException($"Alright Buddy, For The Type {typeof(T).FullName} You have properties with [SQLTableAttribute] but no other properties" +
                                                     //                                                                                           $" have the [SQLColumnAttribute] with MappingIds Set You need to the set mappings Ids");
                                                     //             //secondKeysToJoinOn
                                                     //             advanceMembers.Where(b => b.SqlTableAttritube != null).ToList().ForEach(delegate (AdvanceMember classAsAPropertyType)
                                                     //             {
                                                     //                 var type = classAsAPropertyType.Member.Type;
                                                     //                 if (type.IsTypeIEnumerable())
                                                     //                 {
                                                     //                     type = type.GetIEnumerableRealType();
                                                     //                 }
                                                     //                 else
                                                     //                 {
                                                     //     
                                                     //                 }
                                                     //                 var secondMembers = ExtFastMember.GetAdvanceMembers(type);
                                                     //                 var sb = new StringBuilder($"SELECT B.* FROM {tableName} A INNER JOIN {classAsAPropertyType.SqlTableAttritube.TableName ?? type.Name} B ON ");
                                                     //                 var sbWhereClause = new StringBuilder($" WHERE ");
                                                     //                 var safeKeyword = "";
                                                     //                 var WheresafeKeyword = "";
                                                     //                 var safeLetter = 'B';
                                                     //                 keysToJoinOn.ForEach(delegate (AdvanceMember temp1) // Now need to a married couple 
                                                     //                 {
                                                     //                     var result = secondMembers.Where(z1 => z1.SqlCustomAttritube.MappingIds.ContainAnySameItem(temp1.SqlCustomAttritube.MappingIds)).ToList();
                                                     //                     result.ForEach(delegate (AdvanceMember z2)
                                                     //                     {
                                                     //                         sb.Append($" A.{temp1.SqlCustomAttritube.MapTo ?? temp1.Member.Name} = {safeLetter}.{z2.SqlCustomAttritube.MapTo ?? z2.Member.Name} ");
                                                     //                         sb.Append(safeKeyword);
                                                     //                         safeKeyword = (" AND ");
                                                     //     
                                                     //                     });
                                                     //     
                                                     //                     if (string.IsNullOrEmpty(safeKeyword)) throw new BadCodeException($"DFA... Your're Missing MappingIds For The Type {type.FullName} To Join With the SQL Table {tableName}");
                                                     //     
                                                     //                     if (temp1.Member.Type == typeof(int) || temp1.Member.Type == typeof(bool)) // THIS IS KINDA BAD BUT I DONT SEE A KEY WHERE JOINING ON NOT BEING A DAMN INTERGER
                                                     //                 {
                                                     //                         sbWhereClause.Append($"A.{temp1.SqlCustomAttritube.MapTo ?? temp1.Member.Name} = {temp1.Value}{safeKeyword}");
                                                     //                     }
                                                     //                     else
                                                     //                     {
                                                     //                         sbWhereClause.Append($"A.{temp1.SqlCustomAttritube.MapTo ?? temp1.Member.Name} = '{temp1.Value}'{safeKeyword}");
                                                     //                     }
                                                     //                     safeLetter = Alphabet.GetNextLetter(safeLetter);
                                                     //     
                                                     //                 });
                                                     //     
                                                     //                 var secondaryDataSql = sb.ToString().ReplaceLastOccurrance(safeKeyword, string.Empty, StringComparison.Ordinal);
                                                     //                 secondaryDataSql += sbWhereClause.ToString().ReplaceLastOccurrance(safeKeyword, string.Empty, StringComparison.Ordinal);
                                                     //                 var NWMI = ExecuteManualQuery(secondaryDataSql).MapToList(type);
                                                     //     
                                                     //                 var accessor = TypeAccessor.Create(type, true);
                                                     //     
                                                     //             });
                                                     //     
                                                     //     
                                                     //     
                                                     //         }
                                                     //         catch (Exception error) { Logger?.LogError(error); return preData; }
                                                     //         //var secondaryData = ExecuteManualQuery($"SELECT * FROM {type.Name} WHERE {classAsAPropertyType.SqlCustomAttritube}")
                                                     //     }
            var datareader = ExecuteQuery(tableName, whereClause, ObjectSqlHelper.BuildGetQuery);
            return datareader.MapToList<T>(JsonSerializer, XmlSerializer, CsvSerializer);
        }


        /// <summary>
        /// T1 is sql & T2 is split on 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataBaseType"></param>
        /// <returns></returns>
        public (string sql, string splitOn) BuildJoinString<T>() where T : class
        {

            void ThrowIfAttributesIsMissing(List<AdvanceMember> keys, Type type)
            {
                if (keys.IsNullOrEmpty())
                {
                    throw new BadCodeException($"Can not succesfully build sql if your don't set your matching attriubtes and no id columns name are shared between parent & child of type {type.Name}");
                }
            }
            // var safetly = $@"_TEMP_";
            var tableName = GetDefaultTableName<T>(null);

            var columnIndexMappingForDataReader = new Dictionary<int, Tuple<string, string>>();
            var sqlBuilder = new StringBuilder("SELECT ");
            var sqlFromBuilder = new StringBuilder();
            var advanceMembers = ExtFastMember.GetAdvanceMembers<T>();
            var tableAliasLookup = ObjectSqlHelper.GetTableAliasRecursive<T>();
#if NETFRAMEWORK
            var mainTableAlias = tableAliasLookup.GetValueOrDefault(typeof(T));
#else
            var mainTableAlias = tableAliasLookup.GetValueOrDefaultValue(typeof(T));
#endif
            var currentTableAlias = mainTableAlias;
            var index = 0;
            var splitOn = new List<string>() { };
            var openChar = SqlSyntaxHelper.GetTableOpenChar();
            var closeChar = SqlSyntaxHelper.GetTableClosedChar();

            var columnsAndSplitOnTuple = ObjectSqlHelper.BulidSelectColumnStatement(currentTableAlias, advanceMembers, SqlSyntaxHelper);
            //splitOn.Add(columnsAndSplitOnTuple.Item2);
            sqlBuilder.Append(columnsAndSplitOnTuple.Item1);



            sqlFromBuilder.Append($" FROM {tableName} {mainTableAlias} ");
            var keysToJoinOn = advanceMembers.Where(mm => !mm.SqlCustomAttritube.MappingIds.IsNullOrEmpty()).ToList(); // Keys For Main Table Join
            var canThrow = false;


            var list = advanceMembers.Where(a1 => a1.SqlTableAttritube != null).ToList();
            var i = 1;
            list.ForEach(delegate (AdvanceMember tableMember) // BUILD SECONDARY COLUMNS
            {
                canThrow = true;
                currentTableAlias = Alphabet.GetNextLetter(currentTableAlias); // Every Member For Now On Is Actually A Table

                var tableType = tableMember.Member.Type;
                if (tableType.IsTypeIEnumerable())
                {
                    tableType = tableType.GetEnumerableItemType();
                }

                var tableAdvanceMember = ExtFastMember.GetAdvanceMembers(tableType); // Columns For SECOND TABLE 

                // SAFE COMMENT START
                var xRefType = tableMember.SqlTableAttritube.XReferenceTable;

                if (xRefType != null)
                {

                    var xRefMembers = ExtFastMember.GetAdvanceMembers(xRefType);
                    // TODO :: QUESTION ?? DO WE ALWAYS WANT TO MAKE THE XREF JOIN THE SAME AS THE TABLE 
                    sqlFromBuilder.AppendLine($"{tableMember.SqlTableAttritube.JoinType} JOIN {GetDefaultTableName(xRefType, null)} {currentTableAlias} ON "); // build from clause
                    ThrowIfAttributesIsMissing(keysToJoinOn, xRefType);
                    ObjectSqlHelper.BuildJoinOnStatement(keysToJoinOn, Alphabet.GetPreviousLetter(currentTableAlias), xRefMembers, currentTableAlias, sqlFromBuilder);

                    currentTableAlias = Alphabet.GetNextLetter(currentTableAlias); // FAST FORWARD BECAUSE OF XREF TABLE

                    sqlFromBuilder.AppendLine($"{tableMember.SqlTableAttritube.JoinType} JOIN {GetDefaultTableName(tableType, null)} {currentTableAlias} ON "); // build from clause
                    ThrowIfAttributesIsMissing(xRefMembers, tableType);
                    ObjectSqlHelper.BuildJoinOnStatement(xRefMembers, Alphabet.GetPreviousLetter(currentTableAlias), tableAdvanceMember, currentTableAlias, sqlFromBuilder);

                }
                else
                {

                    sqlFromBuilder.AppendLine($"{tableMember.SqlTableAttritube.JoinType} JOIN {GetDefaultTableName(tableMember.Member.Type, null)} {currentTableAlias} ON "); // build from clause
                    ThrowIfAttributesIsMissing(keysToJoinOn, xRefType);
                    ObjectSqlHelper.BuildJoinOnStatement(keysToJoinOn, Alphabet.GetPreviousLetter(currentTableAlias), tableAdvanceMember, currentTableAlias, sqlFromBuilder);
                }
                // SAFE COMMENT END



                columnsAndSplitOnTuple = ObjectSqlHelper.BulidSelectColumnStatement(currentTableAlias, tableAdvanceMember, SqlSyntaxHelper);
                splitOn.Add(columnsAndSplitOnTuple.Item2);

                if (i < list.Count)
                {

                    // sqlBuilder.ReplaceLastOccurrance(",", "", StringComparison.OrdinalIgnoreCase);
                    sqlBuilder.Append(columnsAndSplitOnTuple.Item1);
                }
                else
                {
                    sqlBuilder.Append(columnsAndSplitOnTuple.Item1.ReplaceLastOccurrance(",", "", StringComparison.OrdinalIgnoreCase));
                    Console.WriteLine(currentTableAlias);
                }

                i++;
            });

            if (canThrow)
                if (keysToJoinOn.IsNullOrEmpty())
                    throw new BadCodeException(
                        $"Alright Buddy, For The Type {typeof(T).FullName} You have properties with [SQLTableAttribute] but no other properties" +
                        $" have the [SQLColumnAttribute] with MappingIds Set You need to the set mappings Ids this drive how join sql syntax is dynamically built");



            // FINISH SELECTING COLUMNS FROM ALL TABLES


            sqlBuilder.Append(sqlFromBuilder);
            return (sqlBuilder.ToString(), string.Join(",", splitOn));
        }


        #endregion


        /// <summary>
        /// Gets the specified table name data with dynamically building your where clause from your paramter object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="dynamicParameters"></param>
        /// <returns>List&lt;T&gt;.</returns>
        public List<T> GetWithParameters<T>(string tableName, ExpandoObject dynamicParameters, string whereClause = null) where T : class
        {

            try
            {
                tableName = GetDefaultTableName<T>(tableName);
                if (CreateTablesIfNotExist)
                    CreateTableFromClass<T>(tableName); // Create Table If Doesn't Exist 


                var tuple = ObjectSqlHelper.BuildWhereClause(GetNewParameter, dynamicParameters, whereClause);
                var datareader = ExecuteQuery(tableName, tuple.Item2, ObjectSqlHelper.BuildGetQuery, tuple.Item1);
                return datareader.MapToList<T>(JsonSerializer, XmlSerializer, CsvSerializer);
            }
            catch (Exception error)
            {
                throw ErrorHandling(error);
            }

        }







        ///// <summary>
        ///// Gets the specified table name.
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="tableName">Name of the table.</param>
        ///// <param name="dynamicParameters"></param>
        ///// <param name="whereClause">The where clause.</param>
        ///// <returns>List&lt;T&gt;.</returns>
        //public List<T> GetWithParameters<T>(string tableName, ExpandoObject dynamicParameters, string whereClause) where T : class
        //{
        //    tableName = GetDefaultTableName<T>(tableName);
        //    if (CreateTablesIfNotExist)
        //        CreateTableFromClass<T>(tableName);  // Create Table If Doesn't Exist 

        //    var list = new List<DbParameter>() { };
        //    var dictionary = DynamicObjectHelper.GetProperties(dynamicParameters);
        //    if (dictionary != null && dictionary.Any())
        //    {
        //        dictionary.ForEach(delegate (KeyValuePair<string, object> pair)
        //        {
        //            list.Add(GetNewParameter(pair.Key, pair.Value));
        //        });
        //    }


        //    var datareader = ExecuteQuery(tableName, whereClause, BuildGetQuery, list);
        //    return datareader.MapToList<T>(JsonSerializer, XmlSerializer, CsvSerializer);
        //}

        /// <summary>
        /// Gets the specified T data filtering using the Linq Expression.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="whereExpression"></param>
        /// <param name="tableName">Name of the table.</param>
        /// <returns>List&lt;T&gt;.</returns>
        public List<T> GetLinq<T>(Expression<Func<T, bool>> whereExpression, string tableName = null) where T : class
        {
            try
            {
                tableName = GetDefaultTableName<T>(tableName);
                if (CreateTablesIfNotExist)
                    CreateTableFromClass<T>(tableName);  // Create Table If Doesn't Exist 



                var whereClause = Generator.WhereSql(whereExpression).Sql;
                return ExecuteQuery(tableName, whereClause, ObjectSqlHelper.BuildGetQuery).MapToList<T>();
            }
            catch (Exception error)
            {
                throw ErrorHandling(error);
            }

        }


        /// <summary>
        /// Executes the stored procedure.
        /// </summary>
        /// <param name="procedureName">Name of the procedure.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>IDataReader.</returns>
        public IDataReader ExecuteStoredProcedure(string procedureName, List<DbParameter> parameters)
        {
            try
            {
                // DO NOT WRAP THIS IN A USING STATEMENT DATAREADER CAN NOT BE RETURN IF THE CONNECTION IS CLOSE
                var conn = GetNewConnection(false, false);
                {
                    conn.Open();
                    var bucketSql = LogConnectionTime(conn, "Executing Stored Procedure " + procedureName);
                    using (var cmd = GetNewCommand(procedureName, conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.AddParameters(parameters);
                        var reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);// Connection Will Close With The Datareader Is Closed
                        bucketSql.ExecutedSuccesfully = true;
                        return reader;
                    }
                }
            }
            catch (Exception error)
            {
                throw ErrorHandling(error, $"EXEC {procedureName}");
            }
        }


        /// <summary>
        /// Connection String Must Be Valid To Create
        /// </summary>
        /// <param name="fullFilePath">The full file path.</param>
        /// <param name="databaseName">Name of the database.</param>
        /// <param name="overwrite">if set to <c>true</c> [overwrite].</param>
        /// <returns>System.Int32.</returns>
        public int CreateLocalDatabaseFile(string fullFilePath, string databaseName, bool overwrite = false)
        {
            var file = new FileObject(fullFilePath);
            if (string.IsNullOrEmpty(databaseName)) databaseName.IsNullThrow(nameof(databaseName), new ArgumentNullException(nameof(databaseName)));
            var sql = $@"
                                   CREATE DATABASE
                                       [{databaseName}]
                                   ON PRIMARY (
                                      NAME={databaseName},
                                      FILENAME = '{file.FilePathOnly}{file.FileNameOnly}'
                                   )
                                   LOG ON (
                                       NAME={databaseName}_log,
                                       FILENAME = '{file.FilePathOnly}{file.FileNameOnlyNoExtension}_DBLogs'
                                   )    ";
            try
            {
                if (file.Exist == true)
                {
                    if (overwrite)
                    {
                        file.DeleteFile(e => throw e);
                    }
                    else
                    {
                        return 0;
                    }
                }

                using (var connection = GetNewConnection(true, true))
                {
                    // TODO :: only if on the same server var folderObject = new FolderObject(file.FilePathOnly).Create();
                    connection.Open();

                    using (var command = GetNewCommand(sql, connection))
                    {
                        return command.ExecuteNonQuery();
                    }

                }
            }
            catch (Exception error)
            {
                throw ErrorHandling(error, sql);
            }
        }


        /// <summary>
        /// Executes the non query.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="poco">The poco.</param>
        /// <param name="buildSqlString">The build SQL string.</param>
        /// <returns>System.Int32.</returns>
        private int ExecuteNonQuery<T>(string tableName, T poco, Action<StringBuilder, string, T, Expression<Func<T, object>>> buildSqlString, Expression<Func<T, object>> overrideKeys) where T : class
        {
            var query = "";
            try
            {
                tableName = GetDefaultTableName<T>(tableName);

                var sqlBuilder = new StringBuilder();
                buildSqlString(sqlBuilder, tableName, poco, overrideKeys);
                using (var conn = GetNewConnection(false, true))
                {
                    conn.Open();
                    query = sqlBuilder.ToString();
                    var bucket = LogConnectionTime(conn, query);
                    using (var cmd = GetNewCommand(query, conn))
                    {
                        cmd.CommandTimeout = (int)Timeout.TotalSeconds;
                        var dbParameters = ObjectSqlHelper.BuildDbParameterList(poco, GetNewParameter, XmlSerializer, JsonSerializer, CsvSerializer);
                        cmd.AddParameters(dbParameters);
                        if (QueryBucketManager.IncludeReadableQuery)
                            bucket.ReadableQuery = cmd.Parameters.ParamToSql(sqlBuilder.ToString());
                        var result = cmd.ExecuteNonQuery();
                        bucket.ExecutedSuccesfully = true;
                        return result;
                    }
                }
            }
            catch (Exception error)
            {
                if (error.Message.StartsWith($@"The INSERT statement conflicted with the FOREIGN KEY constraint"))
                {
                    //  var sb = new StringBuilder();
                    //  sb.AppendLine("Don't Feel To Bad This Library Can Handle Foreign Keys For You But You Need The Following Code To Get Started");
                    //  sb.AppendLine($"var db = new DataSourceSqlServer() {{ \"ConnectionString = {BuildConnectionString()} \" }}");
                    //  var a1  = error.Message.Split(new[] { "The conflict occurred in database" }, StringSplitOptions.None); 
                    //  var a2 = a1[1].Split(new[] { ", table" }, StringSplitOptions.None);
                    //  var realTableName = a2[1].Split(new[] { ", column" }, StringSplitOptions.None)[0].ToString().Replace("\"","");
                    //  // TODO :: Find Out What Happens When This Happens With Object That Have The Schema Not Empty Or Null
                    //  var content = ScriptTableToCSharpClass(realTableName);
                    //  sb.AppendLine(" Be Sure To Add This Model To Your Project " + content);
                    //  Logger?.ConsoleAndLog("Don't Feel To Bad This Library Can Handle Foreign Keys For You But You Need The Following Code To Get Started"
                    //      + $"var db = new DataSourceSqlServer() {{ ConnectionString = {BuildConnectionString()} }}");
                }
                throw ErrorHandling(error, query);
            }
        }



        /// <summary>CandyXrefMultMappingColumns
        /// Executes the non query.
        /// </summary>
        /// <param name="sql">The SQL.</param>
        /// <returns>System.Int32.</returns>
        private int ExecuteNonQuery(string sql)
        {
            try
            {
                using (var conn = GetNewConnection(false, true))
                {
                    conn.Open();
                    var bucket = LogConnectionTime(conn, sql);
                    using (var cmd = GetNewCommand(sql, conn))
                    {
                        cmd.CommandTimeout = (int)Timeout.TotalSeconds;
                        var result = cmd.ExecuteNonQuery();
                        bucket.ExecutedSuccesfully = true;
                        return result;
                    }
                }
            }
            catch (Exception error)
            {
                throw ErrorHandling(error, sql);
            }

        }


        /// <summary>
        /// Generates the scripts.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type">The type.</param>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="listPoco">The list poco.</param>
        /// <param name="buildSqlString">The build SQL string.</param>
        /// <returns>Numbers Of Rows Affected</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// a
        private List<string> GenerateScripts<T>(ScriptType type, string tableName, IEnumerable<T> listPoco, Action<StringBuilder, string, T, Expression<Func<T, object>>> buildSqlString, Expression<Func<T, object>> overrideKeys = null) where T : class
        {
            var scriptsParam = new List<string>() { };
            var scriptsHuman = new List<string>() { };


            tableName = GetDefaultTableName<T>(tableName);


            foreach (var poco in listPoco)
            {
                var sqlBuilder = new StringBuilder();
                buildSqlString(sqlBuilder, tableName, poco, overrideKeys);
                var query = sqlBuilder.ToString();
                var cmd = GetNewCommand();
                var dbParameters = ObjectSqlHelper.BuildDbParameterList(poco, GetNewParameter, XmlSerializer, JsonSerializer, CsvSerializer);
                cmd.Parameters.AddRange(dbParameters);

                if (type == ScriptType.Parameterized)
                    scriptsParam.Add(query);
                if (type == ScriptType.HumanReadable)
                    scriptsHuman.Add(cmd.Parameters.ParamToSql(query));
            }


            if (type == ScriptType.Parameterized)
                return scriptsParam;
            if (type == ScriptType.HumanReadable)
                return scriptsHuman;
            throw new ArgumentOutOfRangeException();
        }


        /// <summary>
        /// Retrieve A DataReader Of All Records From The Specified Query Please Make Sure You Closed The DataReader When No longer need
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>IDataReader.</returns>
        /// <exception cref="NotImplementedException">Sorry Haven't implemented this yet</exception>
        public IDataReader ExecuteManualQuery(string query)
        {
            try
            {
                // DO NOT WRAP THIS IN A USING STATEMENT DATAREADER CAN NOT BE RETURN IF THE CONNECTION IS CLOSE
                var conn = GetNewConnection(false, true);
                {
                    conn.Open();
                    var bucket = LogConnectionTime(conn, query);
                    using (var cmd = GetNewCommand(query, conn))
                    {
                        if (QueryBucketManager.IncludeReadableQuery)
                            bucket.ReadableQuery = cmd.Parameters.ParamToSql(query);
                        var reader = cmd.ExecuteReader(CommandBehavior.CloseConnection); // Connection Will Close With The Datareader Is Closed
                        bucket.ExecutedSuccesfully = true;
                        return reader;
                    }
                }
            }
            catch (Exception error)
            {
                throw ErrorHandling(error, query);
            }
            throw new NotImplementedException("Sorry Haven't implemented this yet");
        }


        /// <summary>
        /// Retrieve A DataReader Of All Records From The Specified Query Please Make Sure You Closed The DataReader When No longer need
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>IDataReader.</returns>
        /// <exception cref="NotImplementedException">Sorry Haven't implemented this yet</exception>
        public IDataReader ExecuteManualQuery(IDbConnection connection, string query, CommandBehavior behavior = CommandBehavior.CloseConnection)
        {
            try
            {
                // DO NOT WRAP THIS IN A USING STATEMENT DATAREADER CAN NOT BE RETURN IF THE CONNECTION IS CLOSE

                connection.OpenSafely();
                var bucket = LogConnectionTime(connection, query);
                // using
                var cmd = GetNewCommand(query, connection);
                {
                    if (QueryBucketManager.IncludeReadableQuery)
                        bucket.ReadableQuery = cmd.Parameters.ParamToSql(query);
                    var reader = cmd.ExecuteReader(behavior); // Connection Will Close With The Datareader Is Closed
                    bucket.ExecutedSuccesfully = true;
                    return reader;
                }


            }
            catch (Exception error)
            {
                throw ErrorHandling(error, query);
            }
            throw new NotImplementedException("Sorry Haven't implemented this yet");
        }


        private IDataReader ExecuteNonQueryGetDataReader<T>(string tableName, T poco, Action<StringBuilder, string, Expression<Func<T, object>>> buildSqlString, Expression<Func<T, object>> overrideKeys = null) where T : class
        {
            var query = "";
            try
            {
                tableName = GetDefaultTableName<T>(tableName);

                var sqlBuilder = new StringBuilder();
                buildSqlString(sqlBuilder, tableName, overrideKeys);
                // DO NOT WRAP THIS IN A USING STATEMENT DATAREADER CAN NOT BE RETURN IF THE CONNECTION IS CLOSE
                var conn = GetNewConnection(false, true);
                {
                    conn.Open();
                    query = sqlBuilder.ToString();
                    var bucket = LogConnectionTime(conn, query);
                    using (var cmd = GetNewCommand(query, conn))
                    {
                        var DbParameters = ObjectSqlHelper.BuildDbParameterList(poco, GetNewParameter, XmlSerializer, JsonSerializer, CsvSerializer);
                        cmd.Parameters.AddRange(DbParameters.ToArray());
                        if (QueryBucketManager.IncludeReadableQuery)
                            bucket.ReadableQuery = cmd.Parameters.ParamToSql(query);
                        var reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);// Connection Will Close With The Datareader Is Closed
                        bucket.ExecutedSuccesfully = true;
                        return reader;
                    }
                }
            }

            catch (Exception error)
            {
                throw ErrorHandling(error, query);
            }
        }





        private T ExecuteNonQuery<T>(string tableName, T poco, Action<StringBuilder, string> buildSqlString) where T : class
        {
            var query = "";
            try
            {
                tableName = GetDefaultTableName<T>(tableName);

                var propertyName = ExtFastMember.GetAdvanceMembers<T>().First(a => a.SqlCustomAttritube.AutoIncrementBy != null && a.SqlCustomAttritube.AutoIncrementBy > 0).Member.Name;
                var sqlBuilder = new StringBuilder();
                buildSqlString(sqlBuilder, tableName);
                //// DO NOT WRAP THIS IN A USING STATEMENT DATAREADER CAN NOT BE RETURN IF THE CONNECTION IS CLOSE
                /// REVISED THIS SHOULD BE IN USING STATEMENT BECAUSE WE IS DONE I THINK THE COMMENT ABOVE WAS CAUSE BY COPY& PASTE
                using (var conn = GetNewConnection(false, true))
                {
                    conn.Open();
                    query = sqlBuilder.ToString();
                    var bucket = LogConnectionTime(conn, query);
                    using (var cmd = GetNewCommand(query, conn))
                    {
                        var DbParameters = ObjectSqlHelper.BuildDbParameterList(poco, GetNewParameter, XmlSerializer, JsonSerializer, CsvSerializer);
                        cmd.Parameters.AddRange(DbParameters.ToArray());
                        if (QueryBucketManager.IncludeReadableQuery)
                            bucket.ReadableQuery = cmd.Parameters.ParamToSql(query);
                        var reader = cmd.ExecuteReader();

                        bucket.ExecutedSuccesfully = true;
                        while (reader.Read())
                        {
                            ExtFastMember.SetMemberValue(poco, propertyName, reader.GetValue(0));
                            return poco;
                        }

                    }
                }
            }

            catch (Exception error)
            {
                throw ErrorHandling(error, query);
            }
            return poco;
        }





        private T ExecuteNonQuery<T>(IDbConnection connection, string tableName, T poco, Action<StringBuilder, string> buildSqlString) where T : class
        {
            var query = "";
            try
            {
                tableName = GetDefaultTableName<T>(tableName);

                var propertyName = ExtFastMember.GetAdvanceMembers<T>().First(a => a.SqlCustomAttritube.AutoIncrementBy != null && a.SqlCustomAttritube.AutoIncrementBy > 0).Member.Name;
                var sqlBuilder = new StringBuilder();
                buildSqlString(sqlBuilder, tableName);


                query = sqlBuilder.ToString();
                connection.OpenSafely();
                var bucket = LogConnectionTime(connection, query);

                using (var cmd = GetNewCommand(query, connection))
                {
                    var DbParameters = ObjectSqlHelper.BuildDbParameterList(poco, GetNewParameter, XmlSerializer, JsonSerializer, CsvSerializer);
                    cmd.Parameters.AddRange(DbParameters.ToArray());
                    if (QueryBucketManager.IncludeReadableQuery)
                        bucket.ReadableQuery = cmd.Parameters.ParamToSql(query);
                    var reader = cmd.ExecuteReader();

                    bucket.ExecutedSuccesfully = true;
                    while (reader.Read())
                    {
                        ExtFastMember.SetMemberValue(poco, propertyName, reader.GetValue(0));
                        return poco;
                    }

                }

            }

            catch (Exception error)
            {
                throw ErrorHandling(error, query);
            }
            return poco;
        }









        private T ExecuteNonQuery<T>(string tableName, T poco, Action<StringBuilder, string, Expression<Func<T, object>>> buildSqlString, Expression<Func<T, object>> overrideKeys = null) where T : class
        {
            var query = "";
            try
            {
                tableName = GetDefaultTableName<T>(tableName);

                var propertyName = ExtFastMember.GetAdvanceMembers<T>().First(a => a.SqlCustomAttritube.AutoIncrementBy != null && a.SqlCustomAttritube.AutoIncrementBy > 0).Member.Name;
                var sqlBuilder = new StringBuilder();
                buildSqlString(sqlBuilder, tableName, overrideKeys);
                //// DO NOT WRAP THIS IN A USING STATEMENT DATAREADER CAN NOT BE RETURN IF THE CONNECTION IS CLOSE
                /// REVISED THIS SHOULD BE IN USING STATEMENT BECAUSE WE IS DONE I THINK THE COMMENT ABOVE WAS CAUSE BY COPY& PASTE
                using (var conn = GetNewConnection(false, true))
                {
                    conn.Open();
                    query = sqlBuilder.ToString();
                    var bucket = LogConnectionTime(conn, query);
                    using (var cmd = GetNewCommand(query, conn))
                    {
                        var DbParameters = ObjectSqlHelper.BuildDbParameterList(poco, GetNewParameter, XmlSerializer, JsonSerializer, CsvSerializer);
                        cmd.Parameters.AddRange(DbParameters.ToArray());
                        if (QueryBucketManager.IncludeReadableQuery)
                            bucket.ReadableQuery = cmd.Parameters.ParamToSql(query);
                        var reader = cmd.ExecuteReader();
                        bucket.ExecutedSuccesfully = true;
                        while (reader.Read())
                        {
                            ExtFastMember.SetMemberValue(poco, propertyName, reader.GetValue(0));
                            return poco;
                        }

                    }
                }
            }

            catch (Exception error)
            {
                throw ErrorHandling(error, query);
            }
            return poco;
        }

        //static Expression<Func<object, object>> ConvertFunction<T>(Expression<Func<T, object>> function)
        //{
        //    ParameterExpression p = Expression.Parameter(typeof(object));

        //    return Expression.Lambda<Func<object, object>>
        //    (
        //        Expression.Invoke(function, Expression.Convert(p, typeof(T))), p
        //    );
        //}

        /// <summary>
        /// Executes the query.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="whereClause">The where clause.</param>
        /// <param name="buildSqlString">The build SQL string.</param>
        /// <returns>IDataReader.</returns>
        private IDataReader ExecuteQuery(string tableName, string whereClause, Action<StringBuilder, string, string> buildSqlString, List<DbParameter> parameters = null)
        {
            var query = "";
            try
            {
                tableName = GetDefaultTableName(tableName, true, true, true);
                var sqlBuilder = new StringBuilder();
                buildSqlString(sqlBuilder, tableName, whereClause);
                // DO NOT WRAP THIS IN A USING STATEMENT DATAREADER CAN NOT BE RETURN IF THE CONNECTION IS CLOSE
                var conn = GetNewConnection(false, true);
                {
                    conn.Open();
                    query = sqlBuilder.ToString();
                    var bucket = LogConnectionTime(conn, query);
                    // if (DBTYPE == DataBaseType.Sqlite) // https://github.com/aspnet/Microsoft.Data.Sqlite/issues/484
                    {
                        var cmd = GetNewCommand(query, conn);
                        {
                            cmd.AddParameters(parameters);
                            if (QueryBucketManager.IncludeReadableQuery)
                                bucket.ReadableQuery = cmd.Parameters.ParamToSql(query);
                            var reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);// Connection Will Close With The Datareader Is Closed
                            bucket.ExecutedSuccesfully = true;
                            return reader;
                        }
                    }
                    //using (var cmd = GetNewCommand(query, conn))
                    //{
                    //    cmd.AddParameters(parameters);
                    //    if (QueryBucketManager.IncludeReadableQuery)
                    //        bucket.ReadableQuery = cmd.Parameters.ParamToSql(query);
                    //    var reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);// Connection Will Close With The Datareader Is Closed
                    //    bucket.ExecutedSuccesfully = true;
                    //    return reader;
                    //}
                }
            }

            catch (Exception error)
            {
                throw ErrorHandling(error, query);
            }
        }

        /// <summary>
        /// Executes the manual query.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>IDataReader.</returns>
        public IDataReader ExecuteManualQuery(string query, List<DbParameter> parameters)
        {

            try
            {
                // DO NOT WRAP THIS IN A USING STATEMENT DATAREADER CAN NOT BE RETURN IF THE CONNECTION IS CLOSE
                var conn = GetNewConnection(false, true);
                {
                    conn.Open();
                    var bucket = LogConnectionTime(conn, query);
                    using (var cmd = GetNewCommand(query, conn))
                    {
                        cmd.AddParameters(parameters);
                        //cmd?.Parameters.AddWithValue(p.ParameterName, p.Value ?? DBNull.Value);



                        if (QueryBucketManager.IncludeReadableQuery)
                            bucket.ReadableQuery = cmd.Parameters.ParamToSql(query);
                        var reader = cmd.ExecuteReader(CommandBehavior.CloseConnection); // Connection Will Close With The Datareader Is Closed
                        bucket.ExecutedSuccesfully = true;
                        return reader;
                    }
                }
            }
            catch (Exception error)
            {
                throw ErrorHandling(error, query);
            }
        }


        /// <summary>
        /// Bulks the insert.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listPoco">The list poco.</param>
        /// <param name="tableName">Name of the table.</param>
        /// <returns>System.Int32.</returns>
        public int BulkInsert<T>(IEnumerable<T> listPoco, string tableName) where T : class
        {
            try
            {
                return DBHelper.BulkInsert(this, listPoco, tableName);
            }
            catch (Exception error)
            {
                throw ErrorHandling(error);
            }
        }

        /// <summary>
        /// Executes the non query on list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="listPoco">The list poco.</param>
        /// <param name="buildSqlString">The build SQL string.</param>
        /// <returns>Numbers Of Rows Affected</returns>
        private int ExecuteNonQueryOnList<T>(string tableName, IEnumerable<T> listPoco, Action<StringBuilder, string, T, Expression<Func<T, object>>> buildSqlString, ActionType type, Expression<Func<T, object>> overrideKeys = null) where T : class
        {
            try
            {
                if (AlwaysUseBulkInsert.Item1 && listPoco.Count() > AlwaysUseBulkInsert.Item2 && type == ActionType.Insert)
                {
                    return BulkInsert(listPoco, tableName);
                }

                return DBHelper.ExecuteNonQueryOnList(this, tableName, listPoco, buildSqlString, overrideKeys);
            }
            catch (Exception)
            {
                // throw ErrorHandling(error);
                return 1;
            }

        }

        /// <summary>
        /// Executes the manual SQL.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query">The query.</param>
        /// <returns>List&lt;T&gt;.</returns>
        public List<T> ExecuteManualSql<T>(string query) where T : class
        {

            try
            {

                using (var conn = GetNewConnection(false, true))
                {
                    conn.Open();
                    var bucket = LogConnectionTime(conn, query);
                    using (var cmd = GetNewCommand(query, conn))
                    {
                        cmd.CommandTimeout = (int)Timeout.TotalSeconds;
                        var reader = cmd.ExecuteReader();
                        bucket.ExecutedSuccesfully = true;
                        return reader.MapToList<T>(JsonSerializer, XmlSerializer, CsvSerializer);
                    }
                }

            }
            catch (Exception error)
            {
                throw ErrorHandling(error, query);
            }

        }

        /// <summary>
        /// Executes the manual non query SQL.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>System.Int32.</returns>
        public int ExecuteManualNonQuerySql(string query)
        {
            try
            {
                using (var conn = GetNewConnection(false, true))
                {
                    conn.Open();
                    var bucket = LogConnectionTime(conn, query);
                    using (var cmd = GetNewCommand(query, conn))
                    {
                        var reader = cmd.ExecuteNonQuery();
                        bucket.ExecutedSuccesfully = true;
                        return reader;
                    }
                }
            }
            catch (Exception error)
            {
                throw ErrorHandling(error, query);
            }

        }


        #endregion



        /// <summary>
        /// Creates the table from class.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="dropIfExist">if set to <c>true</c> [drop if exist].</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool CreateTableFromClass<T>(string tableName, bool dropIfExist = false) where T : class
        {

            try
            {
                var subTables = GetSubTables<T>();
                subTables.ForEach(delegate (AdvanceMember member)
                    {
                        CreateTableFromClass(member.Member.Type, null, dropIfExist);
                    });
                tableName = GetDefaultTableName<T>(tableName);

                var sqlBuilder = new StringBuilder();

                sqlBuilder.Append($"Create Table {tableName} ( ");
                var list = ObjectSqlHelper.GetAllNonIgnoreFields<T>();
                var keyFields = ObjectSqlHelper.GetKeyFields<T>();
                var hasMultipleKeys = keyFields.Count > 1;



                list.ForEach(delegate (AdvanceMember p)
                {
                    var value = DevelopersTools.AdvanceTypeToSqlType(p, DBTYPE, hasMultipleKeys);
                    sqlBuilder.Append($" [{p.GetActualMemberName()}] {value} ,");
                });

                sqlBuilder.Remove(sqlBuilder.Length - 1, 1); // Remove the last comma 
                if (hasMultipleKeys)
                {
                    sqlBuilder.Append($" PRIMARY KEY ( ");
                    keyFields.ForEach(a => sqlBuilder.Append("[" + (a.GetActualMemberName()) + "] , "));
                    sqlBuilder.Remove(sqlBuilder.Length - 2, 2);
                    sqlBuilder.Append($" ) ");
                }



                sqlBuilder.Append($")");

                var tableExist = TableExist(tableName);
                if (!tableExist)
                {
                    ExecuteNonQuery(sqlBuilder.ToString());
                }
                else if (dropIfExist)
                {
                    if (DropTable(tableName))
                        ExecuteNonQuery(sqlBuilder.ToString());
                }

                return true;
            }
            catch (BadCodeException badCodeException)
            {
                var isDynamic = (typeof(IDynamicMetaObjectProvider).IsAssignableFrom(typeof(T)));
                if (isDynamic) return true;
                throw badCodeException;
            }
            catch (Exception error)
            {

                if (error.Message.Contains($"There is already an object named '{tableName}' in the database."))
                {
                    return true;
                }
                if (error.Message.Contains($"table {tableName} already exists"))
                {
                    return true;
                }
                throw ErrorHandling(error);
            }
        }


        /// <summary>
        /// Creates the table from class.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="dropIfExist">if set to <c>true</c> [drop if exist].</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool CreateTableFromClass(Type type, string tableName, bool dropIfExist = false)
        {

            try
            {
                var subTables = GetSubTables(type);
                subTables.ForEach(delegate (AdvanceMember member)
                {
                    CreateTableFromClass(member.Member.Type, null, dropIfExist);
                });

                tableName = GetDefaultTableName(type, tableName);

                var sqlBuilder = new StringBuilder();

                sqlBuilder.Append($"Create Table {tableName} ( ");
                var list = ObjectSqlHelper.GetAllNonIgnoreFields(type);
                var keyFields = ObjectSqlHelper.GetKeyFields(type);
                var hasMultipleKeys = keyFields.Count > 1;



                list.ForEach(delegate (AdvanceMember p)
                {
                    var value = DevelopersTools.AdvanceTypeToSqlType(p, DBTYPE, hasMultipleKeys);
                    sqlBuilder.Append($" [{p.GetActualMemberName()}] {value} ,");
                });

                sqlBuilder.Remove(sqlBuilder.Length - 1, 1); // Remove the last comma 
                if (hasMultipleKeys)
                {
                    sqlBuilder.Append($" PRIMARY KEY ( ");
                    keyFields.ForEach(a => sqlBuilder.Append("[" + (a.GetActualMemberName()) + "] , "));
                    sqlBuilder.Remove(sqlBuilder.Length - 2, 2);
                    sqlBuilder.Append($" ) ");
                }



                sqlBuilder.Append($")");
                var tableExist = TableExist(tableName);
                if (!tableExist)
                {
                    ExecuteNonQuery(sqlBuilder.ToString());
                }
                else if (dropIfExist)
                {
                    if (DropTable(tableName))
                        ExecuteNonQuery(sqlBuilder.ToString());
                }

                return true;
            }
            catch (BadCodeException badCodeException)
            {
                var isDynamic = (typeof(IDynamicMetaObjectProvider).IsAssignableFrom(type));
                if (isDynamic) return true;
                throw ErrorHandling(badCodeException);
            }
            catch (Exception error)
            {

                if (error.Message.Contains($"There is already an object named '{tableName}' in the database."))
                {
                    return true;
                }
                if (error.Message.Contains($"table {tableName} already exists"))
                {
                    return true;
                }
                throw ErrorHandling(error);
            }
        }


        /// <summary>
        /// Creates the table from a dynamic object .
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="dropIfExist">if set to <c>true</c> [drop if exist].</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool CreateTableFromDynamicObject<T>(T dynamicObject, string tableName, bool dropIfExist = false) where T : class
        {
            try
            {
                if (string.IsNullOrEmpty(tableName)) throw new BadCodeException("Look Here. I Could Create A SQL Table For you based on your dynamic object but the very least you can do is provide me a table name smh..");
                var sqlBuilder = new StringBuilder();

                sqlBuilder.Append($"Create Table {GetDefaultTableName<T>(tableName, true, true, true)} ( ");
                var list = ExtFastMember.GetAdvanceMembersForDynamic(dynamicObject);


                list.ForEach(delegate (AdvanceMember p)
                {
                    var value = DevelopersTools.AdvanceTypeToSqlType(p, DBTYPE, false);
                    sqlBuilder.Append($" [{p.Member.Name}] {value} ,");
                });

                sqlBuilder.Remove(sqlBuilder.Length - 1, 1); // Remove the last comma DOESN'T APPEAR TO BE NESSACARY
                sqlBuilder.Append($")");

                var tableExist = TableExist(tableName);
                if (!tableExist)
                {
                    ExecuteNonQuery(sqlBuilder.ToString());
                }
                else if (dropIfExist)
                {
                    if (DropTable(tableName))
                        ExecuteNonQuery(sqlBuilder.ToString());
                }

                return true;
            }
            catch (Exception error)
            {
                if (error.Message.Contains($"There is already an object named '{tableName}' in the database."))
                {
                    return true;
                }
                if (error.Message.Contains($"table {tableName} already exists"))
                {
                    return true;
                }
                throw ErrorHandling(error);
            }
        }



        /// <summary>
        /// Drops the table.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <returns><c>true</c> if a table was drop then return true, <c>false</c> otherwise.</returns>
        public bool DropTable(string tableName)
        {
            //var formattedTableName = GetDefaultTableName(tableName, false, true, true);
            //var sql = SqlSyntaxHelper.BuildIfExistStatement(
            //$"SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'{formattedTableName}') AND type in (N'U')",
            //$"DROP TABLE {formattedTableName}",null);
            //var result = ExecuteManualNonQuerySql(sql);
            //return true;
            if (TableExist(tableName))
            {
                var result = ExecuteManualNonQuerySql($" DROP TABLE {tableName} ");
                return true;
            }
            // ExecuteManualNonQuerySql($"IF EXISTS ( SELECT * FROM {tableName} ) DROP TABLE {tableName} ") == 1;
            return false;

        }


        /// <summary>
        /// Checks If Schema Exist
        /// </summary>
        /// <param name="schema">The schema.</param>
        /// <param name="createIfFalse">if set to <c>true</c> [create if false].</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool SchemaExist(string schema, bool createIfFalse = false)
        {
            var query = "";
            try
            {
                if (string.IsNullOrEmpty(schema)) return true;
                var realSchema = schema.Replace("[", "").Replace("]", "");
                query = $"IF NOT EXISTS ( SELECT * FROM sys.schemas WHERE name = N'{realSchema}') ";
                if (!createIfFalse)
                {
                    query += $"select 'FALSE' else select 'TRUE'";
                }
                else
                {
                    query += $" EXEC('CREATE SCHEMA {realSchema} ') else select 'TRUE'";
                }

                using (var conn = GetNewConnection(false, true))
                {
                    conn.Open();
                    var bucket = LogConnectionTime(conn, query);
                    using (var cmd = GetNewCommand(query, conn))
                    {
                        cmd.CommandTimeout = (int)Timeout.TotalSeconds;
                        var reader = cmd.ExecuteReader();
                        if (reader.HasRows().GetValueOrDefault(true))
                            while (reader.Read())
                            {
                                var row = reader.GetString(0);
                                var exist = Convert.ToBoolean(row);
                                bucket.ExecutedSuccesfully = true;
                                return exist;
                            }
                        return false;
                    }

                }
            }
            catch (Exception error)
            {
                throw ErrorHandling(error, query);
            }

        }



        /// <summary>
        /// Check If Table Exist Must Include Schema
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool TableExist(string tableName)
        {
            var query = "";
            try
            {
                switch (DBTYPE)
                {
                    case DataBaseType.SqlServer:
                        query = $"IF OBJECT_ID(N'{GetDefaultTableName(tableName, true, true, true)}', N'U') IS NOT NULL SELECT 'TRUE' ELSE SELECT 'FALSE'";  // SQL SERVER DOESN'T CARE IF BRACKETS ARE INCLUDED OR NOT
                        break;
                    case DataBaseType.MySql:
                        query = $"SELECT CASE WHEN COUNT(*) > 0 THEN 'TRUE' ELSE 'FALSE' END FROM information_schema.tables " +
                                $" WHERE table_schema = '{GetParseObject(tableName, false).SchemaName}'" +
                                $" AND table_name = '{GetParseObject(tableName, false).TableName}' " +
                                $" LIMIT 1;";
                        break;
                    case DataBaseType.Sqlite:
                        query = $"SELECT CASE WHEN COUNT(( SELECT name FROM sqlite_master WHERE type='table' AND name='{GetParseObject(tableName, false).TableName}')) > 0 THEN 'TRUE' ELSE 'FALSE' END AS WHOCARES";
                        break;
                    case DataBaseType.Oracle:
                        break;
                    case DataBaseType.Oledb:
                        break;
                    case DataBaseType.Access95:
                        break;
                    case DataBaseType.Odbc:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                using (var conn = GetNewConnection(false, true))
                {
                    conn.Open();
                    //  using (var trans = conn.BeginTransaction())
                    //  {
                    var bucket = LogConnectionTime(conn, query);
                    using (var cmd = GetNewCommand(query, conn))
                    {
                        cmd.CommandTimeout = (int)Timeout.TotalSeconds;
                        var reader = cmd.ExecuteReader();
                        if (reader.HasRows().GetValueOrDefault(true))
                            while (reader.Read())
                            {
                                var row = reader.GetString(0);
                                var exist = Convert.ToBoolean(row);
                                bucket.ExecutedSuccesfully = true;
                                //     trans.Commit();
                                conn.Close();
                                return exist;
                            }
                    }
                    // }
                }
            }
            catch (Exception error)
            {
                throw ErrorHandling(error, query);
            }
            return false;
        }



        /// <summary>
        /// Errors the handling.
        /// </summary>
        /// <param name="error">The error.</param>
        /// <returns>Exception.</returns>
        public Exception ErrorHandling(Exception error)
        {
            OnSqlException?.Invoke(this, new SqlExceptionEventArgs(error));
            return ThrowCustomExceptions ? new Exception("An Internal Error Has Occur.") : error;
        }

        /// <summary>
        /// Errors the handling.
        /// </summary>
        /// <param name="error">The error.</param>
        /// <returns>Exception.</returns>
        public Exception ErrorHandling(Exception error, string sql)
        {
            OnSqlException?.Invoke(this, new SqlExceptionEventArgs(error, sql));
            return ThrowCustomExceptions ? new Exception("An Internal Error Has Occur.") : error;
        }


        /// <summary>
        /// Logs the connection time.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="query">The query.</param>
        /// <returns>QueryBucket.</returns>
        public virtual QueryBucket LogConnectionTime(IDbConnection connection, string query)
        {
            return DBHelper.LogConnectionTime(this, connection, query);
        }















        /// <summary>
        /// Executes the dynamic query.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type">Type Of Query To Execute</param>
        /// <param name="poco">The Object To Execute Against</param>
        /// <param name="tableName">Name of the table.</param>
        /// <returns>Returns Record Affected</returns>
        /// <exception cref="ArgumentOutOfRangeException">type - null</exception>
        public int ExecuteDynamicQuery<T>(ActionType type, IEnumerable<T> poco, string tableName = null, Expression<Func<T, object>> overrideKeys = null) where T : class
        {
            if (poco == null) return 0;
            var listPoco = poco as IList<T> ?? poco.ToList();  // Need To Test List Of List <T> here and see where the exeception gets throwned
            if (!listPoco.Any()) return 0;

            var action = new Action<StringBuilder, string, T, Expression<Func<T, object>>>(delegate (StringBuilder builder, string s, T arg3, Expression<Func<T, object>> arg4) { });
            tableName = GetDefaultTableName<T>(tableName);
            switch (type)
            {
                case ActionType.Insert:
                    action = ObjectSqlHelper.BuildInsertQuery;
                    break;
                case ActionType.Update:
                    action = ObjectSqlHelper.BuildUpdateQuery;
                    break;
                case ActionType.Upsert:
                    action = ObjectSqlHelper.BuildUpsertQuery;
                    break;
                case ActionType.Delete:
                    action = ObjectSqlHelper.BuildDeleteQuery;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
            if (CreateTablesIfNotExist)
                CreateTableFromClass<T>(tableName);  // Create Table If Doesn't Exist 

            var first = listPoco.Count() == 1 ? ExecuteNonQuery(tableName, listPoco.First(), action, overrideKeys) : ExecuteNonQueryOnList(tableName, listPoco, action, type);

            return first;

        }








        /// <summary>
        /// Executes the dynamic query.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type">The type.</param>
        /// <param name="poco">The poco.</param>
        /// <param name="tableName">Name of the table.</param>
        /// <returns>System.Int32.</returns>
        /// <exception cref="Exception">If Your Going To ExecuteDyamicQuery Against A List You Must Specified The Type  EG ::  database.ExecuteDynamicQuery<THIS_IS_WHAT_YOUR_MISSING>(DatabaseMaster.ActionType.INSERT, list);</exception>
        /// <exception cref="ArgumentOutOfRangeException">type - null</exception>
        public int ExecuteDynamicQuery<T>(ActionType type, T poco, string tableName = null, Expression<Func<T, object>> overrideKeys = null) where T : class
        {
            var action = new Action<StringBuilder, string, T, Expression<Func<T, object>>>(delegate (StringBuilder builder, string s, T arg3, Expression<Func<T, object>> arg4) { });
            if (poco is IEnumerable)
            {
                if (typeof(IDynamicMetaObjectProvider).IsAssignableFrom(typeof(T)))
                {
                    // Handle Dynamic Object
                    if (CreateTablesIfNotExist)
                        if (!CreateTableFromDynamicObject<T>(poco, tableName)) ErrorHandling(new Exception($"Failed To Create Table {tableName}"));  // Create Table If Doesn't Exist 
                    switch (type)
                    {
                        case ActionType.Insert:
                            action = ObjectSqlHelper.BuildInsertQuery;
                            break;
                        case ActionType.Update:
                            action = ObjectSqlHelper.BuildUpdateQuery;
                            break;
                        case ActionType.Upsert:
                            action = ObjectSqlHelper.BuildUpsertQuery;
                            break;
                        case ActionType.Delete:
                            action = ObjectSqlHelper.BuildDeleteQuery;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(type), type, null);
                    }
                    return ExecuteNonQuery(tableName, poco, action, overrideKeys);
                }
                else
                {
                    throw new Exception("If Your Going To ExecuteDyamicQuery Against A List You Must Specified The Type  EG ::  database.ExecuteDynamicQuery<THIS_IS_WHAT_YOUR_MISSING>(ActionType.INSERT, list);");
                }
            }
            tableName = GetDefaultTableName<T>(tableName);
            switch (type)
            {
                case ActionType.Insert:
                    action = ObjectSqlHelper.BuildInsertQuery;
                    break;
                case ActionType.Update:
                    action = ObjectSqlHelper.BuildUpdateQuery;
                    break;
                case ActionType.Upsert:
                    action = ObjectSqlHelper.BuildUpsertQuery;
                    break;
                case ActionType.Delete:
                    action = ObjectSqlHelper.BuildDeleteQuery;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            if (CreateTablesIfNotExist)
                if (!CreateTableFromClass<T>(tableName)) ErrorHandling(new Exception($"Failed To Create Table {tableName}"));  // Create Table If Doesn't Exist 
            return ExecuteNonQuery(tableName, poco, action, overrideKeys);
        }



        // TODO :: COME BACK THEMOFADE & FINSIH THIS 
        private int ExecuteDynamicQueryMultipleTables<T>(ActionType type, T poco, string tableName = null, Expression<Func<T, object>> overrideKeys = null) where T : class
        {
            var action = new Action<StringBuilder, string, T, Expression<Func<T, object>>>(delegate (StringBuilder builder, string s, T arg3, Expression<Func<T, object>> arg4) { });
            if (poco is IEnumerable)
                throw new Exception("If Your Going To ExecuteDynamicQuery Against A List You Must Specified The Type  EG ::  database.ExecuteDynamicQuery<THIS_IS_WHAT_YOUR_MISSING>(DatabaseMaster.ActionType.INSERT, list);");
            tableName = GetDefaultTableName<T>(tableName);
            switch (type)
            {
                case ActionType.Insert:
                    action = ObjectSqlHelper.BuildInsertQuery;
                    break;
                case ActionType.Update:
                    action = ObjectSqlHelper.BuildUpdateQuery;
                    break;
                case ActionType.Upsert:
                    action = ObjectSqlHelper.BuildUpsertQuery;
                    break;
                case ActionType.Delete:
                    action = ObjectSqlHelper.BuildDeleteQuery;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            if (CreateTablesIfNotExist)
                if (!CreateTableFromClass<T>(tableName)) ErrorHandling(new Exception($"Failed To Create Table {tableName}"));  // Create Table If Doesn't Exist 
            return ExecuteNonQuery(tableName, poco, action, overrideKeys);
        }


        /// <summary>
        /// Executes the dynamic query. And Returns A DataReader With Outputs
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type">The type.</param>
        /// <param name="poco">The poco.</param>
        /// <param name="tableName">Name of the table.</param>
        /// <returns>System.Int32.</returns>
        /// <exception cref="Exception">If Your Going To ExecuteDyamicQuery Against A List You Must Specified The Type  EG ::  database.ExecuteDynamicQuery<THIS_IS_WHAT_YOUR_MISSING>(DatabaseMaster.ActionType.INSERT, list);</exception>
        /// <exception cref="ArgumentOutOfRangeException">type - null</exception>
        public IDataReader ExecuteDynamicQueryWithOutputs<T>(ActionType type, T poco, Expression<Func<T, object>> expression, string tableName = null) where T : class
        {
            var action = new Action<StringBuilder, string, Expression<Func<T, object>>>(delegate (StringBuilder builder, string s, Expression<Func<T, object>> ex) { });
            if (poco is IEnumerable)
                throw new BadCodeException("If Your Going To ExecuteDyamicQuery Against A List You Must Specified The Type  EG ::  database.ExecuteDynamicQuery<THIS_IS_WHAT_YOUR_MISSING>(DatabaseMaster.ActionType.INSERT, list);");
            tableName = GetDefaultTableName<T>(tableName);
            switch (type)
            {
                case ActionType.Insert:
                    action = ObjectSqlHelper.BuildInsertQueryWithOutputs;
                    break;
                case ActionType.Update:
                    throw new NotImplementedException("Joseph Haven't Had Time To Implement This Functionality Yet");
                // action = BuildUpdateQuery;
                case ActionType.Upsert:
                    //   action = BuildUpsertQuery;
                    throw new NotImplementedException("Joseph Haven't Had Time To Implement This Functionality Yet");
                case ActionType.Delete:
                    //   action = BuildDeleteQuery;
                    throw new NotImplementedException("Joseph Haven't Had Time To Implement This Functionality Yet");
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            if (CreateTablesIfNotExist)
                if (!CreateTableFromClass<T>(tableName)) ErrorHandling(new Exception($"Failed To Create Table {tableName}"));  // Create Table If Doesn't Exist 
            return ExecuteNonQueryGetDataReader<T>(tableName, poco, action, expression);
        }



        /// <summary>
        /// Executes the dynamic query. And Returns A DataReader With Outputs
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type">The type.</param>
        /// <param name="poco">The poco.</param>
        /// <param name="tableName">Name of the table.</param>
        /// <returns>System.Int32.</returns>
        /// <exception cref="Exception">If Your Going To ExecuteDyamicQuery Against A List You Must Specified The Type  EG ::  database.ExecuteDynamicQuery<THIS_IS_WHAT_YOUR_MISSING>(DatabaseMaster.ActionType.INSERT, list);</exception>
        /// <exception cref="ArgumentOutOfRangeException">type - null</exception>
        public T ExecuteDynamicQueryReturnIdentity<T>(ActionType type, T poco, string tableName = null) where T : class
        {
            var action = new Action<StringBuilder, string>(delegate (StringBuilder builder, string s) { });
            if (poco is IEnumerable)
                throw new BadCodeException("If Your Going To ExecuteDyamicQuery Against A List You Must Specified The Type  EG ::  database.ExecuteDynamicQuery<THIS_IS_WHAT_YOUR_MISSING>(DatabaseMaster.ActionType.INSERT, list);");
            tableName = GetDefaultTableName<T>(tableName);
            switch (type)
            {
                case ActionType.Insert:
                    action = ObjectSqlHelper.BuildInsertQueryWithOutputs<T>;
                    break;
                case ActionType.Update:
                    throw new NotImplementedException("Joseph Haven't Had Time To Implement This Functionality Yet");
                case ActionType.Upsert:
                    throw new NotImplementedException("Joseph Haven't Had Time To Implement This Functionality Yet");
                case ActionType.Delete:
                    throw new NotImplementedException("Joseph Haven't Had Time To Implement This Functionality Yet");
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            if (CreateTablesIfNotExist)
                if (!CreateTableFromClass<T>(tableName)) ErrorHandling(new Exception($"Failed To Create Table {tableName}"));  // Create Table If Doesn't Exist 

            if (DBTYPE == DataBaseType.Sqlite)
            {
                // TODO :: DBConnection THIS WILL ONLY WORK INTEGERS 
                var sharedConnection = GetNewConnection(true, true);
                var identityFields = ExtFastMember.GetAdvanceMembers(poco).First(m => m.SqlCustomAttritube.PrimaryKey == true && m.SqlCustomAttritube.Ignore != true &&
                    m.SqlTableAttritube == null);

                var t = ExecuteNonQuery<T>(sharedConnection, tableName, poco, action);


                var sql = $"SELECT last_insert_rowid();";
                var value = ExecuteManualQuery(sharedConnection, sql).MapToList<string>().First();
                ExtFastMember.SetMemberValue(poco, identityFields.FastMember.Name, value);
                return poco;
            }
            else
            {
                var t = ExecuteNonQuery<T>(tableName, poco, action);
                return t;
            }

        }


        public void BulkInsertDataTable(DataTable table, SqlBulkCopyOptions options, string destinationTableName = null, Action<SqlRowsCopiedEventArgs> callback = null)
        {

            if (table != null && table.Rows.Count > 0)
                try
                {
                    using (var bulkCopy = new SqlBulkCopy(BuildConnectionString(), options))
                    {
                        bulkCopy.BatchSize = (int)table.Rows.Count;
                        bulkCopy.DestinationTableName = destinationTableName ?? table.TableName;
                        bulkCopy.ColumnMappings.Clear();
                        foreach (var column in table.Columns)
                        {
                            bulkCopy.ColumnMappings.Add(column.ToString(), column.ToString());
                        }
                        bulkCopy.SqlRowsCopied += delegate (object sender, SqlRowsCopiedEventArgs args)
                        {
                            callback?.Invoke(args);
                        };
#if NETFRAMEWORK
                        bulkCopy.WriteToServer(table);
#else

                        bulkCopy.WriteToServer(table.CreateDataReader());
#endif
                    }
                }
                catch (Exception e)
                {
                    throw ErrorHandling(e);
                }

        }



        /// <summary>
        /// Creates the scripts.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="scriptsType">Type of the scripts.</param>
        /// <param name="objects">The objects.</param>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="type">The type.</param>
        /// <returns>List&lt;System.String&gt;.</returns>
        /// <exception cref="ArgumentOutOfRangeException">type - null</exception>
        public List<string> CreateScripts<T>(ActionType scriptsType, List<T> objects, string tableName = null, ScriptType type = ScriptType.HumanReadable, Expression<Func<T, object>> overrideKeys = null) where T : class
        {
            var action = new Action<StringBuilder, string, T, Expression<Func<T, object>>>(delegate (StringBuilder builder, string s, T arg3, Expression<Func<T, object>> arg4) { });
            tableName = GetDefaultTableName<T>(tableName);
            switch (scriptsType)
            {
                case ActionType.Insert:
                    action = ObjectSqlHelper.BuildInsertQuery;
                    break;
                case ActionType.Update:
                    action = ObjectSqlHelper.BuildUpdateQuery;
                    break;
                case ActionType.Upsert:
                    action = ObjectSqlHelper.BuildUpsertQuery;
                    break;
                case ActionType.Delete:
                    action = ObjectSqlHelper.BuildDeleteQuery;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
            return GenerateScripts(type, tableName, objects, action, overrideKeys);
        }


        /// <summary>
        /// Dumps the table data to file.
        /// </summary>
        /// <param name="fullFilePath">The full file path.</param>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="option">The option.</param>
        /// <param name="whereClause">The where clause.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>

        public bool DumpTableDataToFile(string fullFilePath, string tableName, FileOption option, string whereClause = null)
        {


            var dataReader = Get(tableName, whereClause);

            if (dataReader.IsClosed)
            {
                //  Logger?.Log("Couldn't Dump Table To File Beacuse Its Has No Records");
                return false;
            }


            var fileObject = new FileObject(fullFilePath);
            using (var streamWriter = new StreamWriter(fileObject.GetFileStream(option), Encoding.UTF8))
            {

                while (dataReader.Read())
                {

                    streamWriter.Write($"INSERT INTO {tableName} ( ");
                    var sb = new StringBuilder();

                    for (var i = 0; i < dataReader.FieldCount; i++)
                    {
                        var columnName = dataReader.GetName(i);
                        streamWriter.Write(i == dataReader.FieldCount - 1 ? $"[{columnName}]" : $"[{columnName}],");

                        var type = dataReader.GetFieldType(i);
                        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                        {
                            type = Nullable.GetUnderlyingType(type);

                        }

                        if (dataReader[columnName] == DBNull.Value)
                        {
                            sb.Append("NULL,");
                            continue;
                        }
                        else if (type == typeof(byte[]))
                        {
                            var data = (byte[])dataReader[columnName];
                            sb.Append(i == dataReader.FieldCount ? $"{data}" : $"{data},");
                            continue;
                        }
                        else if (type == typeof(byte))
                        {
                            var data = (byte)dataReader[columnName];
                            sb.Append(i == dataReader.FieldCount ? $"{data}" : $"{data},");
                            continue;
                        }
                        else if (type == typeof(string))
                        {
                            sb.Append($"'{dataReader.GetString(i).Replace("'", "''")}',"); // escape single quotes
                            continue;
                        }
                        else if (type == typeof(int))
                        {
                            sb.Append($"{dataReader.GetInt32(i)},");
                            continue;
                        }
                        else if (type == typeof(short))
                        {
                            sb.Append($"{dataReader.GetInt16(i)},");
                            continue;
                        }

                        else if (type == typeof(decimal))
                        {
                            sb.Append($"{dataReader.GetDecimal(i)},");
                            continue;
                        }
                        else if (type == typeof(double))
                        {
                            sb.Append($"{dataReader.GetDouble(i)},");
                            continue;
                        }
                        else if (type == typeof(float))
                        {
                            sb.Append($"{dataReader.GetInt32(i)},");
                            continue;
                        }
                        else if (type == typeof(DateTime))
                        {
                            sb.Append($"'{dataReader.GetDateTime(i)}',");
                            continue;
                        }
                        else if (type == typeof(bool))
                        {
                            sb.Append($"{dataReader.GetBoolean(i)},");
                            continue;
                        }
                        else if (type == typeof(Guid))
                        {
                            sb.Append($"{dataReader.GetGuid(i)},");
                            continue;
                        }
                        else
                        {
                            // Logger?.Log("break");
                        }
                    }
                    streamWriter.Write(" ) VALUES (");
                    if (sb.Length > 0)
                        sb.Remove(sb.Length - 1, 1); // Remove the last comma
                    sb.Append(")");
                    streamWriter.WriteLine(sb.ToString());

                }
            }

            dataReader.Close();
            dataReader.Dispose();


            return true;


        }

        /// <inheritdoc />
        /// <summary>
        /// Scripts the table to c sharp class.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <returns>System.String.</returns>
        public string ScriptTableToCSharpClass(string tableName)
        {
            if (!TableExist(tableName))
            {

            }
            var list = ExecuteStoredProcedure("sp_columns", new List<DbParameter>() { GetNewParameter("@table_name", tableName) }).MapToList<TableDefinition>(JsonSerializer, XmlSerializer, CsvSerializer);
            if (!list.Any())
            {

                return null;
            }
            var sb = new StringBuilder();
            try
            {
                tableName = string.IsNullOrEmpty(tableName) ? "DynamicClass" : tableName;
                sb.AppendLine($"public class {tableName} {{ ");
                foreach (var column in list)
                {
                    var tempType = DBTypeToDotNetType(column.TYPE_NAME);
                    if (column.NULLABLE == 1)
                    {
                        sb.AppendLine($"[{nameof(SqlColumnAttritube)}(SetNullable = true)]");
                    }
                    if (column.NULLABLE == 0)
                    {
                        sb.AppendLine($"[{nameof(SqlColumnAttritube)}(SetNullable = false)]");
                    }
                    sb.AppendLine(column.NULLABLE == 1 && tempType != typeof(string) ? $"public {tempType.Name}? {column.COLUMN_NAME} {{ get; set; }} = null; " : $"public  {tempType.Name} {column.COLUMN_NAME} {{ get; set; }} ");
                }

                sb.AppendLine($"}}");
            }
            catch (Exception e)
            {
                throw ErrorHandling(e);
                //  Logger?.LogError(e);
            }

            return sb.ToString();

        }


        /// <inheritdoc />
        /// <summary>
        /// Scripts the query to c sharp class.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="className">Name of the class.</param>
        /// <returns>System.String.</returns>
        public string ScriptQueryToCSharpClass(string query, string className = null)
        {
            if (string.IsNullOrEmpty(query))
            {
                // Logger?.Log("Couldn't Convert Query To C# Class Because The Query Was Invalid");
                return null;
            }
            var reader = ExecuteManualQuery(query);
            return DevelopersTools.ToCSharpClass(reader, className, false);
        }


        /// <inheritdoc />
        /// <summary>
        /// Scripts the c sharp class to TSQL.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tableName">Name of the table.</param>
        /// <returns>System.String.</returns>
        public string ScriptCSharpClassToTsql<T>(string tableName = null) where T : class
        {
            var sqlBuilder = new StringBuilder();
            if (tableName == null) tableName = typeof(T).Name;
            sqlBuilder.Append($"Create Table {GetDefaultTableName<T>(tableName, true, true, true)} ( ");

            var list = ObjectSqlHelper.GetAllNonIgnoreFields<T>();
            list.ForEach(delegate (AdvanceMember p)
            {
                var value = DevelopersTools.AdvanceTypeToSqlType(p, DBTYPE);
                sqlBuilder.Append($" {p.Member.Name} {value} ,");
            });
            sqlBuilder.Remove(sqlBuilder.Length - 1, 1); // Remove the last comma DOESN'T APPEAR TO BE NESSACARY
            sqlBuilder.Append($" )");

            return sqlBuilder.ToString();
        }



        /// <summary>
        /// Databases the type of the type to dot net.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>Type.</returns>
        public Type DBTypeToDotNetType(string type)
        {

            if (type == "smalldatetime" || type == "datetime")
            {
                return typeof(DateTime);
            }
            if (type == "varbinary" || type == "binary" || type == "rowversion")
            {
                return typeof(byte[]);
            }
            if (type == "varbinary(1)" || type == "binary(1)" || type == "tinyint")
            {
                return typeof(byte);
            }
            if (type == "nvarchar(1)" || type == "nchar(1)")
            {
                return typeof(char);
            }
            if (type == "nvarchar" || type == "nchar" || type == "varchar" || type == "ntext" || type == "text" || type == "char")
            {
                return typeof(string);
            }
            if (type == "smallmoney" || type == "money" || type == "numeric" || type == "decimal")
            {
                return typeof(decimal);
            }
            if (type == "uniqueidentifier")
            {
                return typeof(Guid);
            }
            if (type == "bit")
            {
                return typeof(bool);
            }
            if (type == "smallint")
            {
                return typeof(short);
            }
            if (type == "int" || type == "int identity")
            {
                return typeof(int);
            }
            if (type == "bigint")
            {
                return typeof(long);
            }
            if (type == "float")
            {
                return typeof(double);
            }

            //  Logger?.Log($".Net Doesn't Support The Database Type {type} So I Couldn't Convert It");
            return null;

        }



        /// <inheritdoc />
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            QueryBucketManager.Dispose();
            Properties?.DataBases?.Clear();
            Properties?.Schemas?.Clear();
            Properties?.Schemas?.Clear();
        }


    }

}