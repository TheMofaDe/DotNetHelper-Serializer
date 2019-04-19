using System;
using System.Collections.Specialized;
using System.Linq;
using DotNetHelper_Contracts.Extension;
using FastMember;
namespace DotNetHelper_Serializer.Extension
{
    public static class ExtDictonary
    {
  




        public static T DictionaryToObject<T>(this IOrderedDictionary dict)
        {
            var accessor = TypeAccessor.Create(typeof(T));
            var t = accessor.CreateNew();
            var props = accessor.GetMembers();
            foreach (var key in dict.Keys)
            {
                if (props.Select(a => a.Name).ToList().Contains(key))
                {
                    var p = props.First(b => string.Equals(b.Name, key.ToString(), StringComparison.CurrentCultureIgnoreCase));
                    var type = p.Type;
                    if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        type = Nullable.GetUnderlyingType(type);
                    }
                    var value = dict.GetValue<object, object>(key);

                    if (type != null && value != null) value = Convert.ChangeType(value, type, null);
                    accessor[t, key.ToString()] = value;
                }

            }
            return (T)t;
        }


        

       

       
    }
}
