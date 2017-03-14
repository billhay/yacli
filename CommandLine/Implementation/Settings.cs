// ------------------------------------------------------------------------------------------------- 
// <copyright file="Settings.cs" company="Bill Hay">
// Copyright (c) William D Hay
// </copyright>
// -------------------------------------------------------------------------------------------------
namespace CommandLine.Implementation
{
    using System;

    /// <summary>
    /// The various customizable settings
    /// </summary>
    internal class Settings : ICommandLineSettings
    {
        /// <summary>
        /// Gets or sets a value indicating if parameter matching is to be case (in)sensitive
        /// </summary>
        public StringComparison CaseMatching { get; set; } = StringComparison.CurrentCultureIgnoreCase;

        /// <summary>
        /// Gets or sets the set of possible switch prefixes
        /// </summary>
        /// <remarks>in setting this, make sure "--" comes before "-" in the list</remarks>
        public string[] SwitchPrefixes { get; set; } = { "--", "-", "/" };

        /// <summary>
        /// Gets or sets the set of allowed separator characters between the switch and its value
        /// </summary>
        public string[] SwitchSeparators { get; set; } = { ":", "=" };

        /// <summary>
        /// Gets or sets a value indicating whether the argument start with 'no-'
        /// </summary>
        public string NoPrefix { get; set; } = "no-";

        /// <summary>
        /// Gets or sets a value indicating whether fields and properties must be attributed
        /// </summary>
        public bool RequireAttributes { get; set; } = false;
    }
}
