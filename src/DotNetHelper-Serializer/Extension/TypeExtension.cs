using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;


namespace DotNetHelper_Serializer.Extension
{

    //public enum NullType
    //{
    //    DbNull,
    //    Null
    //
    //}

    public enum DataTypes
    {

    }

    public static class TypeExtension
    {


     //   public static IList<T> ToDummyList<T>(this T a , int count)
     //   {
     //      return  Builder<T>.CreateListOfSize(count).All().Build();
     //   }

        // Shamelessly stolen from StackOverflow titled:
        // Fast creation of objects instead of Activator.CreateInstance(type)
        // http://tinyurl.com/gsbjk52 (see answer 21)
        public static class New<T>
        {
            public static readonly Func<T> Instance = Creator();
          

            private static Func<T> Creator()
            {
                var t = typeof(T);
                try
                {
                    if (t == typeof(string))
                        return Expression.Lambda<Func<T>>(Expression.Constant(string.Empty)).Compile();

                    if (t.HasDefaultConstructor())
                        return Expression.Lambda<Func<T>>(Expression.New(t)).Compile();

                    // Create an instance of the SomeType class that is defined in a non system assembly 
                    // TODO :: FUTURE VERSION OF .NET STANDARD MAY SUPPORT
                    //    var oh = Activator.CreateInstanceFrom(Assembly.GetEntryAssembly().CodeBase, typeof(T).FullName);
                    // Call an instance method defined by the SomeType type using this object.
                    //      return Expression.Lambda<Func<T>>(Expression.Constant(oh.Unwrap())).Compile();

                    var c = typeof(T).GetTypeInfo().DeclaredConstructors.Single(ci => ci.GetParameters().Length == 0);
                    if (Type.EmptyTypes != null) return (Func<T>) c.Invoke(Type.EmptyTypes);

                    return Activator.CreateInstance<Func<T>>();

                    //  var fastMember = TypeAccessor.Create(typeof(T), true);
                    //  if (fastMember.CreateNewSupported) return fastMember.CreateNew() as Func<T>;
                }
                catch (Exception)
                {
               
                    
                    return () => (T) FormatterServices.GetUninitializedObject(t);
                }



                //    return () => (T)FormatterServices.GetUninitializedObject(t);

                //  Alternative Option Copy the values of the POCO passed in to a new POCO.
                //   var clonedPoco = New<T>.Instance(); // Activator.CreateInstance(type)
                //   accessor.GetMembers().ToList().ForEach(c => accessor[clonedPoco, c.Name] = accessor[defaultPoco, c.Name]);
                //   return clonedPoco;
            }
        }

        public static Func<object> CreateDefaultConstructor(Type t)
        {
            try
            {

                if (t == typeof(string))
                    return Expression.Lambda<Func<object>>(Expression.Constant(string.Empty)).Compile();

                if (t.HasDefaultConstructor())
                    return Expression.Lambda<Func<object>>(Expression.New(t)).Compile();


                // Create an instance of the SomeType class that is defined in a non system assembly 
                // TODO :: FUTURE VERSION OF .NET STANDARD MAY SUPPORT
                //    var oh = Activator.CreateInstanceFrom(Assembly.GetEntryAssembly().CodeBase, typeof(T).FullName);
                // Call an instance method defined by the SomeType type using this object.
                //      return Expression.Lambda<Func<T>>(Expression.Constant(oh.Unwrap())).Compile();

                var c = t.GetTypeInfo().DeclaredConstructors.Single(ci => ci.GetParameters().Length == 0);
                if (Type.EmptyTypes != null) return (Func<object>)c.Invoke(Type.EmptyTypes);

                return Activator.CreateInstance<Func<object>>();

                //  var fastMember = TypeAccessor.Create(typeof(T), true);
                //  if (fastMember.CreateNewSupported) return fastMember.CreateNew() as Func<T>;
            }
            catch (Exception)
            {
           
                return () => (object)FormatterServices.GetUninitializedObject(t);
            }
        }

        public static bool HasDefaultConstructor(this Type t) => t.IsValueType ||
                                                                 t.GetConstructor(Type.EmptyTypes) != null;
      


        //..........................................................................................................................................................//
        //                                                                    IProgress Extension
        //..........................................................................................................................................................//




        public static void OnProgressChange<T>(this IProgress<T> progress, Action<T> action, T value)
        {
                action?.Invoke(value);
        }


        public static bool CanHaveNullValue<T>(this T obj)
        {
            // ...
            if (!typeof(T).IsValueType) return true; // ref-type
            if (Nullable.GetUnderlyingType(typeof(T)) != null) return true; // Nullable<T>
            return false;
        }


