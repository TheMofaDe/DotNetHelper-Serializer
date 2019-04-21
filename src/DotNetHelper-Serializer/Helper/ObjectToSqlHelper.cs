using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using DotNetHelper_Contracts.CustomException;
using DotNetHelper_Contracts.Enum;
using DotNetHelper_Contracts.Enum.DataSource;
using DotNetHelper_Contracts.Extension;
using DotNetHelper_Contracts.Helpers;
using DotNetHelper_Contracts.Helpers.OneOffs;
using DotNetHelper_Serializer.Extension;
using DotNetHelper_Serializer.Interface;
using Newtonsoft.Json;

namespace DotNetHelper_Serializer.Helper
{
    public class ObjectToSqlHelper
    {
        //    #region Dynamic Query Building


        public DataBaseType DatabaseType { get; }

        public ObjectToSqlHelper(DataBaseType type)
        {
            DatabaseType = type;
        }


        /// <summary>
        /// Builds the where clause.
        /// </summary>
        /// <param name="sqlBuilder">The SQL builder.</param>
        /// <param name="keyFields">The key fields.</param>
        /// <exception cref="YouSuckAtBeingADeveloperException">Can't Bulid Where Clause Or Perform Upsert And Update Statements Without Having At Least One Property That Inherits The SqlCustomAttritube & Have The PRIMARY KEY SET TO TRUE OR The Foreign Key Set Up</exception>
        public void BuildWhereClause(StringBuilder sqlBuilder, List<AdvanceMember> keyFields, bool throwOnNoAttributes)
        {
            if (throwOnNoAttributes && !keyFields.Where(m => m.SqlCustomAttritube.PrimaryKey == true || !string.IsNullOrEmpty(m.SqlCustomAttritube.xRefTableName)).ToList().Any())
            {

                throw new BadCodeException("Can't Bulid Where Clause Or Perform Upsert And Update Statements Without Having At Least One Property That Inherits The SqlCustomAttritube & Have The PRIMARY KEY SET TO TRUE OR The Foreign Key Set Up");
            }
            else
            {
                sqlBuilder.Append("WHERE");
                keyFields.ForEach(p => sqlBuilder.Append($" [{p.GetActualMemberName()}]=@{p.Member.Name} AND"));
                if (sqlBuilder.ToString().EndsWith(" AND"))
                    sqlBuilder.Remove(sqlBuilder.Length - 4, 4); // Remove the last AND       
            }

        }

        //        Func<string, object, DbParameter> GetNewParameter
        /// <summary>
        /// Builds the where clause.
        /// </summary>
        /// <param name="sqlBuilder">The SQL builder.</param>
        /// <param name="keyFields">The key fields.</param>
        /// <exception cref="YouSuckAtBeingADeveloperException">Can't Bulid Where Clause Or Perform Upsert And Update Statements Without Having At Least One Property That Inherits The SqlCustomAttritube & Have The PRIMARY KEY SET TO TRUE OR The Foreign Key Set Up</exception>
        public List<DbParameter> BuildWhereClauseAndGeDbParameters(Func<string, object, DbParameter> GetNewParameter, StringBuilder sqlBuilder, List<AdvanceMember> keyFields, bool throwOnNoAttributes)
        {
            var list = new List<DbParameter>() { };
            if (throwOnNoAttributes && !keyFields.Where(m => m.SqlCustomAttritube.PrimaryKey == true || !string.IsNullOrEmpty(m.SqlCustomAttritube.xRefTableName)).ToList().Any())
            {
                throw new BadCodeException("Can't Bulid Where Clause Or Perform Upsert And Update Statements Without Having At Least One Property That Inherits The SqlCustomAttritube & Have The PRIMARY KEY SET TO TRUE OR The Foreign Key Set Up");
            }
            else
            {
                sqlBuilder.Append("WHERE");
                keyFields.ForEach(delegate (AdvanceMember p)
                {
                    sqlBuilder.Append($" [{p.GetActualMemberName()}]=@{p.Member.Name} AND");
                    list.Add(GetNewParameter(p.Member.Name, p.Value));
                });
                if (sqlBuilder.ToString().EndsWith(" AND"))
                    sqlBuilder.Remove(sqlBuilder.Length - 4, 4); // Remove the last AND       

            }
            return list;
        }



