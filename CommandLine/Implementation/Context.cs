// ------------------------------------------------------------------------------------------------- 
// <copyright file="Context.cs" company="Bill Hay">
// Copyright (c) William D Hay
// </copyright>
// -------------------------------------------------------------------------------------------------
namespace CommandLine.Implementation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Encapsulates customizable behavior - e.g. if match is to be case sensitive or case insensitive etc. 
    /// </summary>
    /// <typeparam name="T">The type of the application command line argument class</typeparam>
    internal class Context<T> where T : new()
    {
        // results passed back through the context

        /// <summary>
        /// Gets the dictionary of key/value pairs
        /// </summary>
        internal T CommandLineSwitches { get; private set; } = new T();

        /// <summary>
        /// Gets the list of non switch/value arguments
        /// </summary>
        internal List<string> CommandLineNonSwitches { get; private set; } = new List<string>();

        /// <summary>
        /// Gets the exception list which accumulates exceptions through the parsing pipeline
        /// </summary>
        internal List<Exception> Exceptions { get; } = new List<Exception>();

        /// <summary>
        /// Gets or sets the command line settings
        /// </summary>
        internal ICommandLineSettings Settings { get; set; }

        /// <summary>
        /// Gets or sets the list of descriptors
        /// </summary>
        internal IReadOnlyList<ArgumentDescriptor> ArgumentDescriptors { get; set; }

        /// <summary>
        /// Finds a named parameter descriptor
        /// </summary>
        /// <param name="name">The name of the parameter</param>
        /// <returns>The descriptor <see cref="ArgumentDescriptor"/></returns>
        internal ArgumentDescriptor GetDescriptor(string name)
        {
            return this.ArgumentDescriptors.FirstOrDefault(x => x.Match(name, this.Settings.CaseMatching));
        }
    }
}
