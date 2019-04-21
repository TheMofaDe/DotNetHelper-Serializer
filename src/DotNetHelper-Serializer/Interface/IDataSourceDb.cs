using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq.Expressions;
using DotNetHelper_Contracts.Enum.DataSource;
using DotNetHelper_Contracts.Enum.IO;
using DotNetHelper_Serializer.Model;


#pragma warning disable IDE1006 // Naming Styles


namespace DotNetHelper_Serializer.Interface
{



    public interface IDataSourceDb : IDisposable
    {


        /// <summary>
        /// Set True If Being Use As Data Access For Public Apis. This Will Hide Thrown Execptions From The Public and return a friendly Message
        /// </summary>
        /// <value><c>true</c> if [throw custom exceptions]; otherwise, <c>false</c>.</value>
        bool ThrowCustomExceptions { get; set; }

        /// <summary>
        /// Gets or sets the timeout for sql command not the connection itself.
        /// </summary>
        /// <value>The timeout.</value>
        TimeSpan Timeout { get; set; }

        /// <summary>
        /// Database To Connect TO
        /// </summary>
        /// <value>The database.</value>
        string Database { get; set; }

        /// <summary>
        /// Connection String That Will Used To Connect To Server
        /// </summary>
        /// <value>The connection string.</value>
        string ConnectionString { get; set; }

        /// <summary>
        /// Server Name Or IP thats hosting An Sql Instance
        /// </summary>
        /// <value>The server.</value>
        string Server { get; set; }

        /// <summary>
        /// Use Integrated Security AKA Windows Authentication
        /// </summary>
        /// <value><c>true</c> if [integrated security]; otherwise, <c>false</c>.</value>
        bool IntegratedSecurity { get; set; }

        /// <summary>
        /// if T1 is true Bulk Insert Will Always be used when the object counts is greater than or equal to  T2
        /// </summary>
        (bool UseBulkInsert, int minimumRecordRequiredToUse) AlwaysUseBulkInsert { get; set; }

        /// <summary>
        /// Username that will be used if connection string is null or empty
        /// </summary>
        /// <value>The name of the user.</value>
        string UserName { get; set; }

        /// <summary>
        /// Password that will be used if connection string is null or empty
        /// </summary>
        /// <value>The password.</value>
        string Password { get; set; }

        /// <summary>
        /// Set true if Instance is SQLExpress
        /// </summary>
        /// <value><c>true</c> if this instance is SQL express; otherwise, <c>false</c>.</value>
        bool IsSqlExpress { get; set; }

        /// <summary>
        /// TableSchema To Append to table name
        /// </summary>
        /// <value>The table schema.</value>
        string TableSchema { get; set; }

        /// <summary>
        ///  Only Used By OLEDB Connections
        /// </summary>
        bool PersistSecurityInfo { get; set; }

        /// <summary>
        ///  Only Used By OLEDB Connections
        /// </summary>
        string JetOledbSystemDatabase { get; set; }

        /// <summary> 
        /// Only Used BY OLEDB Connections 
        /// </summary>
        string Provider { get; set; }

        /// <summary>
        /// Create Table If It Doesn't Exist When Excuting Queries Only Works For Dynamic Execute Methods
        /// </summary>
        /// <value><c>true</c> if [create tables if not exist]; otherwise, <c>false</c>.</value>
        bool CreateTablesIfNotExist { get; set; }

        /// <summary>
        /// Create Schema If It Doesn't Exist When Excuting Queries
        /// </summary>
        /// <value><c>true</c> if [create schema if not exist]; otherwise, <c>false</c>.</value>
        bool CreateSchemaIfNotExist { get; set; }

        /// <summary>
        /// Rollback Any Batch Data If One Record Fails
        /// </summary>
        /// <value><c>true</c> if [enable transaction rollback]; otherwise, <c>false</c>.</value>
        bool EnableTransactionRollback { get; set; }

        /// <summary>
        /// Gets or sets the port.
        /// </summary>
        /// <value>The port.</value>
        int? Port { get; set; }

        /// <summary>
        /// Gets or sets the QueryBucketManger.
        /// </summary>
        /// <value>The settings.</value>
        QueryBucketManager QueryBucketManager { get; set; }

        /// <summary>
        /// Full File Name To The .MDF File
        /// </summary>
        /// <value>The full name of the file.</value>
        string FullFileName { get; set; }

        /// <summary>
        /// Gets or sets the dbtype.
        /// </summary>
        /// <value>The dbtype.</value>
        DataBaseType DBTYPE { get; }

        /// <summary>
        /// Gets or sets the properties.
        /// </summary>
        /// <value>The properties.</value>
        DataSourceProperties Properties { get; set; }