        public Tuple<List<DbParameter>, string> BuildWhereClause(Func<string, object, DbParameter> GetNewParameter, ExpandoObject parameters, string additionalWhere = null)
        {


            var whereClause = $" WHERE ";
            var startsWithWhere = (!string.IsNullOrEmpty(additionalWhere) && additionalWhere.Replace(" ", "").StartsWith("WHERE", StringComparison.OrdinalIgnoreCase));
            var endWithAnd = (!string.IsNullOrEmpty(additionalWhere) && additionalWhere.Replace(" ", "").EndsWith("AND", StringComparison.OrdinalIgnoreCase));
            var startsWithAnd = (!string.IsNullOrEmpty(additionalWhere) && additionalWhere.Replace(" ", "").StartsWith("AND", StringComparison.OrdinalIgnoreCase));
            var list = new List<DbParameter>() { };

            var dictionary = DynamicObjectHelper.GetProperties(parameters);
            if (dictionary != null && dictionary.Any())
            {

                dictionary.ForEach(delegate (KeyValuePair<string, object> pair)
                {


                    if (pair.Value.GetType().IsTypeIEnumerable() && pair.Value.GetType() != typeof(string))
                    {


                        whereClause += $@"{pair.Key} IN (";

                        //if (pair.Value is IEnumerable<DateTime> dates)
                        //{
                        //      if (dates.IsNullOrEmpty())
                        //      {
                        //          whereClause = whereClause.ReplaceLastOccurrance($@" {pair.Key} IN (", string.Empty, StringComparison.OrdinalIgnoreCase);
                        //      }
                        //      else
                        //      {
                        //          for (var i = 0; i < (dates ?? throw new InvalidOperationException()).Count(); i++)
                        //          {
                        //              whereClause += $"@{pair.Key + i}, ";
                        //              list.Add(GetNewParameter(pair.Key + i, dates.ToList()[i]));

                        //          }
                        //          whereClause = whereClause.ReplaceLastOccurrance(",", ") AND ", StringComparison.OrdinalIgnoreCase);
                        //      }
                        //}

                        var tempList = pair.Value as IEnumerable<object>;
                        if (tempList.IsNullOrEmpty())
                        {
                            whereClause = whereClause.ReplaceLastOccurrance($@" {pair.Key} IN (", string.Empty, StringComparison.OrdinalIgnoreCase);
                        }
                        else
                        {
                            for (var i = 0; i < (tempList ?? throw new InvalidOperationException()).Count(); i++)
                            {
                                whereClause += $"@{pair.Key + i}, ";
                                list.Add(GetNewParameter(pair.Key + i, tempList.ToList()[i]));

                            }
                            whereClause = whereClause.ReplaceLastOccurrance(",", ") AND ", StringComparison.OrdinalIgnoreCase);
                        }



                    }
                    else
                    {
                        whereClause += $@"{pair.Key} = @{pair.Key} AND ";
                        list.Add(GetNewParameter(pair.Key, pair.Value));
                    }



                });
                whereClause = whereClause.ReplaceLastOccurrance("AND", "", StringComparison.OrdinalIgnoreCase);

                if (startsWithWhere)
                {
                    additionalWhere = additionalWhere.ReplaceFirstOccurrance("WHERE", " ", StringComparison.OrdinalIgnoreCase);
                }
                if (endWithAnd)
                {
                    additionalWhere = additionalWhere.ReplaceLastOccurrance("AND", "", StringComparison.OrdinalIgnoreCase);
                }
                if (startsWithAnd)
                {
                    // additionalWhere = additionalWhere.ReplaceLastOccurrance("AND", "", StringComparison.OrdinalIgnoreCase);
                }
                else
                {
                    if (!string.IsNullOrEmpty(additionalWhere))
                        additionalWhere = $" AND {additionalWhere}";
                }

                whereClause += additionalWhere;


            }

            return new Tuple<List<DbParameter>, string>(list, whereClause);
        }



