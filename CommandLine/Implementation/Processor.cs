// ------------------------------------------------------------------------------------------------- 
// <copyright file="Processor.cs" company="Bill Hay">
// Copyright (c) William D Hay
// </copyright>
// -------------------------------------------------------------------------------------------------
namespace CommandLine.Implementation
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using CommandLine;

    /// <summary>
    /// The main command line parsing class
    /// </summary>
    /// <typeparam name="T">The type of the application command line argument class</typeparam>
    internal class Processor<T> where T : new()
    {
        /// <summary>
        /// The list of descriptors for the application setting class. For each field in the class there is a name, an optional short name, and a setter
        /// to set a value into the field
        /// </summary>
        private readonly List<ArgumentDescriptor> descriptors = new List<ArgumentDescriptor>();

        /// <summary>
        /// the context
        /// </summary>
        private readonly Context<T> context;

        /// <summary>
        /// the application setting class instance
        /// </summary>
        private readonly object applicationArgumentClass;

        /// <summary>
        /// Initializes a new instance of the <see cref="Processor{T}"/> class
        /// </summary>
        /// <param name="context">The context for this parse</param>
        private Processor(Context<T> context)
        {
            this.context = context;
            this.applicationArgumentClass = context.CommandLineSwitches;
            this.context.ArgumentDescriptors = this.descriptors;
        }

        /// <summary>
        /// The primary parsing method - given a <c>string[] args</c> it returns the populated application setting object
        /// </summary>
        /// <param name="args">The command line arguments</param>
        /// <param name="start">The index in args to start parsing at - typically 0 or 1</param>
        /// <param name="settings">The command line settings</param>
        /// <returns>A populated instance of the application command line settings class</returns>
        internal static Context<T> Parse(string[] args, int start, ICommandLineSettings settings)
        {
            Context<T> context = new Context<T> { Settings = settings };

            Processor<T> processor = new Processor<T>(context);
            processor.Parse(args, start);
            if (context.Exceptions.Count > 0)
            {
                throw new AggregateException("one or more exceptions parsing command line", context.Exceptions);
            }

            return context;
        }

        /// <summary>
        /// Returns the arguments descriptors from the application settings class
        /// </summary>
        /// <param name="settings">The command line settings</param>
        /// <returns>An enumerator of all the application settings</returns>
        internal static IEnumerable<ArgumentDescriptor> GetArgumentDescriptors(ICommandLineSettings settings) 
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings), "Argument cannot be null");
            }

            Context<T> context = new Context<T> { Settings = settings };
            Processor<T> processor = new Processor<T>(context);
            processor.BuildDescriptors();
            var descriptors = processor.descriptors;

            descriptors.Sort((x, y) => string.Compare(Processor<T>.SortName(x), Processor<T>.SortName(y), StringComparison.CurrentCulture));
            return descriptors;
        }

        /// <summary>
        /// We want required arguments ahead of optional, otherwise alphabetically ordered. This just generates a 
        /// pseudo-name to ensure this happens.
        /// </summary>
        /// <param name="argumentDescriptor">The argument descriptor</param>
        /// <returns>a pseudo name which guarantees the sort order we want</returns>
        private static string SortName(ArgumentDescriptor argumentDescriptor)
        {
            if (argumentDescriptor.Required)
            {
                return "0" + argumentDescriptor.Name;
            }
            else
            {
                return "1" + argumentDescriptor.Name;
            }
        }

        /// <summary>
        /// The instance member which does the parsing
        /// </summary>
        /// <param name="args">The command line arguments</param>
        /// <param name="start">Where to start parsing in args[]</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "In this case we are going to rethrow the exception as an aggregate exception")]
        private void Parse(string[] args, int start)
        {
            this.BuildDescriptors();
            Parser<T> parser = new Parser<T>(args, start, this.context);
            IReadOnlyDictionary<string, List<string>> dict = parser.Parse();
            var exceptions = this.context.Exceptions;

            foreach (KeyValuePair<string, List<string>> param in dict)
            {
                try
                {
                    if (!this.Set(param.Key, param.Value))
                    {
                        exceptions.Add(new CommandLineException(param.Key, param.Value[0], "No matching parameter in application settings class"));
                    }

                    ArgumentDescriptor descriptor = this.context.GetDescriptor(param.Key);
                    if (descriptor != null)
                    {
                        descriptor.Required = false;
                    }
                }
                catch (TargetInvocationException ex)
                {
                    ArgumentDescriptor argumentDescriptor = this.GetArgumentDescriptor(param.Key);
                    Type paramType = argumentDescriptor.MemberType;

                    exceptions.Add(new CommandLineException(param.Key, param.Value[0], paramType, ex.InnerException));
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }

            // iterate over all descriptors to make sure all required parameters have been set
            exceptions.AddRange(
                this
                    .context.ArgumentDescriptors
                    .Where(x => x.Required)
                    .Select(descriptor => new CommandLineException(descriptor.Name, string.Empty, "Required argument not set")));
        }

        /// <summary>
        /// Sets a value into the application settings object
        /// </summary>
        /// <param name="key">The name of the parameter - maps to a field or property</param>
        /// <param name="valueList">The value to insert</param>
        /// <returns>true if the value can be set</returns>
        private bool Set(string key, List<string> valueList)
        {
            bool set = false;

            foreach (ArgumentDescriptor item in this.descriptors.FindAll(x => x.Match(key, this.context.Settings.CaseMatching)))
            {
                foreach (string v in valueList)
                {
                    string value = v;
                    if (item.MemberType == typeof(bool))
                    {
                        if (string.IsNullOrEmpty(value))
                        {
                            value = "true";
                        }
                    }

                    item.Set(value);
                    set = true;
                }
            }

            return set;
        }

        /// <summary>
        /// Get the descriptor for a command line parameter
        /// </summary>
        /// <param name="name">The parameter name to match from the command line</param>
        /// <returns>The descriptor or null</returns>
        private ArgumentDescriptor GetArgumentDescriptor(string name)
        {
            return this.descriptors.FirstOrDefault(x => x.Match(name, this.context.Settings.CaseMatching));
        }

        /// <summary>
        /// Initializes the descriptors by scanning the application settings class
        /// </summary>
        private void BuildDescriptors()
        {
            TypeInfo ti = this.applicationArgumentClass.GetType().GetTypeInfo();

            foreach (PropertyInfo property in ti.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                try
                { 
                    Action<object> action = SetterFactory.GetSetter(property, this.applicationArgumentClass);
                    bool found = false;
                    foreach (CustomAttributeData attributeData in property.CustomAttributes.Where(
                            x => x.AttributeType.FullName == typeof(ArgumentAttribute).FullName))
                    {
                        this.AddDescriptor(attributeData, action, property.Name, property.PropertyType);
                        found = true;
                    }

                    if (!found)
                    {
                        this.AddDescriptor(null, action, property.Name, property.PropertyType);
                    }
                }
                catch (Exception ex)
                {
                    this.context.Exceptions.Add(ex);
                }
            }

            foreach (FieldInfo field in ti.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                // skip property backing fields
                if (field.Name.EndsWith("k__BackingField", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                try
                {
                    Action<object> action = SetterFactory.GetSetter(field, this.applicationArgumentClass);
                    bool found = false;
                    foreach (CustomAttributeData attributeData in
                        field.CustomAttributes.Where(
                            x => x.AttributeType.FullName == typeof(ArgumentAttribute).FullName))
                    {
                        this.AddDescriptor(attributeData, action, field.Name, field.FieldType);
                        found = true;
                    }

                    if (!found)
                    {
                        this.AddDescriptor(null, action, field.Name, field.FieldType);
                    }
                }
                catch (Exception ex)
                {
                    this.context.Exceptions.Add(ex);
                }
            }
        }

        /// <summary>
        /// Adds a descriptor to the list of descriptors
        /// </summary>
        /// <param name="attributeData">The attribute data for the field or property</param>
        /// <param name="action">The action which sets the value</param>
        /// <param name="fieldName">The name of the field or property</param>
        /// <param name="targetType">The type of the field or property</param>
        private void AddDescriptor(CustomAttributeData attributeData, Action<object> action, string fieldName, Type targetType)
        {
            ArgumentDescriptor ar = new ArgumentDescriptor
            {
                Set = action,
                MemberType = targetType
            };

            if (!this.context.Settings.RequireAttributes)
            {
                ar.MemberName = fieldName;
            }

            if (attributeData != null)
            {
                Debug.Assert(attributeData.ConstructorArguments.Count == 1, "Attribute constructor has incorrect number of parameters");

                foreach (CustomAttributeTypedArgument constructorParameterName in attributeData.ConstructorArguments)
                {
                    Debug.Assert(constructorParameterName.ArgumentType == typeof(string), "Attribute constructor parameter must be a string");
                    ar.Name = (string)constructorParameterName.Value;
                    if (string.IsNullOrWhiteSpace(ar.Name))
                    {
                        ar.IsHidden = true;
                    }

                    ar.MemberType = targetType;
                }

                if (attributeData.NamedArguments != null)
                {
                    foreach (CustomAttributeNamedArgument optionalParameter in attributeData.NamedArguments)
                    {
                        if (optionalParameter.MemberName == "ShortName")
                        {
                            Debug.Assert(optionalParameter.TypedValue.ArgumentType == typeof(string), "Optional attribute parameter ShortName must be a string");
                            ar.ShortName = (string)optionalParameter.TypedValue.Value;
                        }

                        if (optionalParameter.MemberName == "Required")
                        {
                            Debug.Assert(optionalParameter.TypedValue.ArgumentType == typeof(bool), "Optional attribute parameter Required must be a bool");
                            ar.Required = (bool)optionalParameter.TypedValue.Value;
                        }

                        if (optionalParameter.MemberName == "HelpText")
                        {
                            Debug.Assert(optionalParameter.TypedValue.ArgumentType == typeof(string), "Optional attribute parameter Help must be a string");
                            ar.Help = (string)optionalParameter.TypedValue.Value;
                        }
                    }
                }
            }

            if (string.IsNullOrEmpty(ar.Name))
            {
                ar.Name = ar.MemberName;
            }

            if (
                this.context.Settings.CaseMatching == StringComparison.CurrentCultureIgnoreCase ||
                this.context.Settings.CaseMatching == StringComparison.InvariantCultureIgnoreCase ||
                this.context.Settings.CaseMatching == StringComparison.OrdinalIgnoreCase)
            {
                ar.Name = ar.Name.ToLower(CultureInfo.CurrentCulture);
                if (ar.ShortName != null)
                {
                    ar.ShortName = ar.ShortName.ToLower(CultureInfo.CurrentCulture);
                }
            }

            ar.IsList = 
                targetType.GetInterfaces()
                    .FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IList<>)) != null;

            this.descriptors.Add(ar);
        }
    }
}