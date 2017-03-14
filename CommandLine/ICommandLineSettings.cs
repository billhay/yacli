// ------------------------------------------------------------------------------------------------- 
// <copyright file="ICommandLineSettings.cs" company="Bill Hay">
// Copyright (c) William D Hay
// </copyright>
// -------------------------------------------------------------------------------------------------
namespace CommandLine
{
    using System;

    /// <summary>
    /// The interface to allow the application to set parameters for things like prefix characters
    /// </summary>
    public interface ICommandLineSettings
    {
        /// <summary>
        /// Gets or sets a value indicating if parameter matching is to be case (in)sensitive
        /// </summary>
        StringComparison CaseMatching { get; set; }

        /// <summary>
        /// Gets or sets the set of possible switch prefixes
        /// </summary>
        /// <remarks>in setting this, make sure "--" comes before "-" in the list</remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "OK to do so here")]
        string[] SwitchPrefixes { get; set; }

        /// <summary>
        /// Gets or sets the set of allowed separator characters between the switch and its value
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "OK to do so here")]
        string[] SwitchSeparators { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the argument start with 'no-'
        /// </summary>
        string NoPrefix { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether fields and properties must be attributed
        /// </summary>
        bool RequireAttributes { get; set; }
    }
}
