// ------------------------------------------------------------------------------------------------- 
// <copyright file="ArgumentAttribute.cs" company="Bill Hay">
// Copyright (c) William D Hay
// </copyright>
// -------------------------------------------------------------------------------------------------
namespace CommandLine
{
    using System;

    /// <summary>
    /// Command line attribute on a field or property for command line parsing
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class ArgumentAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the  <see cref="ArgumentAttribute"/> class
        /// </summary>
        public ArgumentAttribute()
            : this(string.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the  <see cref="ArgumentAttribute"/> class
        /// </summary>
        /// <param name="name">
        /// The name of the command line parameter i.e. <c>-file fred.txt </c> the name would be <c>file</c>
        /// </param>
        public ArgumentAttribute(string name)
        {
            this.Name = name;
        }

        /// <summary>
        /// Gets the long name of the command line parameter
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets or sets an optional short name, for example <c>-inputfile</c> -> <c>-if</c>
        /// </summary>
        public string ShortName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the command line argument has to be present
        /// </summary>
        public bool Required { get; set; } = false;

        /// <summary>
        /// Gets or sets a string explaining this argument
        /// </summary>
        public string HelpText { get; set; }
    }
}
