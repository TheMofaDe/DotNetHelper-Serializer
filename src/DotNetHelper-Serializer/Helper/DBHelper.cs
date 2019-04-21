using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
#if NETFRAMEWORK
using System.Data.Odbc;
using System.Data.OleDb;
#endif
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using DotNetHelper_Contracts.Enum.DataSource;
using DotNetHelper_Contracts.Extension;
using DotNetHelper_Serializer.Extension;
using DotNetHelper_Serializer.Interface;
using DotNetHelper_Serializer.Model;
using FastMember;

namespace DotNetHelper_Serializer.Helper
{
    public static class DBHelper
    {

        public static Dictionary<Type, DbType> TypeToSqlTypeMap = new Dictionary<Type, DbType>
        {
            [typeof(byte)] = DbType.Byte,
            [typeof(sbyte)] = DbType.SByte,
            [typeof(short)] = DbType.Int16,
            [typeof(ushort)] = DbType.UInt16,
            [typeof(int)] = DbType.Int32,
            [typeof(uint)] = DbType.UInt32,
            [typeof(long)] = DbType.Int64,
            [typeof(ulong)] = DbType.UInt64,
            [typeof(float)] = DbType.Single,
            [typeof(double)] = DbType.Double,
            [typeof(decimal)] = DbType.Decimal,
            [typeof(bool)] = DbType.Boolean,
            [typeof(string)] = DbType.String,
            [typeof(char)] = DbType.StringFixedLength,
            [typeof(Guid)] = DbType.Guid,
            [typeof(DateTime)] = DbType.DateTime,
            [typeof(DateTimeOffset)] = DbType.DateTimeOffset,
            [typeof(TimeSpan)] = DbType.Time,
            [typeof(byte[])] = DbType.Binary,
            [typeof(byte?)] = DbType.Byte,
            [typeof(sbyte?)] = DbType.SByte,
            [typeof(short?)] = DbType.Int16,
            [typeof(ushort?)] = DbType.UInt16,
            [typeof(int?)] = DbType.Int32,
            [typeof(uint?)] = DbType.UInt32,
            [typeof(long?)] = DbType.Int64,
            [typeof(ulong?)] = DbType.UInt64,
            [typeof(float?)] = DbType.Single,
            [typeof(double?)] = DbType.Double,
            [typeof(decimal?)] = DbType.Decimal,
            [typeof(bool?)] = DbType.Boolean,
            [typeof(char?)] = DbType.StringFixedLength,
            [typeof(Guid?)] = DbType.Guid,
            [typeof(DateTime?)] = DbType.DateTime,
            [typeof(DateTimeOffset?)] = DbType.DateTimeOffset,
            [typeof(TimeSpan?)] = DbType.Time,
            [typeof(object)] = DbType.Object
        };





        public static string GetQuotedValue(object value, DataBaseType type)
        {

            var syntax = new SqlSyntaxHelper(type);
            return syntax.GetEnclosedValueChar(value.GetType());
        }






