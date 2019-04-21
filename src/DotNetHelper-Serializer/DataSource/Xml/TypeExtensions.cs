using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DotNetHelper_Contracts.Xml
{
    internal static class TypeExtensions
    {
        private static readonly Dictionary<Type, string> ShortNames = new Dictionary<Type, string>
        {
            { Types.String, "String" },
            { Types.Byte, "Byte" },
            { Types.Int16, "Short" },
            { Types.Int32, "Int" },
            { Types.Int64, "Long" },
            { Types.Char, "Char" },
            { Types.Float, "Float" },
            { Types.Double, "Double" },
            { Types.Bool, "Bool" },
            { Types.Decimal, "Decimal" }
        };

        public static bool IsBasicType(this Type type)
        {
            return type.IsPrimitive || type == Types.String;
        }

        public static bool IsEnumerable(this Type type)
        {
            return type.GetInterfaces().Any(x => x == Types.Enumerable);
        }

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

        public static bool HasDefaultConstructor(this Type type)
        {
            return GetDefaultConstructor(type) != null;
        }



        public static bool IsNullable(this Type type)
        {
            return type.IsGenericTypeOf(Types.NullableDefinition);
        }

        public static Type GetUnderlyingNullableType(this Type type)
        {
            if (type.IsNullable())
            {
                return type.GetGenericArguments()[0];
            }

            return null;
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