        /// <summary>
        /// Gets or sets the Json Serializer for columns that stores its value as json
        /// </summary>
        IJsonSerializer JsonSerializer { get; set; }

        /// <summary>
        /// Gets or sets the Xml Serializer for columns that stores its value as xml
        /// </summary>
        IXmlSerializer XmlSerializer { get; set; }

        /// <summary>
        /// Gets or sets the Csv Serializer for columns that stores its value as csv
        /// </summary>
        ICsvSerializer CsvSerializer { get; set; }


        DateTime? LastConnectionOpenTime { get; set; }
        event EventHandler OnSqlException;

        /// <summary>
        /// Build a SqlConnection String Based On DataSource Properties Will AutoBuild A Connection String If An Connection String Is Not Already Defined
        /// </summary>
        /// <returns>connection string</returns>
        string BuildConnectionString();

        DbParameter GetNewParameter(string parmeterName, object value);
        IDbConnection GetNewConnection(bool openConnection, bool throwOnFailOpenConnection);
        IDbCommand GetNewCommand(string cmdText = null, IDbConnection connection = null, IDbTransaction dbTransaction = null);

        /// <inheritdoc />
        /// <summary>
        /// Returns true if the refresh was succesful
        /// </summary>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        bool RefreshDataSourceProperties();

        /// <summary>
        /// Determines whether this instance can connect.
        /// </summary>
        /// <returns><c>true</c> if this instance can connect; otherwise, <c>false</c>.</returns>
        bool CanConnect();

        bool RecordExist<T>(T obj, string tableName, Expression<Func<T, object>> overrideKey = null) where T : class;



        /// <summary>
        /// Gets the specified table name.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="whereClause">The where clause.</param>
        /// <returns>IDataReader.</returns>
        IDataReader Get(string tableName, string whereClause = null);

        /// <summary>
        /// Get the Top 1 record where the primary key(s) matches if matching record exist then return default value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">only use for key mapping against table</param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        T GetTopOne<T>(T obj, string tableName = null) where T : class;

        /// <summary>
        /// Gets the specified table name.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="whereClause">The where clause.</param>
        /// <returns>List&lt;T&gt;.</returns>
        List<T> Get<T>(string tableName = null, string whereClause = null) where T : class;

        /// <summary>
        /// T1 is sql & T2 is split on 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataBaseType"></param>
        /// <returns></returns>
        (string sql, string splitOn) BuildJoinString<T>() where T : class;

        /// <summary>
        /// Gets the specified table name data with dynamically building your where clause from your paramter object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="dynamicParameters"></param>
        /// <returns>List&lt;T&gt;.</returns>
        List<T> GetWithParameters<T>(string tableName, ExpandoObject dynamicParameters, string whereClause = null) where T : class;

        /// <summary>
        /// Gets the specified T data filtering using the Linq Expression.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="whereExpression"></param>
        /// <param name="tableName">Name of the table.</param>
        /// <returns>List&lt;T&gt;.</returns>
        List<T> GetLinq<T>(Expression<Func<T, bool>> whereExpression, string tableName = null) where T : class;

        /// <summary>
        /// Executes the stored procedure.
        /// </summary>
        /// <param name="procedureName">Name of the procedure.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>IDataReader.</returns>
        IDataReader ExecuteStoredProcedure(string procedureName, List<DbParameter> parameters);

        /// <summary>
        /// Connection String Must Be Valid To Create
        /// </summary>
        /// <param name="fullFilePath">The full file path.</param>
        /// <param name="databaseName">Name of the database.</param>
        /// <param name="overwrite">if set to <c>true</c> [overwrite].</param>
        /// <returns>System.Int32.</returns>
        int CreateLocalDatabaseFile(string fullFilePath, string databaseName, bool overwrite = false);

        /// <summary>
        /// Retrieve A DataReader Of All Records From The Specified Query Please Make Sure You Closed The DataReader When No longer need
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>IDataReader.</returns>
        /// <exception cref="NotImplementedException">Sorry Haven't implemented this yet</exception>
        IDataReader ExecuteManualQuery(string query);

        /// <summary>
        /// Executes the manual query.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>IDataReader.</returns>
        IDataReader ExecuteManualQuery(string query, List<DbParameter> parameters);

        /// <summary>
        /// Bulks the insert.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listPoco">The list poco.</param>
        /// <param name="tableName">Name of the table.</param>
        /// <returns>System.Int32.</returns>
        int BulkInsert<T>(IEnumerable<T> listPoco, string tableName) where T : class;

