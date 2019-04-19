using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using DotNetHelper_Contracts.CustomException;
using DotNetHelper_Contracts.Enum;
using DotNetHelper_Contracts.Extension;
using DotNetHelper_Contracts.Helpers;

using FastMember;
using DataValidationAttritube = DotNetHelper_Serializer.Attribute.DataValidationAttritube;
using DataValidationAttritubeMembers = DotNetHelper_Serializer.Attribute.DataValidationAttritubeMembers;
using SqlColumnAttritube = DotNetHelper_Serializer.Attribute.SqlColumnAttritube;
using SqlColumnAttritubeMembers = DotNetHelper_Serializer.Attribute.SqlColumnAttritubeMembers;
using SqlTableAttritube = DotNetHelper_Serializer.Attribute.SqlTableAttritube;
using SqlTableAttritubeMembers = DotNetHelper_Serializer.Attribute.SqlTableAttritubeMembers;
using SQLJoinType = DotNetHelper_Serializer.Attribute.SQLJoinType;

namespace DotNetHelper_Serializer.Extension
{


    public sealed class DynamicMember 
    {
        public string Name { get; internal set; }
        public Type Type { get; internal set; }
        public bool CanWrite { get; internal set; }
        public bool CanRead { get; internal set; }

        internal object Value { get; }

        public System.Attribute GetAttribute(Type attributeType, bool inherit)
        {
            throw  new NotImplementedException("This logic hasn't been implemented  for Dynamic Objects yet");
        }

        public bool IsDefined(Type attributeType)
        {
            throw new NotImplementedException("This logic hasn't been implemented for Dynamic Objects yet");
        }

        internal DynamicMember MapToDynamicMember(Member fastMember)
        {
            Name = fastMember.Name;
            CanRead = fastMember.CanRead;
            CanWrite = fastMember.CanWrite;
            Type = fastMember.Type;
            return this;

        }

    }

    public class AdvanceMember
    {
        public Member FastMember { get; internal set; }
        public DynamicMember Member { get; internal set; } = new DynamicMember();

        public object Value { get; set; }
        public SqlColumnAttritube SqlCustomAttritube { get; set; }
        public SqlTableAttritube SqlTableAttritube { get; set; }
        public DataValidationAttritube Validation { get; set; }
        //public List<string> ValidationErrors { get; set; } = new List<string>();
        //public List<SqlColumnAttritubeMembers> AttritubeType { get; set; } 
    }



    public static class ExtFastMember
    {
        //internal static Dictionary<string,SqlColumnAttritube> LookupAttritubes { get; } = new Dictionary<string, SqlColumnAttritube>();
        //internal static Dictionary<string, DataValidationAttritube> LookupValidation { get; } = new Dictionary<string, DataValidationAttritube>();
        internal static Dictionary<string, List<AdvanceMember>> Lookup { get; } = new Dictionary<string, List<AdvanceMember>>();



