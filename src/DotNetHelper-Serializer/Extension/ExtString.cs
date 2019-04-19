using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DotNetHelper_Contracts.Extension;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DotNetHelper_Serializer.Extension
{
   public static class ExtString
    {
        /// <summary>
        /// Maps Json to a list of T 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <param name="throwOnError"> if true return empty list on exception during parsing json</param>
        /// <returns></returns>
        public static List<T> JsonToList<T>(this string json, bool throwOnError, JsonSerializer serializer = null, bool validateData = false) where T : class
        {
            serializer = serializer ?? new JsonSerializer();
            if (throwOnError)
            {
                var token = JToken.Parse(json);
                if (token is JArray array)
                {
                    if (validateData)
                    {
                        var members = ExtFastMember.GetAdvanceMembers<T>().Where(b => (b.SqlCustomAttritube.PrimaryKey == true && b.SqlCustomAttritube.AutoIncrementBy != null) || b.SqlCustomAttritube.Nullable == false).ToList();
                        if (members.Count <= 0) return token.ToObject<List<T>>(serializer);
                        array.ForEach(delegate (JToken jToken)
                        {

                            members.ForEach(delegate (AdvanceMember m)
                            {

                                if (jToken[m.Member.Name] == null)
                                {
                                    if(m.SqlCustomAttritube.AutoIncrementBy == null && m.SqlCustomAttritube.StartIncrementAt == null && m.SqlCustomAttritube.TSQLDefaultValue == null) // IDENTITY SHOULDN'T MATTER IF THEY EXIST BECAUSE THE DATABASE CREATES THEM
                                    throw new InvalidDataException($"The Field {m.Member.Name} Is Missing");
                                }

                            });

                        });
                    }

                    return token.ToObject<List<T>>(serializer);
                }
                else
                {
                    if (validateData)
                    {
                        var members = ExtFastMember.GetAdvanceMembers<T>().Where(b => (b.SqlCustomAttritube.PrimaryKey == true && b.SqlCustomAttritube.AutoIncrementBy != null) || b.SqlCustomAttritube.Nullable == false).ToList();
                        if (members.Count <= 0) return token.ToObject<List<T>>(serializer);


                        members.ForEach(delegate (AdvanceMember m)
                        {
                            if (token[m.Member.Name] == null)
                            {
                                if (m.SqlCustomAttritube.AutoIncrementBy == null && m.SqlCustomAttritube.StartIncrementAt == null && m.SqlCustomAttritube.TSQLDefaultValue == null ) // IDENTITY SHOULDN'T MATTER IF THEY EXIST BECAUSE THE DATABASE CREATES THEM
                                    throw new InvalidDataException($"The Field {m.Member.Name} Is Missing");
                            }

                        });


                    }
                    return new List<T>() { token.ToObject<T>(serializer) };
                }

            }
            if (string.IsNullOrEmpty(json)) return new List<T>() { };
            try
            {
                var token = JToken.Parse(json);
                return token is JArray ? token.ToObject<List<T>>(serializer) : new List<T>() { token.ToObject<T>(serializer) };
            }
            catch (Exception)
            {
                return new List<T>() { };
            }
        }


        public static bool IsValidJson<T>(this string strInput)
        {
            strInput = strInput.Trim();
            if ((strInput.StartsWith("{") && strInput.EndsWith("}")) || //For object
                (strInput.StartsWith("[") && strInput.EndsWith("]"))) //For array
            {
                try
                {
                    var obj = JsonConvert.DeserializeObject<T>(strInput);
                    return true;
                }
                catch // not valid
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        public static bool IsValidJson(this string strInput, Type type)
        {
            strInput = strInput.Trim();
            if ((strInput.StartsWith("{") && strInput.EndsWith("}")) || //For object
                (strInput.StartsWith("[") && strInput.EndsWith("]"))) //For array
            {
                try
                {
                    var obj = JsonConvert.DeserializeObject(strInput, type);
                    return true;
                }
                catch // not valid
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public static JTokenType GetJsonType(this string strInput)
        {
            dynamic jobject = JsonConvert.DeserializeObject(strInput);
            return jobject.y.x.Type;
        }

    }
}