        /// <summary>
        /// Executes the manual SQL.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query">The query.</param>
        /// <returns>List&lt;T&gt;.</returns>
        List<T> ExecuteManualSql<T>(string query) where T : class;

        /// <summary>
        /// Executes the manual non query SQL.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>System.Int32.</returns>
        int ExecuteManualNonQuerySql(string query);

        /// <summary>
        /// Creates the table from class.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="dropIfExist">if set to <c>true</c> [drop if exist].</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        bool CreateTableFromClass<T>(string tableName, bool dropIfExist = false) where T : class;

        /// <summary>
        /// Creates the table from class.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="dropIfExist">if set to <c>true</c> [drop if exist].</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        bool CreateTableFromClass(Type type, string tableName, bool dropIfExist = false);

        /// <summary>
        /// Creates the table from a dynamic object .
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="dropIfExist">if set to <c>true</c> [drop if exist].</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        bool CreateTableFromDynamicObject<T>(T dynamicObject, string tableName, bool dropIfExist = false) where T : class;

        /// <summary>
        /// Drops the table.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <returns><c>true</c> if a table was drop then return true, <c>false</c> otherwise.</returns>
        bool DropTable(string tableName);

        /// <summary>
        /// Checks If Schema Exist
        /// </summary>
        /// <param name="schema">The schema.</param>
        /// <param name="createIfFalse">if set to <c>true</c> [create if false].</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        bool SchemaExist(string schema, bool createIfFalse = false);

        /// <summary>
        /// Check If Table Exist Must Include Schema
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        bool TableExist(string tableName);

        /// <summary>
        /// Errors the handling.
        /// </summary>
        /// <param name="error">The error.</param>
        /// <returns>Exception.</returns>
        Exception ErrorHandling(Exception error);

        /// <summary>
        /// Errors the handling.
        /// </summary>
        /// <param name="error">The error.</param>
        /// <returns>Exception.</returns>
        Exception ErrorHandling(Exception error, string sql);

        /// <summary>
        /// Logs the connection time.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="query">The query.</param>
        /// <returns>QueryBucket.</returns>
        QueryBucket LogConnectionTime(IDbConnection connection, string query);

        /// <summary>
        /// Executes the dynamic query.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type">Type Of Query To Execute</param>
        /// <param name="poco">The Object To Execute Against</param>
        /// <param name="tableName">Name of the table.</param>
        /// <returns>Returns Record Affected</returns>
        /// <exception cref="ArgumentOutOfRangeException">type - null</exception>
        int ExecuteDynamicQuery<T>(ActionType type, IEnumerable<T> poco, string tableName = null, Expression<Func<T, object>> overrideKeys = null) where T : class;

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
        int ExecuteDynamicQuery<T>(ActionType type, T poco, string tableName = null, Expression<Func<T, object>> overrideKeys = null) where T : class;

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
        IDataReader ExecuteDynamicQueryWithOutputs<T>(ActionType type, T poco, Expression<Func<T, object>> expression, string tableName = null) where T : class;

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
        T ExecuteDynamicQueryReturnIdentity<T>(ActionType type, T poco, string tableName = null) where T : class;

        void BulkInsertDataTable(DataTable table, SqlBulkCopyOptions options, string destinationTableName = null, Action<SqlRowsCopiedEventArgs> callback = null);

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
        List<string> CreateScripts<T>(ActionType scriptsType, List<T> objects, string tableName = null, ScriptType type = ScriptType.HumanReadable, Expression<Func<T, object>> overrideKeys = null) where T : class;

        /// <summary>
        /// Dumps the table data to file.
        /// </summary>
        /// <param name="fullFilePath">The full file path.</param>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="option">The option.</param>
        /// <param name="whereClause">The where clause.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        bool DumpTableDataToFile(string fullFilePath, string tableName, FileOption option, string whereClause = null);

        /// <inheritdoc />
        /// <summary>
        /// Scripts the table to c sharp class.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <returns>System.String.</returns>
        string ScriptTableToCSharpClass(string tableName);

        /// <inheritdoc />
        /// <summary>
        /// Scripts the query to c sharp class.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="className">Name of the class.</param>
        /// <returns>System.String.</returns>
        string ScriptQueryToCSharpClass(string query, string className = null);

        /// <inheritdoc />
        /// <summary>
        /// Scripts the c sharp class to TSQL.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tableName">Name of the table.</param>
        /// <returns>System.String.</returns>
        string ScriptCSharpClassToTsql<T>(string tableName = null) where T : class;

        /// <summary>
        /// Databases the type of the type to dot net.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>Type.</returns>
        Type DBTypeToDotNetType(string type);




    }
}