        public static string GetActualMemberName(this AdvanceMember av)
        {
            return string.IsNullOrEmpty(av.SqlCustomAttritube.MapTo) ? av.Member.Name : av.SqlCustomAttritube.MapTo;
        }

 
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="poco">If Null Default Value Will Be Used For Members</param>
        /// <returns>A List Of Advance Members Of T</returns>
        public static List<AdvanceMember> GetAdvanceMembers(Type type) 
        {
       
            var list = new List<AdvanceMember>() { };

            var accessor = TypeAccessor.Create(type, true); // Fastmember Handles Performance Don't Worries Of Multiple Calls Via Dictionary

            lock (Lookup)
            {
                if (Lookup.ContainsKey(type.FullName))
            {
                var result = Lookup.First(pair => pair.Key == type.FullName).Value;
                var clonedList = new List<AdvanceMember>() { };  // WE MUST CLONED OTHERWISE WE ARE SCREWING OUR SELVE OVER
                foreach (var cv in result)
                {
                    var av = new AdvanceMember()
                    {
                        Member = cv.Member
                        // ,Value = GetMemberValue(cv.FastMember, poco, accessor)
                        ,
                        SqlCustomAttritube = cv.SqlCustomAttritube
                        ,
                        FastMember = cv.FastMember
                        ,
                        SqlTableAttritube = cv.SqlTableAttritube
                        ,
                        Validation = cv.Validation
                    };
                    //av.Value = GetMemberValue(av.FastMember, poco, accessor);
                    clonedList.Add(av);
                }
                return clonedList.ToList();
            }

                var properties = type.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).ToList();

                var enumSql = System.Enum.GetValues(typeof(SqlColumnAttritubeMembers)).Cast<SqlColumnAttritubeMembers>()
                .ToList(); // Performance Reason Why This Is Upper Global
            var enumValidation = System.Enum.GetValues(typeof(DataValidationAttritubeMembers))
                .Cast<DataValidationAttritubeMembers>().ToList(); // Performance Reason Why This Is Upper Global

            accessor.GetMembers().ToList().ForEach(delegate (Member member)
            {
                if (properties.Where(i => i.Name == member.Name).ToList().Count <= 0)
                {
                    return;
                }
                var property = properties.First(info => info.Name == member.Name);
                var advance = new AdvanceMember
                {
                    FastMember = member,
                    Member = new DynamicMember().MapToDynamicMember(member),
                  //  Value = GetMemberValue(member, poco, accessor),
                    SqlCustomAttritube = new SqlColumnAttritube(),
                    Validation = new DataValidationAttritube(),
                    // ValidationErrors = new List<string>() { }
                };



                var sqlAttributeList = property.CustomAttributes
                    .Where(data => data.AttributeType == typeof(SqlColumnAttritube)).ToList();
                var validationAttributeList = property.CustomAttributes
                    .Where(data => data.AttributeType == typeof(DataValidationAttritube)).ToList();

                var tableAttribute = property.CustomAttributes.Where(data => data.AttributeType == typeof(SqlTableAttritube)).ToList();
                if (!tableAttribute.IsNullOrEmpty())
                {
                    advance.SqlTableAttritube = new SqlTableAttritube();
                
                }


                if (sqlAttributeList.Any())
                    enumSql.ForEach(delegate (SqlColumnAttritubeMembers members)
                    {
                        var sqlAttribute = sqlAttributeList.First();
                        var validname = $"{members}";
                        var customAttributeNamedArguments = sqlAttributeList.First()?.NamedArguments;
                        if (customAttributeNamedArguments != null && customAttributeNamedArguments.Any(arg1 => arg1.MemberName == $"{members}"))
                        {
                            var value = sqlAttribute?.NamedArguments.First(arg => arg.MemberName == validname).TypedValue.Value;

                            switch (members)
                            {
                                case SqlColumnAttritubeMembers.SetMaxColumnSize:
                                    advance.SqlCustomAttritube.MaxColumnSize = (int)value;
                                    break;
                                case SqlColumnAttritubeMembers.SetAutoIncrementBy:
                                    advance.SqlCustomAttritube.AutoIncrementBy = (int)value;
                                    break;
                                case SqlColumnAttritubeMembers.SetStartIncrementAt:
                                    advance.SqlCustomAttritube.StartIncrementAt = (int)value;
                                    break;
                                case SqlColumnAttritubeMembers.SetUtcDateTime:
                                    advance.SqlCustomAttritube.UtcDateTime = (bool)value;
                                    break;
                                case SqlColumnAttritubeMembers.SetPrimaryKey:
                                    advance.SqlCustomAttritube.PrimaryKey = (bool)value;
                                    break;
                                case SqlColumnAttritubeMembers.SetNullable:
                                    advance.SqlCustomAttritube.Nullable = (bool)value;
                                    break;
                                case SqlColumnAttritubeMembers.SetApiId:
                                    advance.SqlCustomAttritube.ApiId = (bool)value;
                                    break;
                                case SqlColumnAttritubeMembers.SetSyncTime:
                                    advance.SqlCustomAttritube.SyncTime = (bool)value;
                                    break;
                                case SqlColumnAttritubeMembers.SetIgnore:
                                    advance.SqlCustomAttritube.Ignore = (bool)value;
                                    break;
                                case SqlColumnAttritubeMembers.MapTo:
                                    advance.SqlCustomAttritube.MapTo = (string)value;
                                    break;
                                case SqlColumnAttritubeMembers.DefaultValue:
                                    advance.SqlCustomAttritube.DefaultValue = value;
                                    break;
                                case SqlColumnAttritubeMembers.TSQLDefaultValue:
                                    advance.SqlCustomAttritube.TSQLDefaultValue = (string)value;
                                    break;
                                case SqlColumnAttritubeMembers.SetxRefTableType:
                                    advance.SqlCustomAttritube.xRefTableType = (Type) value;
                                    break;
                                case SqlColumnAttritubeMembers.xRefTableSchema:
                                    advance.SqlCustomAttritube.xRefTableSchema = (string)value;
                                    break;
                                case SqlColumnAttritubeMembers.xRefTableName:
                                    advance.SqlCustomAttritube.xRefTableName = (string)value;
                                    break;
                                case SqlColumnAttritubeMembers.xRefJoinOnColumn:
                                    advance.SqlCustomAttritube.xRefJoinOnColumn = (string)value;
                                    break;
                                case SqlColumnAttritubeMembers.SetxRefOnUpdateCascade:
                                    advance.SqlCustomAttritube.xRefOnUpdateCascade = (bool)value;
                                    break;
                                case SqlColumnAttritubeMembers.SetxRefOnDeleteCascade:
                                    advance.SqlCustomAttritube.xRefOnDeleteCascade = (bool)value;
                                    break;
                                case SqlColumnAttritubeMembers.MappingIds:
                                    var a = value as ICollection<System.Reflection.CustomAttributeTypedArgument>;
                                    var b = new List<string>(){};
                                    b.AddRange(a.Select(c => c.ToString().ReplaceFirstOccurrance("\"", string.Empty, StringComparison.Ordinal).ReplaceLastOccurrance("\"", string.Empty, StringComparison.Ordinal)));
                                    advance.SqlCustomAttritube.MappingIds = b.ToArray();
                                    break;

                                case SqlColumnAttritubeMembers.SerializableType:

                                    advance.SqlCustomAttritube.SerializableType = value.ToEnum<SerializableType>();
                                    
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException(nameof(members), members, null);
                            }
                        }

                    });

                if (validationAttributeList.Any())
                    enumValidation.ForEach(delegate (DataValidationAttritubeMembers members)
                    {
                        var validationAttribute = validationAttributeList.First();
                        var validname = $"{members}";
                        var customAttributeNamedArguments = validationAttributeList.First()?.NamedArguments;
                        if (customAttributeNamedArguments != null &&
                            customAttributeNamedArguments.Any(arg1 => arg1.MemberName == $"{members}"))
                        {
                            var value = validationAttribute?.NamedArguments.First(arg => arg.MemberName == validname)
                                .TypedValue.Value;

                            switch (members)
                            {
                                case DataValidationAttritubeMembers.SetMaxLengthSize:
                                    advance.Validation.MaxLengthSize = (int)value;
                                    break;
                                case DataValidationAttritubeMembers.SetRequireValue:
                                    advance.Validation.RequireValue = (bool)value;
                                    break;
                                case DataValidationAttritubeMembers.SetDataType:
                                    advance.Validation.DataType = (DataType)value;
                                    break;
                                case DataValidationAttritubeMembers.SetCanContainNumbers:
                                    advance.Validation.CanContainNumbers = (bool)value;
                                    break;
                                case DataValidationAttritubeMembers.SetCanContainLetter:
                                    advance.Validation.CanContainLetter = (bool)value;
                                    break;
                                case DataValidationAttritubeMembers.SetIgnore:
                                    advance.Validation.Ignore = (bool)value;
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException(nameof(members), members, null);
                            }
                        }
                    });

                list.Add(advance);

            });

            Lookup.Add(type.FullName, list);
               }

