// ------------------------------------------------------------------------------------------------- 
// <copyright file="ObjectCreator.cs" company="Bill Hay">
// Copyright (c) William D Hay
// </copyright>
// -------------------------------------------------------------------------------------------------
namespace CommandLine.Implementation
{
    using System;
    using System.Reflection;

    /// <summary>
    /// Class to create objects given the object type and a a value as a string
    /// </summary>
    internal static class ObjectCreator
    {
        /// <summary>
        /// Generic version of Create - attempts to create an object of type T from string
        /// </summary>
        /// <typeparam name="T">The type of the object to be created</typeparam>
        /// <param name="parameter">The string parameter</param>
        /// <returns>An object of type T</returns>
        internal static T Create<T>(string parameter)
        {
            return (T)ObjectCreator.Create(typeof(T), parameter);
        }

        /// <summary>
        /// Creates an object given the type and a string representation
        /// It expects there to be either a constructor that takes a string,
        /// or a Parse method.
        /// </summary>
        /// <param name="paramType">The type of the object</param>
        /// <param name="parameter">the string parameter</param>
        /// <returns>The object to be created</returns>
        internal static object Create(Type paramType, string parameter)
        {
            if (paramType.IsEnum)
            {
                object value = Enum.Parse(paramType, parameter, ignoreCase: true);
                return value;
            }

            ConstructorInfo constructor = paramType.GetConstructor(new[] { typeof(string) });
            if (constructor != null)
            {
                return constructor.Invoke(new[] { (object)parameter });
            }

            MethodInfo parseMethod = paramType.GetMethod("Parse", new[] { typeof(string) });
            if (parseMethod != null)
            {
                return parseMethod.Invoke(null, new[] { (object)parameter });
            }

            if (paramType == typeof(string))
            {
                return parameter;
            }

            return default(object);
        }

        /// <summary>
        /// Assigns a value to a property or field in an object
        /// </summary>
        /// <param name="target">The object whose property/field is being updated</param>
        /// <param name="member">The property or field to be updated</param>
        /// <param name="commandLineValue">The value as a string</param>
        internal static void Assign(object target, MemberInfo member, string commandLineValue)
        {
            PropertyInfo prop = member as PropertyInfo;
            if (prop != null)
            {
                object value = ObjectCreator.Create(prop.PropertyType, commandLineValue);
                prop.SetValue(target, value);
                return;
            }

            FieldInfo field = member as FieldInfo;
            if (field != null)
            {
                object value = ObjectCreator.Create(field.FieldType, commandLineValue);
                field.SetValue(target, value);
                return;
            }
        }
    }
}
