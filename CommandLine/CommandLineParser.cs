// ------------------------------------------------------------------------------------------------- 
// <copyright file="CommandLineParser.cs" company="Bill Hay">
// Copyright (c) William D Hay
// </copyright>
// -------------------------------------------------------------------------------------------------
namespace CommandLine
{
    using System;
    using System.Collections.Generic;
    using Implementation;

    /// <summary>
    /// External API
    /// </summary>
    /// <typeparam name="T">The application command line argument specification class</typeparam>
    public class CommandLineParser<T> where T : new()
    {
        /// <summary>
        /// Convenience overload when we start parsing at the first element in args[]
        /// </summary>
        /// <param name="args">The command line arguments</param>
        /// <param name="start">The index in args to start parsing at - typically 0 or 1</param>
        /// <param name="settings">The command line settings</param>
        /// <returns>A populated instance of the application command line settings class</returns>
        public static ValueTuple<T, List<string>> Parse(string[] args, int start, ICommandLineSettings settings)
        {
            if (args == null)
            {
                throw new ArgumentNullException(nameof(args), "Argument cannot be null");
            }

            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings), "Argument cannot be null");
            }

            Context<T> result = Processor<T>.Parse(args, start, settings);
            if (result.Exceptions.Count > 0)
            {
                throw new AggregateException("Failed to parse command line", result.Exceptions);
            }

            return new ValueTuple<T, List<string>>(result.CommandLineSwitches, result.CommandLineNonSwitches);
        }

        /// <summary>
        /// Convenience overload for default parser settings
        /// </summary>
        /// <param name="args">The command line arguments</param>
        /// <param name="start">The index in args to start parsing at - typically 0 or 1</param>
        /// <returns>A populated instance of the application command line settings class</returns>
        public static ValueTuple<T, List<string>> Parse(string[] args, int start)
        {
            return CommandLineParser<T>.Parse(args, start, new Settings());
        }

        /// <summary>
        /// Convenience overload when we start parsing at the first element in args[] and use default settings
        /// </summary>
        /// <param name="args">The command line arguments</param>
        /// <returns>A populated instance of the application command line settings class</returns>
        public static ValueTuple<T, List<string>> Parse(string[] args)
        {
            return CommandLineParser<T>.Parse(args, 0, new Settings());
        }

        /// <summary>
        /// Factory to provide a Settings object to the user
        /// </summary>
        /// <returns>A newly minted command line settings object - it is pre-populated with default values</returns>
        public static ICommandLineSettings CommandLineSettings()
        {
            return new Settings();
        }

        /// <summary>
        /// get descriptors - allows application to build a help command
        /// </summary>
        /// <param name="settings">The appropriate command line settings</param>
        /// <returns>An enumerator over the command line settings</returns>
        public static IEnumerable<IArgumentDescriptor> GetDescriptors(ICommandLineSettings settings)
        {
            return Processor<T>.GetArgumentDescriptors(settings);
        }
    }
}