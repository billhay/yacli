// ------------------------------------------------------------------------------------------------- 
// <copyright file="Parser.cs" company="Bill Hay">
// Copyright (c) William D Hay
// </copyright>
// -------------------------------------------------------------------------------------------------
namespace CommandLine.Implementation
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Runtime.CompilerServices;

    internal enum CommandLineArgumentType
    {
        Switch,
        Value,
        EndSwitchMarker
    };

    /// <summary>
    /// Converts the command line into a Dictionary
    /// </summary>
    /// <typeparam name="T">The type of the application command line argument class</typeparam>
    internal class Parser<T> where T : new()
    {
        /// <summary>
        /// The command line context (prefix style etc.)
        /// </summary>
        private readonly Context<T> context;

        /// <summary>
        ///   Copy of the command line arguments
        /// </summary>
        private readonly string[] args;

        /// <summary>
        ///   The index of the argument currently being processed
        /// </summary>
        private int currentIndex;

        /// <summary>
        /// Initializes a new instance of the <see cref="Parser{T}"/> class
        /// </summary>
        /// <param name="args">The command line</param>
        /// <param name="start">
        ///   The start index into the array. Usually 0, but there is a pattern of calls like
        ///   <c>app.exe command -param1 ...</c>
        /// </param>
        /// <param name="context">The command line context</param>
        internal Parser(string[] args, int start, Context<T> context)
        {
            // clone the arguments just to be super safe
            this.args = args.ToArray();
            this.currentIndex = start;
            this.context = context;
        }

        /// <summary>
        /// Parses the command line into a key/value dictionary
        /// </summary>
        /// <returns>The command line arguments as key/value pairs</returns>
        /// <remarks>Just to keep unit tests which used this not obsolescent API happy</remarks>
        internal IReadOnlyDictionary<string, List<string>> Parse()
        {
            Dictionary<string, List<string>> result = new Dictionary<string, List<string>>();
            this.Parse(result, this.context.CommandLineNonSwitches);
            return result;
        }

        /// <summary>
        /// Fills in the switchArguments and nonSwitchArguments data structures
        /// </summary>
        /// <param name="switchArguments">switch arguments - i.e. ones that have a -, --, or / in front, and their corresponding values</param>
        /// <param name="nonSwitchArguments">all non switch arguments</param>
        internal void Parse(Dictionary<string, List<string>> switchArguments, List<string> nonSwitchArguments)
        {
            bool parsing = true;

            while (this.currentIndex < this.args.Length)
            {
                try
                {
                    string arg = this.args[this.currentIndex];
                    string orginalArg = arg; 
                    CommandLineArgumentType argumentType = this.GetCommandLineArgumentType(ref arg);

                    if (argumentType == CommandLineArgumentType.EndSwitchMarker)
                    {
                        parsing = false;
                        this.currentIndex++;
                        continue;
                    }

                    List<string> valueList = new List<string>();
                    string value = string.Empty;

                    if (parsing && argumentType == CommandLineArgumentType.Switch)
                    {
                        string[] keyValue = this.SplitIntoKeyAndValue(arg);
                        string key = keyValue[0];
                        if (key.StartsWith(this.context.Settings.NoPrefix))
                        {
                            key = key.Replace(this.context.Settings.NoPrefix, string.Empty);
                            value = false.ToString(CultureInfo.CurrentCulture);
                        }

                        ArgumentDescriptor ar = this.context.GetDescriptor(key);
                        if (ar == null)
                        {
                            ++this.currentIndex;
                            throw new CommandLineException(key, keyValue[1] ?? string.Empty, "Unknow command line switch: " + key);
                        }

                        // if target is a bool then:
                        // 1. if argument begins 'no-' it is always false, and should have no value
                        // 2. if argument does not begin 'no-', and has a value separated by a defined separator, it takes that value
                        // 3. it is true
                        if (ar.MemberType == typeof(bool))
                        {
                            if (string.IsNullOrWhiteSpace(value))
                            {
                                // I am unclear about localization of string representations of bool: see http://stackoverflow.com/a/20620119
                                value = keyValue.Length > 1 ? keyValue[1] : true.ToString(CultureInfo.CurrentCulture);
                            }
                            else if (keyValue.Length > 1)
                            {
                                // this is a no-... switch, not allowed to have a value
                                throw new CommandLineException(orginalArg, keyValue[1], "Value not allowed on a 'no-...' bool switch");
                            }
                        }
                        else
                        {
                            if (keyValue.Length > 1)
                            {
                                value = keyValue[1];
                            }
                            else if (this.currentIndex + 1 < this.args.Length)
                            {
                                string v = this.args[this.currentIndex + 1];
                                if (!this.LooksLikeASwitch(v))
                                {
                                    value = v;
                                    ++this.currentIndex;
                                }
                            }
                        }

                        while (true)
                        {
                            valueList.Add(value);
                            if (!ar.IsList || (this.currentIndex + 1 >= this.args.Length) ||
                                this.LooksLikeASwitch(this.args[this.currentIndex + 1]))
                            {
                                break;
                            }

                            value = this.args[++this.currentIndex];
                        }

                        switchArguments[key] = valueList;
                    }
                    else
                    {
                       nonSwitchArguments.Add(arg);
                    }

                    ++this.currentIndex;
                }
                catch (Exception ex)
                {
                    this.context.Exceptions.Add(ex);
                }
            }
        }

        /// <summary>
        /// Strips the switch indicator off the front of the argument 
        /// </summary>
        /// <param name="s">the parameter with switch prefix ("--", "-", "/" ...)</param>
        /// <returns>the parameter with the switch prefix removed, or null if there is no switch prefix</returns>
        private string StripSwitchIndicator(string s)
        {
            s = s.Trim();
            return
                this.context.Settings.SwitchPrefixes.Where(s.StartsWith)
                    .Select(x => s.Substring(x.Length))
                    .FirstOrDefault();
        }

        /// <summary>
        /// Determines the type of the arg  
        /// - switch (because it has a switch prefix)
        /// - endOfSwitchMarker (switch prefix but nothing following it
        /// - token
        /// If its a switch it strips the prefix
        /// </summary>
        /// <param name="arg">The command line argument - if its a switch it will have the prefix stripped</param>
        /// <returns>The type of the command line argument (not the .net type)</returns>
        private CommandLineArgumentType GetCommandLineArgumentType(ref string arg)
        {
            arg = arg.Trim();

            foreach (string prefix in this.context.Settings.SwitchPrefixes)
            {
                if (arg.StartsWith(prefix))
                {
                    arg = arg.Substring(prefix.Length);
                    return string.IsNullOrEmpty(arg) ? CommandLineArgumentType.EndSwitchMarker : CommandLineArgumentType.Switch;
                }
            }

            return CommandLineArgumentType.Value;
        }

        /// <summary>
        /// Test if this is an end of switch marker - i.e. something like '--' without a value
        /// </summary>
        /// <param name="s">The command line argument to check</param>
        /// <returns>True if this is a stand-alone switch marker</returns>
        private bool IsEndOfSwitchesMarker(string s)
        {
            return this.context.Settings.SwitchPrefixes.Contains(s);
        }

        /// <summary>
        /// Check if string starts with a valid switch setting
        /// </summary>
        /// <param name="s">the parameter with switch prefix ("--", "-", "/" ...)</param>
        /// <returns>true if the string appears to be a switch</returns>
        private bool LooksLikeASwitch(string s)
        {
            return
                this.context.Settings.SwitchPrefixes.Where(s.StartsWith)
                    .Select(x => true)
                    .FirstOrDefault();
        }

        /// <summary>
        /// Splits a parameter such as /file=c:\foo\bar.txt into the parameter name and value
        /// </summary>
        /// <param name="arg">parameter name</param>
        /// <returns>the name and value in a string array. If there is no value then the array has only the name</returns>
        /// <remarks>
        /// This is complicated by the possibility that the value may itself contain one of the switch values. So
        /// the code finds the first possible switch in the input string, and treats everything after that as the value
        /// </remarks>
        private string[] SplitIntoKeyAndValue(string arg)
        {
            int posn = arg.Length;
            int bestMatchLength = 0;
            foreach (string separator in this.context.Settings.SwitchSeparators)
            {
                int p = arg.IndexOf(separator, 0, this.context.Settings.CaseMatching);
                if (p != -1 && p < posn)
                {
                    posn = p;
                    bestMatchLength = separator.Length;
                }
            }

            if (bestMatchLength > 0)
            {
                return new[] { arg.Substring(0, posn), arg.Substring(posn + bestMatchLength) };
            }

            return new[] { arg };
        }
    }
}