        /// <summary>
        /// Gets the non key fields.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>List&lt;AdvanceMember&gt;.</returns>
        public List<AdvanceMember> GetNonKeyFields<T>(T poco = null) where T : class
        {
            if (typeof(IDynamicMetaObjectProvider).IsAssignableFrom(typeof(T)))
            {
                // Get non primary key fields - the ones we want to update.
                return ExtFastMember.GetAdvanceMembersForDynamic<T>(poco).Where(m =>
                    m.SqlCustomAttritube.PrimaryKey != true && m.SqlCustomAttritube.Ignore != true &&
                    m.SqlTableAttritube == null).ToList();
            }
            else
            {
                // Get non primary key fields - the ones we want to update.
                return ExtFastMember.GetAdvanceMembers<T>(poco).Where(m =>
                    m.SqlCustomAttritube.PrimaryKey != true && m.SqlCustomAttritube.Ignore != true &&
                    m.SqlTableAttritube == null).ToList();
            }

        }

        /// <summary>
        /// Gets the key fields.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>List&lt;AdvanceMember&gt;.</returns>
        public List<AdvanceMember> GetKeyFields<T>(T poco = null) where T : class
        {
            if (typeof(IDynamicMetaObjectProvider).IsAssignableFrom(typeof(T)))
            {
                // Get the primary key fields - The properties in the class decorated with PrimaryKey attribute.
                return ExtFastMember.GetAdvanceMembersForDynamic<T>(poco).Where(m =>
                    m.SqlCustomAttritube.PrimaryKey == true && m.SqlCustomAttritube.Ignore != true &&
                    m.SqlTableAttritube == null).ToList();
            }
            else
            {
                // Get the primary key fields - The properties in the class decorated with PrimaryKey attribute.
                return ExtFastMember.GetAdvanceMembers<T>(poco).Where(m =>
                    m.SqlCustomAttritube.PrimaryKey == true && m.SqlCustomAttritube.Ignore != true &&
                    m.SqlTableAttritube == null).ToList();
            }
        }

        /// <summary>
        /// Gets the key fields.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>List&lt;AdvanceMember&gt;.</returns>
        public List<AdvanceMember> GetKeyFields(Type type)
        {

            // Get the primary key fields - The properties in the class decorated with PrimaryKey attribute.
            return ExtFastMember.GetAdvanceMembers(type).Where(m =>
                m.SqlCustomAttritube.PrimaryKey == true && m.SqlCustomAttritube.Ignore != true &&
                m.SqlTableAttritube == null).ToList();

        }


        /// <summary>
        /// Gets the non identity fields.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>List&lt;AdvanceMember&gt;.</returns>
        public static List<AdvanceMember> GetNonIdentityFields<T>(T poco = null) where T : class
        {
            if (typeof(IDynamicMetaObjectProvider).IsAssignableFrom(typeof(T)))
            {
                // Get the primary key fields - The properties in the class decorated with PrimaryKey attribute.
                return ExtFastMember.GetAdvanceMembersForDynamic<T>(poco).Where(m => m.SqlCustomAttritube.AutoIncrementBy == null && m.SqlCustomAttritube.Ignore != true && m.SqlTableAttritube == null).ToList();
            }
            else
            {
                // Get the primary key fields - The properties in the class decorated with PrimaryKey attribute.
                return ExtFastMember.GetAdvanceMembers<T>().Where(m => m.SqlCustomAttritube.AutoIncrementBy == null && m.SqlCustomAttritube.Ignore != true && m.SqlTableAttritube == null).ToList();
            }

        }

        /// <summary>
        /// Gets all non ignore fields.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>List&lt;AdvanceMember&gt;.</returns>
        public List<AdvanceMember> GetAllNonIgnoreFields<T>() where T : class
        {
            // Get the primary key fields - The properties in the class decorated with PrimaryKey attribute.
            var temp = ExtFastMember.GetAdvanceMembers<T>().Where(m => m.SqlCustomAttritube.Ignore != true && m.SqlTableAttritube == null).ToList();
            return temp;
        }

