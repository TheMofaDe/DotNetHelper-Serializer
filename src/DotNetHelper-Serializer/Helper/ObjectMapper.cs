using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using DotNetHelper_Contracts.Comparer;
using DotNetHelper_Contracts.Extension;
using DotNetHelper_Contracts.Helpers;
using DotNetHelper_Serializer.Extension;
using FastMember;

namespace DotNetHelper_Serializer.Helper
{
    public static class ObjectMapper
    {

    //    public static ILogger Logger { get; set; }

        private static Tuple<Dictionary<Member, Member>, TypeAccessor,TypeAccessor> GetMatchingMembers<T1,T2>(bool exactTypeOnly = false, StringComparison comparer = StringComparison.CurrentCulture, IDictionary<Type, IFormatProvider> formatProviders = null)
        {
            var accessor1 = TypeAccessor.Create(typeof(T1), true);
            var accessor2 = TypeAccessor.Create(typeof(T2), true);

            var members1 = accessor1.GetMembers().ToList();
            var members2 = accessor2.GetMembers().ToList();

            Dictionary<Member, Member> propertyMapping  = new Dictionary<Member, Member>();
            members2.ForEach(delegate(Member m2)
            {
                var master = members1.FirstOrDefault(m1 => string.Equals(m2.Name, m1.Name,comparer));
                if (string.IsNullOrEmpty(master?.Name)) return;
                if (exactTypeOnly)
                {
                    if (master.Type == m2.Type) 
                    propertyMapping.Add(master, m2);
                }
                else
                {
                    propertyMapping.Add(master, m2);
                }
              
            });
            //var list  = exactTypeOnly ? members2.Where(m => members1.Exists(n => n.Name == m.Name && string.Equals(n.Name, m.Name, comparer) && n.Type == m.Type)).ToList()
            //    : members2.Where(m => members1.Exists(n => n.Name == m.Name && string.Equals(n.Name, m.Name, comparer))).ToList();
          
            return new Tuple<Dictionary<Member, Member>, TypeAccessor, TypeAccessor>(propertyMapping, accessor1,accessor2);
        }




        private static object GetValue<T1>(KeyValuePair<Member, Member> pair, TypeAccessor accessor1, T1 original, IDictionary<Type, IFormatProvider> beforeMappinFormatProviders = null)
        {

            if (!beforeMappinFormatProviders.IsNullOrEmpty())
            {
#if NETFRAMEWORK
                return Convert.ChangeType(accessor1[original, pair.Key.Name], pair.Value.Type, beforeMappinFormatProviders.GetValueOrDefault(pair.Key.Type));
#else
                return Convert.ChangeType(accessor1[original, pair.Key.Name], pair.Value.Type, beforeMappinFormatProviders.GetValueOrDefaultValue(pair.Key.Type));
#endif
            }
            else
            {
                if (!pair.Value.Type.IsUnderlyingTypeNullable() && pair.Value.Type.IsNullableType() ) // System.Convert Dont Handle Nullable<T> see link for reference https://stackoverflow.com/questions/3531318/convert-changetype-fails-on-nullable-types
                {
                    return accessor1[original, pair.Key.Name];
                }
                else
                {
                    return Convert.ChangeType(accessor1[original, pair.Key.Name], pair.Value.Type, null);
                }
            }
        }

        public static T2 MapProperties<T1, T2>(T1 original, T2 copyCat, bool exactTypeOnly = false, StringComparison comparer = StringComparison.CurrentCulture,IDictionary<Type,IFormatProvider> beforeMappinFormatProviders = null)
        {

            var tuple = GetMatchingMembers<T1, T2>(exactTypeOnly, comparer);
            var sameKids = tuple.Item1;
            var accessor1 = tuple.Item2;
            var accessor2 = tuple.Item3;
            sameKids.ForEach(delegate(KeyValuePair<Member, Member> pair) 
            {
                accessor2[copyCat, pair.Value.Name] = GetValue(pair, accessor1, original, beforeMappinFormatProviders);
            });

            return copyCat;
        }
        public static T2 MapPropertiesDontThrow<T1, T2>(T1 original, T2 copyCat, bool exactTypeOnly = false, StringComparison comparer = StringComparison.CurrentCulture, IDictionary<Type, IFormatProvider> beforeMappinFormatProviders = null)
        {

            var tuple = GetMatchingMembers<T1, T2>(exactTypeOnly, comparer);
            var sameKids = tuple.Item1;
            var accessor1 = tuple.Item2;
            var accessor2 = tuple.Item3;
            sameKids.ForEach(delegate (KeyValuePair<Member, Member> pair)
            {
                try
                {
                    accessor2[copyCat, pair.Value.Name] = GetValue(pair, accessor1, original, beforeMappinFormatProviders);
                }
                catch (Exception)
                {
                  
      //              Logger?.LogError(new Exception($"Failed To Map Property {pair.Key.Name} : {pair.Key.Type.FullName} --> {pair.Value.Name} : {pair.Value.Type.FullName}",error));
                }
            });

            return copyCat;
        }