        /// <summary>
        /// Returns Whether Type is Nullable<T> & the lowest Type level 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Tuple<bool,Type> IsNullable(this Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                var childType = Nullable.GetUnderlyingType(type);
                return new Tuple<bool, Type>(true, childType);
            }
            return new Tuple<bool, Type>(false, type);
        }


        public static bool IsCSharpClass(this Type type)
        {

            return    type != typeof(DateTime)
                   && type != typeof(DateTimeOffset)
                   && type != typeof(string)
                   && type != typeof(bool)
                   && type != typeof(double)
                   && type != typeof(char)
                   && type != typeof(decimal)
                   && type != typeof(Guid)
                   && type != typeof(short)
                   && type != typeof(int) //   || type == typeof(Int16) || type == typeof(Int32) || type == typeof(Int64)           
                   && type != typeof(float)
                   && type != typeof(long)
                   && type != typeof(byte)
                   && type != typeof(byte[])
                   && type != typeof(sbyte)
                   && type != typeof(ulong)
                   && type != typeof(ushort)
                   && type != typeof(uint)
                   ;
            

        }


        public static List<T> ToDefaultList<T>(int count)
        {
            var list = new List<T>() { };
            while (list.Count < count)
            {
                var newobject = New<T>.Instance();
                list.Add(newobject);
            }
            return list;
        }

       


        public static List<T> EnumToList<T>()
        {
            return System.Enum.GetValues(typeof(T)).Cast<T>().ToList();
        }





        public static readonly Dictionary<Type, object> CommonTypeDictionary = new Dictionary<Type, object>
        {
#pragma warning disable IDE0034 // DO NOT REMOVE THIS CODE Simplify 'default' expression - default causes default(object)
            { typeof(int), default(int) },
            { typeof(Guid), default(Guid) },
            { typeof(DateTime), default(DateTime) },
            { typeof(DateTimeOffset), default(DateTimeOffset) },
            { typeof(long), default(long) },
            { typeof(bool), default(bool) },
            { typeof(double), default(double) },
            { typeof(short), default(short) },
            { typeof(float), default(float) },
            { typeof(byte), default(byte) },
            { typeof(char), default(char) },
            { typeof(uint), default(uint) },
            { typeof(ushort), default(ushort) },
            { typeof(ulong), default(ulong) },
            { typeof(sbyte), default(sbyte) }
#pragma warning restore IDE0034 // Simplify 'default' expression
        };

        public static object GetDefaultValue(this Type type)
        {
            if (!type.GetTypeInfo().IsValueType)
            {
                return null;
            }

            // A bit of perf code to avoid calling Activator.CreateInstance for common types and
            // to avoid boxing on every call. This is about 50% faster than just calling CreateInstance
            // for all value types.
            return CommonTypeDictionary.TryGetValue(type, out var value) ? value : Activator.CreateInstance(type);
        }

        public static Type UnwrapNullableType(this Type type) => Nullable.GetUnderlyingType(type) ?? type;

        public static bool IsNullableType(this Type type)
        {
            var typeInfo = type.GetTypeInfo();

            return !typeInfo.IsValueType
                   || typeInfo.IsGenericType
                   && typeInfo.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        public static bool IsUnderlyingTypeNullable(this Type type)
        {
            return IsNullableType(UnwrapNullableType(type));
        }


        public static Type MakeNullable(this Type type)
            => type.IsNullableType()
                ? type
                : typeof(Nullable<>).MakeGenericType(type);

        public static bool IsInteger(this Type type)
        {
            type = type.UnwrapNullableType();

            return type == typeof(int)
                   || type == typeof(long)
                   || type == typeof(short)
                   || type == typeof(byte)
                   || type == typeof(uint)
                   || type == typeof(ulong)
                   || type == typeof(ushort)
                   || type == typeof(sbyte)
                   || type == typeof(char);
        }

        public static bool IsStruct(this Type type)
        {
            return type.IsValueType && !type.IsEnum;
        }

        public static bool IsInstantiable(this Type type) => IsInstantiable(type.GetTypeInfo());

        private static bool IsInstantiable(TypeInfo type)
            => !type.IsAbstract
               && !type.IsInterface
               && (!type.IsGenericType || !type.IsGenericTypeDefinition);



        public static bool IsTypeIEnumerable(this Type type)
        {
            return typeof(IEnumerable).IsAssignableFrom(type);
        }

        //public static Type GetIEnumerableRealType(this Type type)
        //{
        //    if (type.GetInterfaces().ToList().Exists(i => i.IsGenericType))
        //    {
        //        var a = type.GetInterfaces().Where(i => i.IsGenericType).ToList();
        //        if (a.Count > 1)
        //        {

        //        }
        //        else
        //        {
        //            return a.First(i => i.GetGenericTypeDefinition() == typeof(IEnumerable<>)).GetGenericArguments().First();
        //        }

        //    }
        //    else
        //    {

        //    }

        //    return type.GetGenericArguments().First();
        //}



        public static T CopyObject<T>(this T obj) where T : class, ICloneable
        {
            return obj.Clone() as T;
        }


    

    private static readonly Dictionary<Type, string> ShortNames = new Dictionary<Type, string>
        {
            {typeof(string), "String" },
            {typeof(byte), "Byte" },
            {typeof(short), "Short" },
            {typeof(int), "Int" },
            {typeof(long), "Long" },
            {typeof(char), "Char" },
            {typeof(float), "Float" },
            {typeof(double), "Double" },
            {typeof(bool), "Bool" },
            {typeof(decimal), "Decimal" }
        };

        public static bool IsBasicType(this Type type)
        {
            return type.IsPrimitive || type == typeof(string);
        }

        //public static bool IsEnumerable(this Type type)
        //{
        //    return type.GetInterfaces().Any(x => x == typeof(IEnumerable));
        //}

        public static bool IsFinalType(this Type type)
        {
            return type.IsValueType || type.IsSealed;
        }

        public static bool IsActivable(this Type type)
        {
            return !type.IsAbstract && !type.IsInterface && type.HasDefaultConstructor();
        }

        public static ConstructorInfo GetDefaultConstructor(this Type type)
        {
            var bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            return type.GetConstructor(bindingFlags, null, Type.EmptyTypes, null);
        }

        //public static bool HasDefaultConstructor(this Type type)
        //{
        //    return GetDefaultConstructor(type) != null;
        //}

        public static Type GetEnumerableItemType(this Type type)
        {
            if (type.IsArray)
            {
                return type.GetElementType();
            }

            Type elementType = null;

            if (type == typeof(IEnumerable))
            {
                elementType = typeof(object);
            }
            else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                elementType = type.GetGenericArguments()[0];
            }
            else
            {
                foreach (var interfaceType in type.GetInterfaces())
                {
                    if (interfaceType == typeof(IEnumerable))
                    {
                        elementType = typeof(object);
                    }
                    else if (interfaceType.IsGenericType &&
                        interfaceType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                    {
                        elementType = interfaceType.GetGenericArguments()[0];
                        break;
                    }
                }
            }


            return elementType;
        }

        //public static bool IsNullable(this Type type)
        //{
        //    return type.IsGenericTypeOf(typeof(Nullable<>));
        //}

        public static Type GetUnderlyingNullableType(this Type type)
        {
            if (type.IsNullable().Item1)
            {
                return type.GetGenericArguments()[0];
            }

            return type;
        }

        public static bool IsGenericTypeOf(this Type type, Type definitionType)
        {
            return type.IsGenericType
                && !type.IsGenericTypeDefinition
                && type.GetGenericTypeDefinition() == definitionType;
        }

        public static bool IsGenericTypeOf(this Type type, params Type[] definitionTypes)
        {
            if (type.IsGenericType && !type.IsGenericTypeDefinition)
            {
                var typeDefinition = type.GetGenericTypeDefinition();

                foreach (var expectedType in definitionTypes)
                {
                    if (typeDefinition == expectedType)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static string GetShortName(this Type type)
        {

            if (!ShortNames.TryGetValue(type, out string shortName))
            {
                if (type.IsArray)
                {
                    shortName = "ArrayOf" + GetShortName(type.GetElementType());
                }
                else
                {
                    shortName = type.Name;

                    if (type.IsGenericType)
                    {
                        var typeDefIndex = shortName.LastIndexOf('`');

                        if (typeDefIndex != -1)
                        {
                            shortName = shortName.Substring(0, typeDefIndex);
                        }
                    }
                }
            }

            return shortName;
        }

        public static MethodInfo GetStaticMethod(this Type type, string methodName, params Type[] parameters)
        {
            var bindingFlags = BindingFlags.Static | BindingFlags.Public;
            return type.GetMethod(methodName, bindingFlags, null, parameters ?? Type.EmptyTypes, null);
        }

        public static MethodInfo GetInstanceMethod(this Type type, string methodName, params Type[] parameters)
        {
            var bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            return type.GetMethod(methodName, bindingFlags, null, parameters ?? Type.EmptyTypes, null);
        }


    }
}