        /// <summary>
        /// Gets all non ignore fields.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>List&lt;AdvanceMember&gt;.</returns>
        public List<AdvanceMember> GetAllNonIgnoreFields(Type type)
        {
            // Get the primary key fields - The properties in the class decorated with PrimaryKey attribute.

            var temp = ExtFastMember.GetAdvanceMembers(type).Where(m => m.SqlCustomAttritube.Ignore != true && m.SqlTableAttritube == null).ToList();
            return temp;
        }



        /// <summary>
        /// Builds the insert query.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sqlBuilder">The SQL builder.</param>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="poco">The poco.</param>
        public void BuildInsertQuery<T>(StringBuilder sqlBuilder, string tableName, T poco, Expression<Func<T, object>> overrideKeys) where T : class
        {

            //var allFields = new List<AdvanceMember>(){};

            //if (overrideKeys != null)
            //{
            //    var outputFields = overrideKeys.GetPropertyNamesFromExpression();
            //    allFields = ExtFastMember.GetAdvanceMembers<T>().Where(m => outputFields.Contains(m.FastMember.Name)).ToList();
            //}
            //else
            //{
            //    allFields = GetNonIdentityFields<T>(poco);
            //}
            var allFields = GetNonIdentityFields<T>(poco);
            // Insert sql statement prefix 

            sqlBuilder.Append($"INSERT INTO {tableName} (");

            // Add field names
            allFields.ForEach(p => sqlBuilder.Append($"[{p.GetActualMemberName()}],"));
            sqlBuilder.Remove(sqlBuilder.Length - 1, 1); // Remove the last comma

            // Add parameter names for values
            sqlBuilder.Append(") VALUES (");
            allFields.ForEach(p => sqlBuilder.Append($"@{p.Member.Name},"));
            sqlBuilder.Remove(sqlBuilder.Length - 1, 1); // Remove the last comma
            sqlBuilder.Append(")");
        }


        /// <summary>
        /// Builds the insert query and return the expression.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sqlBuilder">The SQL builder.</param>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="expression"></param>
        public void BuildInsertQueryWithOutputs<T>(StringBuilder sqlBuilder, string tableName, Expression<Func<T, object>> expression) where T : class
        {

            var outputFields = expression.GetPropertyNamesFromExpression();

            var allFields = GetNonIdentityFields<T>();
            // Insert sql statement prefix 

            sqlBuilder.Append($"INSERT INTO {tableName} (");

            // Add field names
            allFields.ForEach(p => sqlBuilder.Append($"[{p.GetActualMemberName()}],"));
            sqlBuilder.Remove(sqlBuilder.Length - 1, 1); // Remove the last comma

            // Add parameter names for values
            sqlBuilder.Append($") {Environment.NewLine}");
            sqlBuilder.Append($" OUTPUT ");
            //outputFields.ForEach(delegate (string s) {
            //    sqlBuilder.Append($" INSERTED.[{s}] ,");
            //});
            var members = ExtFastMember.GetAdvanceMembers<T>();
            outputFields.ForEach(delegate (string s)
            {
                sqlBuilder.Append($" INSERTED.[{members.FirstOrDefault(av => av.Member.Name == s)?.GetActualMemberName() ?? s}] ,");
            });
            if (!outputFields.IsNullOrEmpty())
            {
                sqlBuilder.Remove(sqlBuilder.Length - 1, 1);
            }

            sqlBuilder.Append($"{Environment.NewLine} VALUES (");
            allFields.ForEach(p => sqlBuilder.Append($"@{p.Member.Name},"));
            sqlBuilder.Remove(sqlBuilder.Length - 1, 1); // Remove the last comma
            sqlBuilder.Append(")");
        }