        public static T2 MapExcept<T1, T2>(T1 original, T2 copyCat, Expression<Func<T1, object>> excludeProperties = null, bool exactTypeOnly = false, StringComparison comparer = StringComparison.CurrentCulture, IDictionary<Type, IFormatProvider> beforeMappinFormatProviders = null)
        {
            var tuple = GetMatchingMembers<T1, T2>(exactTypeOnly, comparer);
            var sameKids = tuple.Item1;
            var list = excludeProperties.GetPropertyNamesFromExpression();
            var temp = sameKids.ToList();
            temp.RemoveAll(m => list.Contains(m.Value.Name, new EqualityComparerString(comparer)));
            var accessor1 = tuple.Item2;
            var accessor2 = tuple.Item3;
            temp.ForEach(delegate (KeyValuePair<Member, Member> pair)
            {
                accessor2[copyCat, pair.Value.Name] = GetValue(pair, accessor1, original, beforeMappinFormatProviders);
            });

            return copyCat;
        }

        public static T2 MapExceptDontThrow<T1, T2>(T1 original, T2 copyCat, Expression<Func<T1, object>> excludeProperties = null, bool exactTypeOnly = false, StringComparison comparer = StringComparison.CurrentCulture, IDictionary<Type, IFormatProvider> beforeMappinFormatProviders = null)
        {
            var tuple = GetMatchingMembers<T1, T2>(exactTypeOnly, comparer);
            var sameKids = tuple.Item1;
            var list = excludeProperties.GetPropertyNamesFromExpression();
            var temp = sameKids.ToList();
            temp.RemoveAll(m => list.Contains(m.Value.Name, new EqualityComparerString(comparer)));
            var accessor1 = tuple.Item2;
            var accessor2 = tuple.Item3;
            temp.ForEach(delegate (KeyValuePair<Member, Member> pair)
            {
                try {
                    accessor2[copyCat, pair.Value.Name] = GetValue(pair, accessor1, original, beforeMappinFormatProviders);
                }
                catch (Exception)
            {

     //           Logger?.LogError(new Exception($"Failed To Map Property {pair.Key.Name} : {pair.Key.Type.FullName} --> {pair.Value.Name} : {pair.Value.Type.FullName}", error));
            }
        });

            return copyCat;
        }

        public static T2 MapOnly<T1, T2>(T1 original, T2 copyCat, Expression<Func<T1, object>> includeProperties = null, bool exactTypeOnly = false, StringComparison comparer = StringComparison.CurrentCulture, IDictionary<Type, IFormatProvider> beforeMappinFormatProviders = null)
        {
            var tuple = GetMatchingMembers<T1, T2>();
            var list = includeProperties.GetPropertyNamesFromExpression();
            var sameKids = tuple.Item1.Where(m => list.Contains(m.Value.Name, new EqualityComparerString(comparer))).ToList();
            var temp = sameKids.ToList();
            temp.RemoveAll(m => list.Contains(m.Value.Name, new EqualityComparerString(comparer)));
            var accessor1 = tuple.Item2;
            var accessor2 = tuple.Item3;
            temp.ForEach(delegate (KeyValuePair<Member, Member> pair)
            {
                accessor2[copyCat, pair.Value.Name] = GetValue(pair, accessor1, original, beforeMappinFormatProviders);
            });

            return copyCat;
        }



        //public static bool DoesPropertiesMatchExact<T>(List<string> propertyToSearch, T obj1, T Tobj2)
        //{
            
        //}

        public static bool DoesAllPropertiesMatchExact(List<string> propertyToSearch, IEnumerable<AdvanceMember> members1, IEnumerable<AdvanceMember> members2)
        {
            propertyToSearch.IsNullThrow(nameof(propertyToSearch));
            members1.IsNullThrow(nameof(members1));
            members2.IsNullThrow(nameof(members2));
            var passTest = true;
            propertyToSearch.ForEach(delegate(string s)
            {
                if (!passTest) return;
                var value1 = members1.FirstOrDefault(m => m.Member.Name == s);
                var value2 = members2.FirstOrDefault(m => m.Member.Name == s);
                if (value1?.Value == null && value2?.Value == null) return;
                if (!value1.Value.Equals(value2.Value))
                {
                    passTest = false;
                }
            });
            return passTest;
        }


        public static bool IsSqlColumnsKeysMatchingValues<T>(T poco1, T poco2) where T : class
        {
            var k1 = ExtFastMember.GetAdvanceMembers(poco1);
            var k2 = ExtFastMember.GetAdvanceMembers(poco2);
            var keys1 = k1.Where(member => member.SqlCustomAttritube.PrimaryKey == true);
            var keys2 = k2.Where(member => member.SqlCustomAttritube.PrimaryKey == true);
            var i = 0;
            var passTest = true;
            keys1.ToList().ForEach(delegate (AdvanceMember s) // KEY MUST MATCH IN ORDER FOR RECORD TO EXIST 
            {
                if (!passTest) return;
                var tempAdvance = keys2.ToList()[i];
                
                if (s.Value.Equals(tempAdvance.Value)) 
                {
                   // if(k1.First(a => a.Member.Name == "Name").Value  == )
                }
                else
                {
                    //var test = s.Value.Equals(tempAdvance.Value);
                    passTest = false;
                }
                i++;
            });
            return passTest;
        }

    }
}
