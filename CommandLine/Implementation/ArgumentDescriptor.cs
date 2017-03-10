// ------------------------------------------------------------------------------------------------- 
// <copyright file="ArgumentDescriptor.cs" company="Bill Hay">
// Copyright (c) William D Hay
// </copyright>
// -------------------------------------------------------------------------------------------------
namespace CommandLine.Implementation
{
    using System;

    /// <summary>
    /// Contains all the details about one command line parameter
    /// </summary>
    internal class ArgumentDescriptor : IArgumentDescriptor
    {
        /// <summary>
        /// Gets or sets the argument name (long form)
        /// </summary>
        public string Name { get; set;  }

        /// <summary>
        /// Gets or sets the argument name (short form). This is optional
        /// </summary>
        public string ShortName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the type of the field or property
        /// </summary>
        public Type MemberType { get; set; }

        /// <summary>
        /// Gets or sets help text
        /// </summary>
        public string Help { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the field or property name
        /// </summary>
        internal string MemberName { get; set; }

        /// <summary>
        /// Gets or sets the action which sets a value into the property
        /// </summary>
        internal Action<object> Set { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this argument must be defined on the command line
        /// </summary>
        internal bool Required { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this property/field is a list
        /// </summary>
        internal bool IsList { get; set; } = false;

        /// <summary>
        /// Determines if a the name matches this descriptor
        /// </summary>
        /// <param name="name">The requested name</param>
        /// <param name="stringComparison">MemberType of string comparison to do</param>
        /// <returns>True if the name is the long or short name of this descriptor</returns>
        internal bool Match(string name, StringComparison stringComparison)
        {
            bool result =
                string.Equals(this.Name, name, stringComparison) ||
                string.Equals(this.ShortName, name, stringComparison);

            if (string.IsNullOrEmpty(this.Name))
            {
                result |= string.Equals(this.MemberName, name, stringComparison);
            }

            return result;
        }
    }
}