        //  /// <summary>
        //  /// Builds the insert query and return the expression.
        //  /// </summary>
        //  /// <typeparam name="T"></typeparam>
        //  /// <param name="sqlBuilder">The SQL builder.</param>
        //  /// <param name="tableName">Name of the table.</param>
        //  /// <param name="expression"></param>
        //  internal void BuildUpdateQueryWithOutputs<T>(StringBuilder sqlBuilder, string tableName) where T : class
        //  {
        // 
        //      var outputFields = new List<string>() { };
        //      var members = ExtFastMember.GetAdvanceMembers<T>();
        // 
        //      if (members.Exists(a => a.SqlCustomAttritube.AutoIncrementBy != null && a.SqlCustomAttritube.AutoIncrementBy > 0))
        //          outputFields.Add(members.First(a => a.SqlCustomAttritube.AutoIncrementBy != null && a.SqlCustomAttritube.AutoIncrementBy > 0).Member.Name);
        // 
        // 
        //      var allFields = GetNonIdentityFields<T>();
        //      // Insert sql statement prefix 
        // 
        //      sqlBuilder.Append($"INSERT INTO {tableName} (");
        // 
        //      // Add field names
        //      allFields.ForEach(p => sqlBuilder.Append($"[{p.GetActualMemberName()}],"));
        //      sqlBuilder.Remove(sqlBuilder.Length - 1, 1); // Remove the last comma
        // 
        //      // Add parameter names for values
        //      sqlBuilder.Append($" ) {Environment.NewLine}");
        //      sqlBuilder.Append($" OUTPUT ");
        // 
        // 
        //      outputFields.ForEach(delegate (string s) {
        //          sqlBuilder.Append($" UPDATED.[{members.FirstOrDefault(av => av.Member.Name == s)?.SqlCustomAttritube.MapTo ?? s}] ,");
        //      });
        //      // outputFields.ForEach(delegate (string s) {
        //      //     sqlBuilder.Append($" INSERTED.[{s}] ,");
        //      // });
        //      if (!outputFields.IsNullOrEmpty())
        //      {
        //          sqlBuilder.Remove(sqlBuilder.Length - 1, 1);
        //      }
        // 
        //      sqlBuilder.Append($"{Environment.NewLine} VALUES (");
        //      allFields.ForEach(p => sqlBuilder.Append($"@{p.Member.Name},"));
        //      sqlBuilder.Remove(sqlBuilder.Length - 1, 1); // Remove the last comma
        //      sqlBuilder.Append(")");
        //  }


        /// <summary>
        /// Builds the insert query and return the expression.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sqlBuilder">The SQL builder.</param>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="expression"></param>
        public void BuildInsertQueryWithOutputs<T>(StringBuilder sqlBuilder, string tableName) where T : class
        {

            var outputFields = new List<string>() { };
            var members = ExtFastMember.GetAdvanceMembers<T>();

            if (members.Exists(a => a.SqlCustomAttritube.AutoIncrementBy != null && a.SqlCustomAttritube.AutoIncrementBy > 0))
                outputFields.Add(members.First(a => a.SqlCustomAttritube.AutoIncrementBy != null && a.SqlCustomAttritube.AutoIncrementBy > 0).Member.Name);


            var allFields = GetNonIdentityFields<T>();
            // Insert sql statement prefix 

            sqlBuilder.Append($"INSERT INTO {tableName} (");

            // Add field names
            allFields.ForEach(p => sqlBuilder.Append($"[{p.GetActualMemberName()}],"));
            sqlBuilder.Remove(sqlBuilder.Length - 1, 1); // Remove the last comma

            // Add parameter names for values
            sqlBuilder.Append($" ) {Environment.NewLine}");


            if (DatabaseType != DataBaseType.Sqlite) // SQLITE DOESN'T SUPPORT THIS SYNTAX
            {
                sqlBuilder.Append($" OUTPUT ");
                outputFields.ForEach(delegate (string s)
                {
                    sqlBuilder.Append($" INSERTED.[{members.FirstOrDefault(av => av.Member.Name == s)?.GetActualMemberName() ?? s}] ,");
                });
            }

            // outputFields.ForEach(delegate (string s) {
            //     sqlBuilder.Append($" INSERTED.[{s}] ,");
            // });
            if (!outputFields.IsNullOrEmpty())
            {
                sqlBuilder.Remove(sqlBuilder.Length - 1, 1);
            }

            sqlBuilder.Append($"{Environment.NewLine} VALUES (");
            allFields.ForEach(p => sqlBuilder.Append($"@{p.Member.Name},"));
            sqlBuilder.Remove(sqlBuilder.Length - 1, 1); // Remove the last comma
            sqlBuilder.Append(")");
        }

