// ------------------------------------------------------------------------------------------------- 
// <copyright file="IArgumentDescriptor.cs" company="Bill Hay">
// Copyright (c) William D Hay
// </copyright>
// -------------------------------------------------------------------------------------------------
namespace CommandLine
{
    using System;

    /// <summary>
    /// Describes a single attribute
    /// </summary>
    public interface IArgumentDescriptor
    {
        /// <summary>
        /// Gets the argument name (long form)
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the argument name (short form). This is optional
        /// </summary>
        string ShortName { get; }

        /// <summary>
        /// Gets the type of the field or property
        /// </summary>
        Type MemberType { get; }

        /// <summary>
        /// Gets the help text
        /// </summary>
        string Help { get; }
    }
}