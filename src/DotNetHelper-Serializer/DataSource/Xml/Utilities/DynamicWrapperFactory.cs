using System;
using System.Reflection;
using System.Reflection.Emit;
using DotNetHelper_Serializer.Extension;

namespace DotNetHelper_Serializer.DataSource.Xml.Utilities
{
    internal static class DynamicWrapperFactory
    {
        private static readonly Type CreaterType = typeof(Func<object>);
        private static readonly Type GetterType = typeof(Func<object, object>);
        private static readonly Type SetterType = typeof(Action<object, object>);
        private static readonly Module Module = typeof(DynamicWrapperFactory).Module;

        public static Func<object> CreateConstructor(Type valueType)
        {

            return TypeExtension.CreateDefaultConstructor(valueType);
#pragma warning disable 162
            var dynamicMethod = new DynamicMethod(
                "Create" + valueType.FullName,
                typeof(object),
                Type.EmptyTypes,
                Module)
            {
                InitLocals = true
            };

            var generator = dynamicMethod.GetILGenerator();

            if (valueType.IsValueType)
            {
                generator.DeclareLocal(valueType);
                generator.Emit(OpCodes.Ldloc_0);
                generator.Emit(OpCodes.Box, valueType);
            }
            else
            {
                var constructor = valueType.GetDefaultConstructor();

                if (constructor == null)
                {
                    //  throw new ArgumentException($"Type \"{valueType}\" hasn't default constructor.");
                }
                else
                {
                    generator.Emit(OpCodes.Newobj, constructor);
                }


            }

            generator.Emit(OpCodes.Ret);

            return (Func<object>)dynamicMethod.CreateDelegate(CreaterType);
#pragma warning restore 162
        }

        public static Func<object, object> CreateGetter(PropertyInfo propertyInfo)
        {
            var ownerType = propertyInfo.DeclaringType;
            var returnType = propertyInfo.PropertyType;

            var dynamicMethod = new DynamicMethod(
                "Get" + propertyInfo.Name,
                typeof(object),
                new Type[] { typeof(object) },
                Module,
                false);

            var generator = dynamicMethod.GetILGenerator();
            var getMethod = propertyInfo.GetGetMethod();

            PushOwner(ownerType, generator, getMethod);
            EmitMethodCall(generator, getMethod);

            if (returnType.IsValueType)
            {
                generator.Emit(OpCodes.Box, returnType);
            }

            generator.Emit(OpCodes.Ret);

            return (Func<object, object>)dynamicMethod.CreateDelegate(GetterType);
        }

        public static Action<object, object> CreateSetter(PropertyInfo propertyInfo)
        {
            var ownerType = propertyInfo.DeclaringType;
            var returnType = propertyInfo.PropertyType;

            var dynamicMethod = new DynamicMethod(
                "Set" + propertyInfo.Name,
                typeof(void),
                new Type[] { typeof(object), typeof(object) },
                Module,
                false);

            var generator = dynamicMethod.GetILGenerator();
            var setMethod = propertyInfo.GetSetMethod(true);

            PushOwner(ownerType, generator, setMethod);
            generator.Emit(OpCodes.Ldarg_1);
            EmitUnboxOrCast(generator, propertyInfo.PropertyType);
            EmitMethodCall(generator, setMethod);
            generator.Emit(OpCodes.Ret);

            return (Action<object, object>)dynamicMethod.CreateDelegate(SetterType);
        }

        private static void EmitUnboxOrCast(ILGenerator il, Type type)
        {
            var opCode = type.IsValueType ? OpCodes.Unbox_Any : OpCodes.Castclass;
            il.Emit(opCode, type);
        }

        private static void EmitMethodCall(ILGenerator il, MethodInfo method)
        {
            var opCode = method.IsFinal ? OpCodes.Call : OpCodes.Callvirt;
            il.Emit(opCode, method);
        }

        private static void PushOwner(Type ownerType, ILGenerator generator, MethodInfo getMethod)
        {
            if (!getMethod.IsStatic)
            {
                generator.Emit(OpCodes.Ldarg_0);
                EmitUnboxOrCast(generator, ownerType);
            }
        }
    }
}