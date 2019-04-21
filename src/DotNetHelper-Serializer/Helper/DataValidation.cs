using System;
using System.Collections.Generic;
using DotNetHelper_Serializer.Extension;

namespace DotNetHelper_Serializer.Helper
{
    public static class DataValidation
    {


        public static Tuple<bool, List<string>> IsValidBasedOnSqlColumnAttributes(AdvanceMember member)// where T : Attribute
        {
            var tuple = new Tuple<bool, List<string>>(true, new List<string>() { });
            if (member.SqlCustomAttritube.Ignore == true)
            {
                return tuple;
            }
            if (member.SqlCustomAttritube.Nullable == false)
            {
                if (member.Value == null)
                {
                    tuple.Item2.Add($"The field {member.Member.Name} is mark as a non-nullable field therefore it's required & it can't be null");
                }

            }
            if (member.SqlCustomAttritube.PrimaryKey == true)
            {
                if (member.Value == null)
                {
                    tuple.Item2.Add($"The field {member.Member.Name} is a primary key therefore it's required & it can't be null");
                }
                else
                {
                    if (member.Member.Type == typeof(DateTime) && (DateTime)member.Value == DateTime.MinValue)
                    {
                        tuple.Item2.Add($"The field {member.Member.Name} is a primary key therefore it's required & it can't be null");
                    }
                }
            }
            if (member.SqlCustomAttritube.MaxColumnSize.GetValueOrDefault(0) > 0)
            {
                var valueSize = 0;
                if (member.Value != null) valueSize = member.Value.ToString().Length;
                if (valueSize > member.SqlCustomAttritube.MaxColumnSize.GetValueOrDefault(0))
                {
                    tuple.Item2.Add($"The field {member.Member.Name} exceeds the maximum amount of characters ({member.SqlCustomAttritube.MaxColumnSize.GetValueOrDefault(0)})");
                }
            }



            return new Tuple<bool, List<string>>(tuple.Item2.Count <= 0, tuple.Item2);
        }


    }
}