        /// <summary>
        /// Builds the update query.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sqlBuilder">The SQL builder.</param>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="poco">The poco.</param>
        public void BuildUpdateQuery<T>(StringBuilder sqlBuilder, string tableName, T poco, Expression<Func<T, object>> overrideKeys) where T : class
        {

            var keyFields = new List<AdvanceMember>() { };

            if (overrideKeys != null)
            {
                var outputFields = overrideKeys.GetPropertyNamesFromExpression();
                //     keyFields = ExtFastMember.GetAdvanceMembers<T>().Where(m => outputFields.Contains(m.FastMember.Name,new EqualityComparerString(StringComparison.OrdinalIgnoreCase))).ToList();
                keyFields = ExtFastMember.GetAdvanceMembers<T>().Where(m => outputFields.Contains(m.FastMember.Name)).ToList();
            }
            else
            {
                keyFields = GetKeyFields<T>(poco);
            }

            //  var keyFields = GetKeyFields<T>(poco);
            var updateFields = GetNonIdentityFields<T>(poco);

            // Build Update Statement Prefix
            sqlBuilder.Append($"UPDATE {tableName} SET ");

            // Build Set fields
            updateFields.ForEach(p => sqlBuilder.Append($"[{p.GetActualMemberName()}]=@{p.Member.Name},"));
            sqlBuilder.Remove(sqlBuilder.Length - 1, 1); // Remove the last comma

            // Build Where clause.
            sqlBuilder.Append(" ");
            BuildWhereClause(sqlBuilder, keyFields, overrideKeys == null);
        }

        /// <summary>
        /// Builds the delete query.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sqlBuilder">The SQL builder.</param>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="poco">The poco.</param>
        public void BuildDeleteQuery<T>(StringBuilder sqlBuilder, string tableName, T poco, Expression<Func<T, object>> overrideKeys) where T : class
        {

            var keyFields = new List<AdvanceMember>() { };

            if (overrideKeys != null)
            {
                var outputFields = overrideKeys.GetPropertyNamesFromExpression();
                keyFields = ExtFastMember.GetAdvanceMembers<T>().Where(m => outputFields.Contains(m.FastMember.Name)).ToList();
            }
            else
            {
                keyFields = GetKeyFields<T>(poco);
            }

            // var keyFields = GetKeyFields<T>(poco);

            sqlBuilder.Append($"DELETE FROM {tableName} ");
            BuildWhereClause(sqlBuilder, keyFields, overrideKeys == null);
        }

        /// <summary>
        /// Builds the get query.
        /// </summary>
        /// <param name="sqlBuilder">The SQL builder.</param>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="whereClause">The where clause.</param>
        public void BuildGetQuery(StringBuilder sqlBuilder, string tableName, string whereClause)
        {
            sqlBuilder.Append($"SELECT * FROM {tableName} ");
            if (string.IsNullOrEmpty(whereClause))
            {

            }
            else
            {
                sqlBuilder.Append(whereClause.ToLower().Replace(" ", "").StartsWith("where") ? $"{whereClause}" : $"WHERE {whereClause}");
            }
        }


        /// <summary>
        /// Builds the get query.
        /// </summary>
        /// <param name="sqlBuilder">The SQL builder.</param>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="whereClause">The where clause.</param>
        public void BuildGetQuery<T>(StringBuilder sqlBuilder, string tableName, string whereClause) where T : class
        {
            sqlBuilder.Append($"SELECT * FROM {tableName} ");
            if (string.IsNullOrEmpty(whereClause))
            {

            }
            else
            {
                sqlBuilder.Append(whereClause.ToLower().Replace(" ", "").StartsWith("where") ? $"{whereClause}" : $"WHERE {whereClause}");
            }
        }

