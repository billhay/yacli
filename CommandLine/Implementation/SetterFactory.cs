// ------------------------------------------------------------------------------------------------- 
// <copyright file="SetterFactory.cs" company="Bill Hay">
// Copyright (c) William D Hay
// </copyright>
// -------------------------------------------------------------------------------------------------
namespace CommandLine.Implementation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// This class generates a setter which will either set a value into 
    /// a scalar field or property, or if the object is a list, will optionally
    /// create the typed list, and will then add the value to the list. 
    /// </summary>
    internal class SetterFactory
    {
        /// <summary>
        ///  returns the content of the field/property
        /// </summary>
        private readonly Func<object, object> get;

        /// <summary>
        /// The type of the field/property being set
        /// </summary>
        private readonly Type methodType;

        /// <summary>
        /// adds the value to a list
        /// </summary>
        private readonly Action<object, object> setIntoList;

        /// <summary>
        /// sets the value into the field/property
        /// </summary>
        private readonly Action<object, object> setIntoScalar;

        /// <summary>
        /// Initializes a new instance of the <see cref="SetterFactory"/> class
        /// </summary>
        /// <param name="pinfo">The PropertyInfo which is going to be set.</param>
        internal SetterFactory(PropertyInfo pinfo)
        {
            this.methodType = pinfo.PropertyType;
            this.get = x => pinfo.GetMethod.Invoke(x, null);
            this.setIntoList = (obj, list) => pinfo.SetMethod.Invoke(obj, new[] { list });
            this.setIntoScalar = pinfo.SetValue;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SetterFactory"/> class
        /// </summary>
        /// <param name="finfo">The PropertyInfo which is going to be set.</param>
        internal SetterFactory(FieldInfo finfo)
        {
            this.methodType = finfo.FieldType;
            this.get = finfo.GetValue;
            this.setIntoList = finfo.SetValue;
            this.setIntoScalar = finfo.SetValue;
        }

        /// <summary>
        /// gets a setter to set a property in a class to a value given a string representation of the value
        /// </summary>
        /// <param name="info">The <see cref="PropertyInfo"> of the property being set</see></param>
        /// <param name="argumentClass">The class instance that the property will be set in</param>
        /// <returns>A setter (Action) which given a string will set the property to the parsed value of the string</returns>
        internal static Action<object> GetSetter(PropertyInfo info, object argumentClass)
        {
            SetterFactory sf = new SetterFactory(info);
            var setter2 = sf.GetSetter();
            return x => setter2(argumentClass, (string)x);
        }

        /// <summary>
        /// gets a setter to set a field in a class to a value given a string representation of the value
        /// </summary>
        /// <param name="info">The <see cref="FieldInfo"> of the field being set</see></param>
        /// <param name="argumentClass">The class instance that the field will be set in</param>
        /// <returns>A setter (Action) which given a string will set the field to the parsed value of the string</returns>
        internal static Action<object> GetSetter(FieldInfo info, object argumentClass)
        {
            SetterFactory sf = new SetterFactory(info);
            var setter2 = sf.GetSetter();
            return x => setter2(argumentClass, (string)x);
        }

        /// <summary>
        /// Builds an Action which will set the value of 
        /// field or property. If the field or property is a List{}
        /// if will check if the field/property has been initialized, if not
        /// it will initialize it. Then it adds the value to the list.
        /// </summary>
        /// <returns>The action object</returns>
        internal Action<object, string> GetSetter()
        {
            Type targetType = SetterFactory.GetListType(this.methodType);
            if (targetType == null)
            {
                Action<object, string> setter = (obj, v) =>
                {
                    object value = ObjectCreator.Create(this.methodType, v);
                    this.setIntoScalar(obj, value);
                };

                return setter;
            }
            else
            {
                // this is a list, and it may be null, so get the list
                Action<object, string> setter = (obj, v) =>
                {
                    object value = ObjectCreator.Create(targetType, v);
                    object list = this.get(obj);
                    if (list == null)
                    {
                        ConstructorInfo constructor = this.methodType.GetConstructor(new Type[] { });
                        if (constructor != null)
                        {
                            list = constructor.Invoke(new object[] { });
                            this.setIntoList(obj, list);
                        }
                    }

                    if (list != null)
                    {
                        MethodInfo addMethod = list.GetType().GetMethod("Add");
                        addMethod.Invoke(list, new[] { value });
                    }
                };

                return setter;
            }
        }

        /// <summary>
        /// Returns the type of the list, or null if this is not a list
        /// </summary>
        /// <param name="t">The list type</param>
        /// <returns>The type of the objects in the list</returns>
        private static Type GetListType(Type t)
        {
            return
                t.GetInterfaces()
                    .Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IList<>))
                    .Select(x => x.GetGenericArguments()[0])
                    .FirstOrDefault();
        }
    }
}