            return list;
        }










        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="poco">If Null Default Value Will Be Used For Members</param>
        /// <returns>A List Of Advance Members Of T</returns>
        public static List<AdvanceMember> GetAdvanceMembersForDynamic<T>(T poco = null) where T : class
        {
      
            if (poco == null)
            {
                throw new BadCodeException($"Can Get The Properties Of The Dynamic Type {typeof(T).FullName} Because No Properties Has Been Assigned to the object");
            }
          
            var list = new List<AdvanceMember>() { };


            var props = DynamicObjectHelper.GetProperties(poco as ExpandoObject);

            props.ForEach(delegate(KeyValuePair<string, object> pair)
            {
                var advance = new AdvanceMember
                {
                  
                    Member = new DynamicMember() { Type = pair.Value.GetType() ,Name = pair.Key, CanWrite =  true, CanRead = true },
                    Value = pair.Value,
                    SqlCustomAttritube = new SqlColumnAttritube(),
                    Validation = new DataValidationAttritube(),
                };
               

                list.Add(advance);
            });
           
            return list;
        }






        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="poco">If Null Default Value Will Be Used For Members</param>
        /// <returns>A List Of Advance Members Of T</returns>
        public static List<AdvanceMember> GetAdvanceMembers<T>(T poco = null) where T : class
        {
            if (typeof(T) == typeof(ExpandoObject) || typeof(IDynamicMetaObjectProvider).IsAssignableFrom(typeof(T))) return GetAdvanceMembersForDynamic(poco);
            if (poco == null)
            {
                poco = TypeExtension.New<T>.Instance();
            }
            var list = new List<AdvanceMember>() { };

            var accessor = TypeAccessor.Create(poco.GetType(), true); // Fastmember Handles Performance Don't Worries Of Multiple Calls Via Dictionary
            var type = poco.GetType();

            // Add Support For Dynamic Objects
            // if (typeof(T) == typeof(ExpandoObject))
            // {
            //     if (realParameterValue == null)
            //     {
            //         throw new BadCodeException(
            //             $"Can Get The Properties Of The Type {type.FullName} Because No Properties Has Been Assigned");
            //     }
            //     else
            //     {
            //         var props = DynamicObjectHelper.GetProperties(realParameterValue as ExpandoObject);
            //         var member = TypeExtension.New<Member>.Instance();
            //     }
            // }

            lock (Lookup)
            {
                if (Lookup.ContainsKey(type.FullName))
            {
                var result = Lookup.First(pair => pair.Key == type.FullName).Value;
                var clonedList = new List<AdvanceMember>(){};  // WE MUST CLONED OTHERWISE WE ARE SCREWING OUR SELVE OVER
                foreach (var cv in result)
                {
                    var av = new AdvanceMember()
                    {
                        Member = cv.Member
                      // ,Value = GetMemberValue(cv.FastMember, poco, accessor)
                       ,SqlCustomAttritube = cv.SqlCustomAttritube
                       ,FastMember =  cv.FastMember
                       ,SqlTableAttritube =  cv.SqlTableAttritube
                       ,Validation =  cv.Validation
                    };
                    av.Value = GetMemberValue(av.FastMember, poco, accessor);
                    clonedList.Add(av);
                }
                return clonedList.ToList();
            }

            var properties = type.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).ToList();

            var enumSql = System.Enum.GetValues(typeof(SqlColumnAttritubeMembers)).Cast<SqlColumnAttritubeMembers>().ToList(); // Performance Reason Why This Is Upper Global
            var enumTableSql = System.Enum.GetValues(typeof(SqlTableAttritubeMembers)).Cast<SqlTableAttritubeMembers>().ToList(); // Performance Reason Why This Is Upper Global
            var enumValidation = System.Enum.GetValues(typeof(DataValidationAttritubeMembers)) .Cast<DataValidationAttritubeMembers>().ToList(); // Performance Reason Why This Is Upper Global

            accessor.GetMembers().ToList().ForEach(delegate (Member member)
            {
                if (properties.Where(i => i.Name == member.Name).ToList().Count <= 0)
                {
                    return;
                }
                var property = properties.First(info => info.Name == member.Name);
                var advance = new AdvanceMember
                {
                    FastMember = member,
                    Member = new DynamicMember().MapToDynamicMember(member),
                    Value = GetMemberValue(member, poco, accessor),
                    SqlCustomAttritube = new SqlColumnAttritube(),
                    Validation = new DataValidationAttritube(),
                 
                    // ValidationErrors = new List<string>() { }
                };



                var sqlAttributeList = property.CustomAttributes
                    .Where(data => data.AttributeType == typeof(SqlColumnAttritube)).ToList();
                var validationAttributeList = property.CustomAttributes
                    .Where(data => data.AttributeType == typeof(DataValidationAttritube)).ToList();

                var tableAttribute = property.CustomAttributes.Where(data => data.AttributeType == typeof(SqlTableAttritube)).ToList();
                if (!tableAttribute.IsNullOrEmpty())
                {

                    enumTableSql.ForEach(delegate(SqlTableAttritubeMembers members)
                    {
                        var sqlAttribute = tableAttribute.First();
                        var validname = $"{members}";
                        var customAttributeNamedArguments = tableAttribute.First()?.NamedArguments;
                        if (customAttributeNamedArguments != null && customAttributeNamedArguments.Any(arg1 => arg1.MemberName == $"{members}"))
                        {
                            var value = sqlAttribute?.NamedArguments.First(arg => arg.MemberName == validname).TypedValue.Value;
                            if(advance.SqlTableAttritube == null) advance.SqlTableAttritube = new SqlTableAttritube(); // I DON'T KNOW HOW I FEEL ABOUT THIS RIGHT HERE IS MY SWAG
                            switch (members)
                            {
                                case SqlTableAttritubeMembers.JoinType:
                                    var temp = value.ToEnum<SQLJoinType>();
                                    advance.SqlTableAttritube.JoinType = temp;
                                    break;
                                case SqlTableAttritubeMembers.TableName:
                                    advance.SqlTableAttritube.TableName = (string)value;
                                    break;
                                case SqlTableAttritubeMembers.XReferenceTable:
                                    advance.SqlTableAttritube.XReferenceTable = (Type) value;
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException(nameof(members), members, null);
                            }
                        }

                    });
                    //advance.SqlTableAttritube = new SqlTableAttritube();
                }


                if (sqlAttributeList.Any())
                    enumSql.ForEach(delegate (SqlColumnAttritubeMembers members)
                    {
                        var sqlAttribute = sqlAttributeList.First();
                        var validname = $"{members}";
                        var customAttributeNamedArguments = sqlAttributeList.First()?.NamedArguments;
                        if (customAttributeNamedArguments != null &&
                            customAttributeNamedArguments.Any(arg1 => arg1.MemberName == $"{members}"))
                        {
                            var value = sqlAttribute?.NamedArguments.First(arg => arg.MemberName == validname)
                                .TypedValue.Value;

                            switch (members)
                            {
                                case SqlColumnAttritubeMembers.SetMaxColumnSize:
                                    advance.SqlCustomAttritube.MaxColumnSize = (int)value;
                                    break;
                                case SqlColumnAttritubeMembers.SetAutoIncrementBy:
                                    advance.SqlCustomAttritube.AutoIncrementBy = (int)value;
                                    break;
                                case SqlColumnAttritubeMembers.SetStartIncrementAt:
                                    advance.SqlCustomAttritube.StartIncrementAt = (int)value;
                                    break;
                                case SqlColumnAttritubeMembers.SetUtcDateTime:
                                    advance.SqlCustomAttritube.UtcDateTime = (bool)value;
                                    break;
                                case SqlColumnAttritubeMembers.SetPrimaryKey:
                                    advance.SqlCustomAttritube.PrimaryKey = (bool)value;
                                    break;
                                case SqlColumnAttritubeMembers.SetNullable:
                                    advance.SqlCustomAttritube.Nullable = (bool)value;
                                    break;
                                case SqlColumnAttritubeMembers.SetApiId:
                                    advance.SqlCustomAttritube.ApiId = (bool)value;
                                    break;
                                case SqlColumnAttritubeMembers.SetSyncTime:
                                    advance.SqlCustomAttritube.SyncTime = (bool)value;
                                    break;
                                case SqlColumnAttritubeMembers.SetIgnore:
                                    advance.SqlCustomAttritube.Ignore = (bool)value;
                                    break;
                                case SqlColumnAttritubeMembers.MapTo:
                                    advance.SqlCustomAttritube.MapTo = (string)value;
                                    break;
                                case SqlColumnAttritubeMembers.DefaultValue:
                                    advance.SqlCustomAttritube.DefaultValue = value;
                                    break;
                                case SqlColumnAttritubeMembers.TSQLDefaultValue:
                                    advance.SqlCustomAttritube.TSQLDefaultValue = (string)value;
                                    break;
                                case SqlColumnAttritubeMembers.SetxRefTableType:
                                    advance.SqlCustomAttritube.xRefTableType = (Type) value;
                                    break;
                                case SqlColumnAttritubeMembers.xRefTableSchema:
                                    advance.SqlCustomAttritube.xRefTableSchema = (string)value;
                                    break;
                                case SqlColumnAttritubeMembers.xRefTableName:
                                    advance.SqlCustomAttritube.xRefTableName = (string)value;
                                    break;
                                case SqlColumnAttritubeMembers.xRefJoinOnColumn:
                                    advance.SqlCustomAttritube.xRefJoinOnColumn = (string)value;
                                    break;
                                case SqlColumnAttritubeMembers.SetxRefOnUpdateCascade:
                                    advance.SqlCustomAttritube.xRefOnUpdateCascade = (bool)value;
                                    break;
                                case SqlColumnAttritubeMembers.SetxRefOnDeleteCascade:
                                    advance.SqlCustomAttritube.xRefOnDeleteCascade = (bool)value;
                                    break;
                                case SqlColumnAttritubeMembers.MappingIds:
                                    var a = value as ICollection<System.Reflection.CustomAttributeTypedArgument>;
                                    var b = new List<string>(){};
                                    b.AddRange(a.Select(c => c.ToString().ReplaceFirstOccurrance("\"", string.Empty, StringComparison.Ordinal).ReplaceLastOccurrance("\"", string.Empty, StringComparison.Ordinal)));
                                    advance.SqlCustomAttritube.MappingIds = b.ToArray();
                                    break;
                                case SqlColumnAttritubeMembers.SerializableType:
                                    advance.SqlCustomAttritube.SerializableType = value.ToEnum<SerializableType>();
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException(nameof(members), members, null);
                            }
                        }

                    });

                if (validationAttributeList.Any())
                    enumValidation.ForEach(delegate (DataValidationAttritubeMembers members)
                    {
                        var validationAttribute = validationAttributeList.First();
                        var validname = $"{members}";
                        var customAttributeNamedArguments = validationAttributeList.First()?.NamedArguments;
                        if (customAttributeNamedArguments != null &&
                            customAttributeNamedArguments.Any(arg1 => arg1.MemberName == $"{members}"))
                        {
                            var value = validationAttribute?.NamedArguments.First(arg => arg.MemberName == validname)
                                .TypedValue.Value;

                            switch (members)
                            {
                                case DataValidationAttritubeMembers.SetMaxLengthSize:
                                    advance.Validation.MaxLengthSize = (int)value;
                                    break;
                                case DataValidationAttritubeMembers.SetRequireValue:
                                    advance.Validation.RequireValue = (bool)value;
                                    break;
                                case DataValidationAttritubeMembers.SetDataType:
                                    advance.Validation.DataType = (DataType)value;
                                    break;
                                case DataValidationAttritubeMembers.SetCanContainNumbers:
                                    advance.Validation.CanContainNumbers = (bool)value;
                                    break;
                                case DataValidationAttritubeMembers.SetCanContainLetter:
                                    advance.Validation.CanContainLetter = (bool)value;
                                    break;
                                case DataValidationAttritubeMembers.SetIgnore:
                                    advance.Validation.Ignore = (bool)value;
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException(nameof(members), members, null);
                            }
                        }
                    });

                list.Add(advance);

            });

            Lookup.Add(type.FullName, list);
            }