        /// <summary>
        /// Builds the upsert query.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sqlBuilder">The SQL builder.</param>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="poco">The poco.</param>
        public void BuildUpsertQuery<T>(StringBuilder sqlBuilder, string tableName, T poco, Expression<Func<T, object>> overrideKeys) where T : class
        {

            var keyFields = new List<AdvanceMember>() { };

            if (overrideKeys != null)
            {
                var outputFields = overrideKeys.GetPropertyNamesFromExpression();
                keyFields = ExtFastMember.GetAdvanceMembers<T>().Where(m => outputFields.Contains(m.FastMember.Name)).ToList();
            }
            else
            {
                keyFields = GetKeyFields<T>(poco);
            }
            //  var keyFields = GetKeyFields<T>(poco);
            // Build If Exists statement
            sqlBuilder.Append($"IF EXISTS ( SELECT * FROM {tableName} ");
            BuildWhereClause(sqlBuilder, keyFields, overrideKeys == null);
            sqlBuilder.Append(" ) BEGIN ");

            // Build Update or Insert statement
            BuildUpdateQuery(sqlBuilder, tableName, poco, overrideKeys);
            sqlBuilder.Append(" END ELSE BEGIN ");
            BuildInsertQuery(sqlBuilder, tableName, poco, overrideKeys);
            sqlBuilder.Append(" END");
        }




        /// <summary>
        /// Converts to database value.
        /// </summary>
        /// <param name="member">The member.</param>
        /// <param name="value">The value.</param>
        /// <returns>System.Object.</returns>
        public object ConvertToDatabaseValue(DynamicMember member, object value)
        {

            if (value == null)
            {
                return DBNull.Value;
            }

            if (member.Type == typeof(DateTime) && (DateTime)value == DateTime.MinValue || member.Type == typeof(DateTime?) && (DateTime)value == DateTime.MinValue)
            {
                return new DateTime(1753, 01, 01);
                //  return (DateTime?)SqlDateTime.MinValue;
            }

            if (member.Type == typeof(byte[]))
            {
                return value;
            }
            if (!member.Type.FullName.StartsWith("System.") || value is IEnumerable && member.Type != typeof(string) || value.GetType().IsCSharpClass()) // strings are enumerable of characters 
            {
                var a = JsonConvert.SerializeObject(value);
                if (a == null) return DBNull.Value;
                return a.Replace("'", "''");
            }
            return value;
        }



        /// <summary>
        /// Builds the SQL parameter list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="poco">The poco.</param>
        /// <returns>List&lt;DbParameter&gt;.</returns>
        public List<DbParameter> BuildDbParameterList<T>(T poco, Func<string, object, DbParameter> GetNewParameter, IXmlSerializer XmlSerializer, IJsonSerializer JsonSerializer, ICsvSerializer CsvSerializer) where T : class
        {
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
                        parameterValue = ConvertToDatabaseValue(p.Member, p.Value);
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
                                    XmlSerializer.IsNullThrow(nameof(XmlSerializer),
                                        new BadCodeException(
                                            $"YOU FOOL!!! Your claiming the property {p.Member.Name} in the type {p.Member.Type.FullName} value is in XML Format but your not specifying how to deserialize/serialize it )"));
                                    parameterValue = XmlSerializer.SerializeToString(p.Value);
                                    break;
                                case SerializableType.JSON:
                                    JsonSerializer.IsNullThrow(nameof(JsonSerializer),
                                        new BadCodeException(
                                            $"YOU FOOL!!! Your claiming the property {p.Member.Name} in the type {p.Member.Type.FullName} value is in JSON Format but your not specifying how to deserialize/serialize it )"));
                                    parameterValue = JsonSerializer.SerializeToString(p.Value);
                                    break;
                                case SerializableType.CSV:
                                    CsvSerializer.IsNullThrow(nameof(CsvSerializer),
                                        new BadCodeException(
                                            $"YOU FOOL!!! Your claiming the property {p.Member.Name} in the type {p.Member.Type.FullName} value is in CSV Format but your not specifying how to deserialize/serialize it )"));
                                    parameterValue = CsvSerializer.SerializeToString(p.Value);
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
                    parameterValue = ConvertToDatabaseValue(p.Member, p.Value);
                }
                // var command = GetNewCommand();