        #region IDBConnection & IDBCommand
        public static IDbConnection GetDbConnection(IDataSourceDb databse)
        {
            IDbConnection connection = null;
            switch (databse.DBTYPE)
            {
                case DataBaseType.SqlServer:
                    connection = new SqlConnection();
                    break;
                case DataBaseType.MySql:
                case DataBaseType.Sqlite:
                    connection = databse.GetNewConnection(false, true);
                    break;
                case DataBaseType.Oracle:
                    break;
                case DataBaseType.Oledb:
#if NETFRAMEWORK
                    connection = new OleDbConnection();
                    break;
#else
                    throw new NotImplementedException($@".Net Standard haven't implemented this functionality yet.");
#endif

                case DataBaseType.Access95:
#if NETFRAMEWORK
                    connection = new OleDbConnection();
                    break;
#else
                    throw new NotImplementedException($@".Net Standard haven't implemented this functionality yet.");
#endif

                case DataBaseType.Odbc:
#if NETFRAMEWORK
                    connection = new OdbcConnection();
                    break;
#else
                    throw new NotImplementedException($@".Net Standard haven't implemented this functionality yet.");
#endif

                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (connection != null)
            {
                connection.ConnectionString = databse.BuildConnectionString();
            }
            return connection;
        }

        public static DbParameter GetNewParameter(IDataSourceDb database, string name, object value)
        {
            if (name.StartsWith("@")) name = name.Remove(0, 1);
            switch (database.DBTYPE)
            {
                case DataBaseType.SqlServer:
                    var param = new SqlParameter(name, value);
                    //if (value.GetType() == typeof(byte[]))
                    //{
                    //    param.SqlDbType = SqlDbType.VarBinary;
                    //}
                    return param;
                case DataBaseType.MySql:
                case DataBaseType.Sqlite:
                    return database.GetNewParameter(name, value);
                case DataBaseType.Oracle:
                // return new SqlParameter(name, value);
                case DataBaseType.Oledb:
                // return new SqlParameter(name, value);
                case DataBaseType.Access95:
#if NETFRAMEWORK
                    return new OleDbParameter(name, value);
#else
                    throw new NotImplementedException($@".Net Standard haven't implemented this functionality yet.");
#endif
                // return new SqlParameter(name, value);
                case DataBaseType.Odbc:
#if NETFRAMEWORK
                    return new OdbcParameter(name, value);
#else
                    throw new NotImplementedException($@".Net Standard haven't implemented this functionality yet.");
#endif
                default:
                    throw new ArgumentOutOfRangeException();
            }

        }

        public static IDbCommand GetDbCommand(IDataSourceDb database, string commandText = null, IDbConnection connection = null, IDbTransaction dbTransaction = null)
        {
            IDbCommand command = null;
            switch (database.DBTYPE)
            {
                case DataBaseType.SqlServer:
                    command = new SqlCommand();
                    break;
                case DataBaseType.MySql:
                case DataBaseType.Sqlite:
                case DataBaseType.Oracle:
                    command = database.GetNewCommand(commandText, connection, dbTransaction);
                    break;

                    throw new ArgumentOutOfRangeException(nameof(database.DBTYPE), database.DBTYPE, null);
                case DataBaseType.Oledb:
#if NETFRAMEWORK
                    command = new OleDbCommand();
                    break;
#else
                    throw new NotImplementedException($@".Net Standard haven't implemented this functionality yet.");
#endif

                case DataBaseType.Access95:
#if NETFRAMEWORK
                    command = new OleDbCommand();
                    break;
#else
                    throw new NotImplementedException($@".Net Standard haven't implemented this functionality yet.");
#endif

                case DataBaseType.Odbc:
#if NETFRAMEWORK
                    command = new OdbcCommand();
                    break;
#else
                    throw new NotImplementedException($@".Net Standard haven't implemented this functionality yet.");
#endif
                default:
                    throw new ArgumentOutOfRangeException(nameof(database.DBTYPE), database.DBTYPE, null);
            }
            command.CommandText = commandText;
            command.CommandTimeout = (int)database.Timeout.TotalSeconds;
            if (connection != null)
            {
                command.Connection = connection;
            }
            if (dbTransaction != null)
            {
                command.Transaction = dbTransaction;
            }
            return command;
        }



        // https://technet.microsoft.com/en-us/library/ms189122(v=sql.105).aspx
        public static IDbTransaction GetDbTransaction(IDbConnection connection, IsolationLevel level = IsolationLevel.ReadCommitted)
        {
            return connection?.BeginTransaction(level);
        }

        #endregion


        #region  BuildConnection String


        /// <summary>
        /// Build a SqlConnection String Based On DataSource Properties Will AutoBuild A Connection String If An Connection String Is Not Already Defined
        /// </summary>
        /// <returns>connection string</returns>
        public static string BuildConnectionString(IDataSourceDb datasource)
        {
            switch (datasource.DBTYPE)
            {
                case DataBaseType.SqlServer:
                    return GetSqlServerConnectionString(datasource);
                case DataBaseType.MySql:
                    return GetMySqlConnectionString(datasource);

                case DataBaseType.Sqlite:
                    throw new NotImplementedException("You must reference the nuget package dotnethelper-sqlite to use this ");
                //   return GetSqliteConnectionString(datasource);;
                case DataBaseType.Oracle:
                    throw new NotImplementedException("You must reference the nuget package dotnethelper-OracleDB to use this ");

                case DataBaseType.Oledb:

                    break;
                case DataBaseType.Access95:
                    return GetAccess95ConnectionString(datasource);
                case DataBaseType.Odbc:
                    return GetOdbcConnectionString(datasource);
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return datasource.ConnectionString;
        }

        /// <summary>
        /// Build a OleDbConnection String Based On DataSource Properties, If An Connection String Is Already Set Then it will that
        /// </summary>
        /// <returns>connection string</returns>
        private static string GetAccess95ConnectionString(IDataSourceDb datasource)
        {

            var PersistSecurityInfo = datasource.PersistSecurityInfo;
            var Provider = datasource.Provider;
            var DataSource = datasource.FullFileName;
            var JetOledbSystemDatabase = datasource.JetOledbSystemDatabase;
            var UserName = datasource.UserName;
            var Password = datasource.Password;
            var sb = new StringBuilder();
            if (!string.IsNullOrEmpty(datasource.ConnectionString)) return datasource.ConnectionString;
            var temp = PersistSecurityInfo ? "True" : "False";
            if (!string.IsNullOrEmpty(Provider))
                sb.Append(Provider.EndsWith(";") ? "Provider=" + Provider : "Provider=" + Provider + ";");
            if (!string.IsNullOrEmpty(DataSource))
                sb.Append(DataSource.EndsWith(";") ? "Data Source=" + DataSource : "Data Source=" + DataSource + ";");
            //      if (PersistSecurityInfo)
            sb.Append($"Persist Security Info={temp};");
            if (!string.IsNullOrEmpty(JetOledbSystemDatabase))
                sb.Append(JetOledbSystemDatabase.EndsWith(";") ? "Jet OLEDB:System database=" + JetOledbSystemDatabase : "Jet OLEDB:System database=" + JetOledbSystemDatabase + ";");
            if (!string.IsNullOrEmpty(UserName))
                sb.Append(UserName.EndsWith(";") ? "User ID=" + UserName : "User ID=" + UserName + ";");
            if (!string.IsNullOrEmpty(Password))
                sb.Append(Password.EndsWith(";") ? "Password=" + Password : "Password=" + Password + ";");
            return sb.ToString();

        }



        private static string GetSqlServerConnectionString(IDataSourceDb datasource)
        {
            var FullFileName = datasource.FullFileName;
            var IntegratedSecurity = datasource.IntegratedSecurity;
            var Timeout = datasource.Timeout;
            var IsSqlExpress = datasource.IsSqlExpress;
            var ConnectionString = datasource.ConnectionString;
            var UserName = datasource.UserName;
            var Password = datasource.Password;
            var Server = datasource.Server;
            var Database = datasource.Database;
            var sb = new StringBuilder();

            if (!string.IsNullOrEmpty(datasource.FullFileName))
            {
                sb.Append($@"Data Source=.\SQLEXPRESS;
                          AttachDbFilename={FullFileName};
                          Integrated Security={IntegratedSecurity};
                          Connect Timeout={Timeout.TotalSeconds};
                          User Instance=True");
                return sb.ToString();
            }


            if (!IsSqlExpress)
            {
                if (!string.IsNullOrEmpty(ConnectionString)) return ConnectionString;
                sb.Append(Server.EndsWith(";") ? "Server=" + Server : "Server=" + Server + ";");
                if (!string.IsNullOrEmpty(Database))
                    sb.Append(Database.EndsWith(";") ? "Database=" + Database : "Database=" + Database + ";");
                if (IntegratedSecurity)
                    sb.Append(" Integrated Security=SSPI;");
                if (!string.IsNullOrEmpty(UserName))
                    sb.Append(UserName.EndsWith(";") ? "User ID=" + UserName : "User ID=" + UserName + ";");
                if (!string.IsNullOrEmpty(Password))
                    sb.Append(Password.EndsWith(";") ? "Password=" + Password : "Password=" + Password + ";");
                return sb.ToString();
            }
            else
            {
                if (!string.IsNullOrEmpty(ConnectionString)) return ConnectionString;
                sb.Append(Server.EndsWith(";") ? "Data Source=" + Server.Remove(Server.Length - 1, 1) : "Data Source=" + Server + "");
                sb.Append($"\\SQLExpress;");

                if (!string.IsNullOrEmpty(Database))
                    sb.Append(Database.EndsWith(";") ? "Initial Catalog=" + Database : "Initial Catalog=" + Database + ";");
                if (IntegratedSecurity)
                    sb.Append("Integrated Security=True;");
                return sb.ToString();

            }

        }


        private static string GetMySqlConnectionString(IDataSourceDb datasource)
        {
            var FullFileName = datasource.FullFileName;
            var IntegratedSecurity = datasource.IntegratedSecurity;
            var Timeout = datasource.Timeout;
            var IsSqlExpress = datasource.IsSqlExpress;
            var ConnectionString = datasource.ConnectionString;
            var UserName = datasource.UserName;
            var Password = datasource.Password;
            var Server = datasource.Server;
            var Database = datasource.Database;
            var sb = new StringBuilder();

            if (!string.IsNullOrEmpty(datasource.FullFileName))
            {
                sb.Append($@"Data Source=.\SQLEXPRESS;
                          AttachDbFilename={FullFileName};
                          Integrated Security={IntegratedSecurity};
                          Connect Timeout={Timeout.TotalSeconds};
                          User Instance=True");
                return sb.ToString();
            }


            if (!IsSqlExpress)
            {
                if (!string.IsNullOrEmpty(ConnectionString)) return ConnectionString;
                sb.Append(Server.EndsWith(";") ? "Server=" + Server : "Server=" + Server + ";");
                if (!string.IsNullOrEmpty(Database))
                    sb.Append(Database.EndsWith(";") ? "Database=" + Database : "Database=" + Database + ";");
                if (IntegratedSecurity)
                    sb.Append(" Integrated Security=SSPI;");
                if (!string.IsNullOrEmpty(UserName))
                    sb.Append(UserName.EndsWith(";") ? "User ID=" + UserName : "User ID=" + UserName + ";");
                if (!string.IsNullOrEmpty(Password))
                    sb.Append(Password.EndsWith(";") ? "Password=" + Password : "Password=" + Password + ";");
                return sb.ToString();
            }
            else
            {
                if (!string.IsNullOrEmpty(ConnectionString)) return ConnectionString;
                sb.Append(Server.EndsWith(";") ? "Data Source=" + Server.Remove(Server.Length - 1, 1) : "Data Source=" + Server + "");
                sb.Append($"\\SQLExpress;");

                if (!string.IsNullOrEmpty(Database))
                    sb.Append(Database.EndsWith(";") ? "database=" + Database : "database=" + Database + ";");
                if (IntegratedSecurity)
                    sb.Append("Integrated Security=True;");
                return sb.ToString();

            }

        }



        /// <summary>
        /// Build a OdbcConnection String Based On DataSource Properties, If An Connection String Is Already Set Then it will that
        /// </summary>
        /// <returns>connection string</returns>
        private static string GetOdbcConnectionString(IDataSourceDb datasource)
        {
            var IntegratedSecurity = datasource.IntegratedSecurity;
            var ConnectionString = datasource.ConnectionString;
            var UserName = datasource.UserName;
            var Password = datasource.Password;
            var Server = datasource.Server;
            var Database = datasource.Database;

            var sb = new StringBuilder();
            if (!string.IsNullOrEmpty(ConnectionString)) return ConnectionString;
            //   if (!string.IsNullOrEmpty(DSN) && string.IsNullOrEmpty(UserName) && string.IsNullOrEmpty(Password)) return $"DSN={DSN}";
            // 
            // 
            // 
            //   if (!string.IsNullOrEmpty(Driver))
            //       sb.Append(Driver.EndsWith(";") ? $"{nameof(Driver)}=" + Driver : $"{nameof(Driver)}=" + Driver + ";");
            //   if (!string.IsNullOrEmpty(Server))
            //       sb.Append(Server.EndsWith(";") ? $"servername=" + Server : $"servername=" + Server + ";");
            //   if (Port != null)
            //       sb.Append(Server.EndsWith(";") ? $"port=" + Port.Value : $"port=" + Port.Value + ";");
            //   if (!string.IsNullOrEmpty(Database))
            //       sb.Append(Database.EndsWith(";") ? "Database=" + Database : "Database=" + Database + ";");
            //   if (!string.IsNullOrEmpty(UserName))
            //       sb.Append(UserName.EndsWith(";") ? "UserName=" + UserName : "UserName=" + UserName + ";");
            //   if (!string.IsNullOrEmpty(Password))
            //       sb.Append(Password.EndsWith(";") ? "Password=" + Password : "Password=" + Password + ";");
            // 
            //   sb.Append(IntegratedSecurity ? "Integrated Security=True;" : "Integrated Security=False;");
            return sb.ToString();


        }



        #endregion

        #region Bulk Insert Logic

        public static int BulkInsert<T>(IDataSourceDb database, IEnumerable<T> listPoco, string tableName) where T : class
        {
            switch (database.DBTYPE)
            {
                case DataBaseType.SqlServer:
                    return BulkInsertSqlServer(database, listPoco, tableName);
                case DataBaseType.MySql:
                    break;
                case DataBaseType.Sqlite:
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
            return 0;
        }




        /// <summary>
        /// Bulks the insert.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listPoco">The list poco.</param>
        /// <param name="tableName">Name of the table.</param>
        /// <returns>System.Int32.</returns>
        private static int BulkInsertSqlServer<T>(IDataSourceDb database, IEnumerable<T> listPoco, string tableName) where T : class
        {

            using (var bcp = new SqlBulkCopy(database.BuildConnectionString()))
            {
                var openTime = DateTime.Now;
                var bucket = new QueryBucket()
                {
                    Query = $"Bulk Insert On List Of Type {typeof(T).Name} Into Table {tableName}",
                    Server = database.Server,
                    DatabaseType = database.DBTYPE.ToString()
                };
                bcp.SqlRowsCopied += delegate (object sender, SqlRowsCopiedEventArgs args)
                {
                    bucket.ConnectionDisposeTime = DateTime.Now;
                };
                database.QueryBucketManager.AddBucket(bucket);



                var array = new List<string>() { };
                var advances = ExtFastMember.GetAdvanceMembers(listPoco.FirstOrDefault()).Where(m => m.SqlCustomAttritube.AutoIncrementBy == null && m.SqlCustomAttritube.StartIncrementAt == null && m.SqlCustomAttritube.Ignore != true).ToList(); // EXECLUDES IDENTITY FEILDS
                advances.ForEach(delegate (AdvanceMember member)
                {
                    var columnName = member.SqlCustomAttritube.MapTo ?? member.Member.Name;
                    array.Add(columnName);
                });
                using (var reader = ObjectReader.Create(listPoco, array.ToArray()))
                {
                    bcp.DestinationTableName = tableName;
                    bcp.BulkCopyTimeout = database.Timeout.TotalSeconds.ToInt(false);
                    bcp.WriteToServer(reader);
                    return listPoco.Count();
                }
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
        public static int ExecuteNonQueryOnList<T>(IDataSourceDb database, string tableName, IEnumerable<T> listPoco, Action<StringBuilder, string, T, Expression<Func<T, object>>> buildSqlString, Expression<Func<T, object>> overrideKeys) where T : class
        {
            switch (database.DBTYPE)
            {
                case DataBaseType.SqlServer:
                    return ExecuteNonQueryOnListSQLServer(database, tableName, listPoco, buildSqlString, overrideKeys);
                case DataBaseType.MySql:
                    return ExecuteNonQueryOnListStandard<T>(database, tableName, listPoco, buildSqlString, overrideKeys);
                case DataBaseType.Sqlite:
                    return ExecuteNonQueryOnListStandard<T>(database, tableName, listPoco, buildSqlString, overrideKeys);
                case DataBaseType.Oracle:
                    return ExecuteNonQueryOnListStandard<T>(database, tableName, listPoco, buildSqlString, overrideKeys);
                case DataBaseType.Oledb:
                    return ExecuteNonQueryOnListStandard<T>(database, tableName, listPoco, buildSqlString, overrideKeys);
                case DataBaseType.Access95:
                    return ExecuteNonQueryOnListStandard<T>(database, tableName, listPoco, buildSqlString, overrideKeys);

                case DataBaseType.Odbc:
                    return ExecuteNonQueryOnListStandard<T>(database, tableName, listPoco, buildSqlString, overrideKeys);
                default:
                    throw new ArgumentOutOfRangeException();
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
        private static int ExecuteNonQueryOnListSQLServer<T>(IDataSourceDb database, string tableName, IEnumerable<T> listPoco, Action<StringBuilder, string, T, Expression<Func<T, object>>> buildSqlString, Expression<Func<T, object>> overrideKeys = null) where T : class
        {
            var records = 0;


            //     if (!EnableTransactionRollback && DBTYPE == DataBaseType.SqlServer) return BulkInsert(listPoco, tableName); TODO :: This triggers on any action type not insert only fix this

            using (var conn = database.GetNewConnection(false, true) as SqlConnection)
            {
                conn.Open();
                using (var trans = conn.BeginTransaction("StartingPoint"))
                {
                    var startTime = DateTime.Now;


                    foreach (var poco in listPoco)
                    {
                        try
                        {
                            var sqlBuilder = new StringBuilder();
                            buildSqlString(sqlBuilder, tableName, poco, overrideKeys);
                            var query = sqlBuilder.ToString();
                            using (var cmd = database.GetNewCommand(query, conn, trans))
                            {
                                var bucket = database.LogConnectionTime(conn, query);

                                var DbParameters = new ObjectToSqlHelper(database.DBTYPE).BuildDbParameterList(poco, database.GetNewParameter, database.XmlSerializer, database.JsonSerializer, database.CsvSerializer);
                                cmd.Parameters.AddRange(DbParameters.ToArray());
                                if (database.QueryBucketManager.IncludeReadableQuery)
                                    bucket.ReadableQuery = cmd.Parameters.ParamToSql(sqlBuilder.ToString());
                                records += cmd.ExecuteNonQuery();
                                bucket.ExecutedSuccesfully = true;
                            }

                        }
                        catch (Exception error)
                        {
                            if (database.EnableTransactionRollback)
                            {
                                trans.Rollback("StartingPoint");
                                var endTime = DateTime.Now;
                                var difference = endTime.Subtract(startTime);
                                return 0;  //records;
                            }
                            else
                            {
                                trans.Commit(); // commit whats already there anyways
                                throw database.ErrorHandling(error);
                            }

                        }
                    }
                    if (records >= 0)
                    {
                        trans.Commit();
                        var endTime = DateTime.Now;
                        var difference = endTime.Subtract(startTime);

                    }

                }
            }
            return records;

        }


        /// <summary>
        /// Executes the non query on list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="listPoco">The list poco.</param>
        /// <param name="buildSqlString">The build SQL string.</param>
        /// <returns>Numbers Of Rows Affected</returns>
        private static int ExecuteNonQueryOnListStandard<T>(IDataSourceDb database, string tableName, IEnumerable<T> listPoco, Action<StringBuilder, string, T, Expression<Func<T, object>>> buildSqlString, Expression<Func<T, object>> overrideKeys = null) where T : class
        {
            var records = 0;


            //     if (!EnableTransactionRollback && DBTYPE == DataBaseType.SqlServer) return BulkInsert(listPoco, tableName); TODO :: This triggers on any action type not insert only fix this

            using (var conn = database.GetNewConnection(false, true))
            {
                conn.Open();
                using (var trans = conn.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    var startTime = DateTime.Now;


                    foreach (var poco in listPoco)
                    {
                        try
                        {
                            var sqlBuilder = new StringBuilder();
                            buildSqlString(sqlBuilder, tableName, poco, overrideKeys);
                            var query = sqlBuilder.ToString();
                            using (var cmd = database.GetNewCommand(query, conn, trans))
                            {
                                var bucket = database.LogConnectionTime(conn, query);

                                var DbParameters = new ObjectToSqlHelper(database.DBTYPE).BuildDbParameterList(poco, database.GetNewParameter, database.XmlSerializer, database.JsonSerializer, database.CsvSerializer);
                                cmd.Parameters.AddRange(DbParameters.ToArray());
                                if (database.QueryBucketManager.IncludeReadableQuery)
                                    bucket.ReadableQuery = cmd.Parameters.ParamToSql(sqlBuilder.ToString());

                                records += cmd.ExecuteNonQuery();
                                bucket.ExecutedSuccesfully = true;
                            }

                        }
                        catch (Exception error)
                        {
                            if (database.EnableTransactionRollback)
                            {
                                trans.Rollback();
                                var endTime = DateTime.Now;
                                var difference = endTime.Subtract(startTime);

                                return records;
                            }
                            else
                            {
                                throw database.ErrorHandling(error);
                            }

                        }
                    }
                    if (records >= 0)
                    {
                        trans.Commit();
                        var endTime = DateTime.Now;
                        var difference = endTime.Subtract(startTime);

                    }

                }
            }
            return records;

        }





        #endregion


        public static QueryBucket LogConnectionTime(IDataSourceDb database, IDbConnection connection, string query)
        {
            var openTime = DateTime.Now;
            if (connection.State == ConnectionState.Open) database.LastConnectionOpenTime = openTime;
            var bucket = new QueryBucket() { ConnectionStartTime = openTime, Query = query, Server = database.Server, DatabaseType = DataBaseType.SqlServer.ToString() };
            switch (database.DBTYPE)
            {
                case DataBaseType.SqlServer:
                    if (connection is SqlConnection c)
                    {
                        c.Disposed += delegate (object sender, EventArgs args)
                        {
                            bucket.ConnectionDisposeTime = DateTime.Now;
                        };
                    }
                    break;
                case DataBaseType.MySql:
                case DataBaseType.Sqlite:

                    bucket.ConnectionDisposeTime = null;


                    break;
                case DataBaseType.Oracle:
                    break;
                case DataBaseType.Oledb:
                case DataBaseType.Access95:
#if NETFRAMEWORK
                    if (connection is OleDbConnection oledb)
                    {
                        oledb.Disposed += delegate (object sender, EventArgs args)
                        {
                            bucket.ConnectionDisposeTime = DateTime.Now;

                        };
                    }
                    break;
#else
                    throw new NotImplementedException($@".Net Standard haven't implemented this functionality yet.");
#endif

                case DataBaseType.Odbc:
#if NETFRAMEWORK
                    if (connection is OdbcConnection odbc)
                    {
                        odbc.Disposed += delegate (object sender, EventArgs args)
                        {
                            bucket.ConnectionDisposeTime = DateTime.Now;

                        };
                    }
                    break;
#else
                    break;

#endif

                default:
                    throw new ArgumentOutOfRangeException();
            }

            database.QueryBucketManager.AddBucket(bucket);
            return bucket;
        }










    }
}