            return list;
        }

        public static object GetMemberValue<T>(Member p, T poco, TypeAccessor accessor)
        {
                if (poco == null) return null;
                return accessor[poco, p.Name] == null ? null : accessor[poco, p.Name];  
        }


        internal static object GetMemberValue(DynamicMember dynamicMember)
        {
            return dynamicMember.Value;
        }


        public static void SetMemberValue<T>(T poco, string propertyName, object value)
        {
            var accessor = TypeAccessor.Create(typeof(T), true);
            var members = accessor.GetMembers().ToList();
            if (poco == null || string.IsNullOrEmpty(propertyName) || !accessor.GetMembers().ToList().Exists(a => string.Equals(a.Name, propertyName, StringComparison.CurrentCultureIgnoreCase))) throw new BadCodeException("SetMemberValue Method Can't Work If You Pass It Null Object Or Invalid Property Name");

            var needToBeType = members.First(m => m.Name == propertyName).Type;

            if (value == null)
            {
                accessor[poco, propertyName] = value;
                return;
            }
            if (value.GetType() != needToBeType)
            {

                if (needToBeType == typeof(DateTimeOffset) || needToBeType == typeof(DateTimeOffset?)) 
                {
                    value = TypeDescriptor.GetConverter(needToBeType).ConvertFrom(value);
                }
                else
                {
                    value = needToBeType.IsEnum
                  ? Enum.Parse(needToBeType.GetUnderlyingNullableType(), value.ToString(), true)
                  : Convert.ChangeType(value, needToBeType.GetUnderlyingNullableType(), null);
                }
              
            }
            
            
            accessor[poco, propertyName] = value;
        }





    }
}