                list.Add(GetNewParameter($"@{p.Member.Name}", parameterValue));

            });
            return list;
        }






        /// <summary>
        /// 
        /// </summary>
        /// <param name="tableAlias"></param>
        /// <param name="advanceMembers"></param>
        /// <param name="sqlSyntax"></param>
        /// <returns>item 1 is the actual columns being selected
        ///          item 2 is the split on column</returns>
        internal Tuple<string, string> BulidSelectColumnStatement(char tableAlias, List<AdvanceMember> advanceMembers, SqlSyntaxHelper sqlSyntax)
        {
            var sb = new StringBuilder();
            var isFirstTime = true;
            var splitOn = "";
            advanceMembers.Where(a1 => a1.SqlTableAttritube == null && a1.SqlCustomAttritube.Ignore != true).ToList().ForEach(delegate (AdvanceMember member) // BUILD SQL COLUMNS
            {
                var columnName = $"{tableAlias}.{sqlSyntax.GetTableOpenChar()}{member.Member.Name}{sqlSyntax.GetTableClosedChar()}";
                sb.AppendLine($"{columnName} , ");
                if (isFirstTime)
                {
                    splitOn = member.Member.Name;
                    isFirstTime = false;
                }
            });
            return new Tuple<string, string>(sb.ToString(), splitOn);
        }

        internal void BuildJoinOnStatement(List<AdvanceMember> members, char mainTableAlias, List<AdvanceMember> members1, char secondTableAlias, StringBuilder sqlFromBuilder)
        {
            var safeKeyword = " AND ";
            members.Where(a => !a.SqlCustomAttritube.MappingIds.IsNullOrEmpty() && a.SqlTableAttritube == null).ToList().ForEach(delegate (AdvanceMember mainTableColumn) // LOOP THRU MAIN TABLE PROPERTIES 
            {

                members1.Where(a => !a.SqlCustomAttritube.MappingIds.IsNullOrEmpty()).ToList().ForEach(delegate (AdvanceMember secondTableColumn)
                {

                    if (mainTableColumn.SqlCustomAttritube.MappingIds.ContainAnySameItem(secondTableColumn.SqlCustomAttritube.MappingIds))
                    {


                        sqlFromBuilder.Append($" {mainTableAlias}.{mainTableColumn.Member.Name} " +
                                              $"= {secondTableAlias}.{secondTableColumn.Member.Name} ");
                        sqlFromBuilder.Append(safeKeyword);

                        // var iHateThis = sqlFromBuilder.ToString().ReplaceLastOccurrance(safeKeyword, string.Empty, StringComparison.Ordinal);
                        // sqlFromBuilder.Clear();
                        //  sqlFromBuilder.Append(sqlFromBuilder.ToString().ReplaceLastOccurrance(safeKeyword, string.Empty, StringComparison.Ordinal));
                        // sqlFromBuilder.Clear();
                    }
                });

            });

            sqlFromBuilder = sqlFromBuilder.ReplaceLastOccurrance(safeKeyword, string.Empty, StringComparison.Ordinal);

        }


        internal Dictionary<Type, char> GetTableAliasRecursive<T>() where T : class
        {

            var currentAlias = 'A';
            var lookup = new Dictionary<Type, char>()
            {
                 {   typeof(T),currentAlias  }
            };


            void addAlias(AdvanceMember m)
            {
                if (lookup.ContainsKey(m.FastMember.Type))
                {

                }
                else
                {
                    currentAlias = Alphabet.GetNextLetter(currentAlias);
                    lookup.Add(m.FastMember.Type, currentAlias);
                }

            }



            void getTableMembers(Type type)
            {
                ExtFastMember.GetAdvanceMembers(type).Where(m => m.SqlTableAttritube != null).ToList().ForEach(delegate (AdvanceMember m)
                {
                    addAlias(m);
                    getTableMembers(m.FastMember.Type);
                });
            }



            getTableMembers(typeof(T));


            return lookup;
        }

    }
}
