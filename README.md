# <center>YAC#CLP - another command line parser for .net</center>

## Quick Start

At its simplest, all you need to do is:

* Include the YACCLP nuget in your project
* Write a class with a property or field for every command line switch that can be used, say _CommandLineParmeters_
* Call    _CommandLine`<CommandLineParameters`>.Parse(args)_
* The result is returned as a ValueTuple, the first element of which is an instance of _CommandLineParameters_ with the appropriate Properties/Fields set from the 
command, line, and the second element is a list of non-switch elements, perhaps a list of files the application will work on.

Example:  _Note: this uses the new C# 7.0 syntax for returning multiple values from a method. You need to use a slight variant on this if you are using C# 6.x, which will be shown later_ 

 
    namespace CommandLineDemo
    {
        using System;
        using System.Collections.Generic;
        using CommandLine;

        /*
       Input:
       > CommandLineDemo.exe -command:transform -start:"3/12/2017 21:22:19" -verbose -color:blue "c:\temp\fred.txt c:\temp\jim.txt

       Output:
          Command: transform
            Color: White
            Start: 3/12/2017 21:22:19
          Verbose: False

          Files:
              c:\temp\fred.txt
              c:\temp\jim.txt
       */

        internal enum Colors
        {
            Red,
            White,
            Blue
        };

        internal class CommandLineArguments
        {
            internal DateTime Start;
            internal String Command { get; private set; }
            internal Colors Color { get; private set; }
            internal bool Verbose { get; private set; }
        }
   
        class Program
        {
            static void Main(string[] args)
            {
                try
                {
                    // nb - this is the C# 7.0 ValueTuple style call
                    (CommandLineArguments cla, List<string> files) = CommandLineParser<CommandLineArguments>.Parse(args);

                    Console.WriteLine("Command: {0}", cla.Command);
                    Console.WriteLine("  Color: {0}", cla.Color);
                    Console.WriteLine("  Start: {0}", cla.Start);
                    Console.WriteLine("Verbose: {0}", cla.Verbose);
                    Console.WriteLine();
                    Console.WriteLine("Files: ");
                    foreach (string file in files)
                    {
                        Console.WriteLine("    {0}", file);
                    }

                    // in C# 6.5 you would do:
                    ValueTuple<CommandLineArguments, List<string>> vt = CommandLineParser<CommandLineArguments>.Parse(args);
                    CommandLineArguments cla1 = vt.Item1;
                    List<string> files1 = vt.Item2;
                    Console.WriteLine("Command: {0}", cla1.Command);
                    // etc.

                }
                catch (AggregateException aex)
                {
                    foreach (Exception ex in aex.InnerExceptions)
                    {
                        Console.Error.WriteLine("Exception: {0}", ex.Message);
                    }
                }
            }
        }
    }

Running this with the command line:

    $ CommandLineDemo.exe -command:transform -start:"3/12/2017 21:22:19" -verbose -color:blue "c:\temp\fred.txt c:\temp\jim.txt

     Command: transform
            Color: White
            Start: 3/12/2017 21:22:19
          Verbose: False

          Files:
              c:\temp\fred.txt
              c:\temp\jim.txt

    $

Things to notice about this:

* The fields/properties in the user defined class are strongly typed. The only requirement on the types are that they must have either a 
constructor that takes a single string parameter, or a static member Parse which also must take a single string parameter.
* By default matching is case insensitive, using *CurrentCultureIgnoreCase*  for string comparison.
* The switch prefix is by default any of *-*, *--*, or */*. The seperator between the switch and its argument can be *:*, *=*, or a space.

The switch prefixes, switch/argument seperators, and case matching can be changed via a configuration object. And fields/properties can have an attribute
which allows you to choose the switch name, including having an abbreviated name, on the command line.

## Attributed Fields and Properties

Any field or property can have an ArgumentAttribute attached to it:

        [Argument("command", ShortName = "c", Required = true, HelpText = "This specifies string field")]
        internal string ApplicationCommand { get; private set }

* The first parameter is the switch name, which overrides the field/property name. In this case you would use:  
    /argument=delete  
* The *ShortName* is just an optional alias for the full name. It is a string, so does not have to be a single character
* If *Required* is set to *true* the parser will throw an exception if the switch is not provided
* "HelpText" is some text which can be obtained by the application to aid it in constructing a *help* string

## Configuration

Configuration settings can be changed using an *ICommandLineSettings* object:

    internal class Calendar
    {
        internal DateTime Start { get; set; }
    }

    .....

    string[] args = { "-Start", "03/12/2017 15:40:23" };

    ICommandLineSettings settings = CommandLineParser<Calendar>.CommandLineSettings();
    settings.CaseMatching = StringComparison.Ordinal;
    (Calendar calendar, List<string> files) = CommandLineParser<Calendar>.Parse(args, settings);

This will only match if switch matches the case.

The settings that can be customized are:
* *CaseMatching*:       switch in the command line must match the case of the name in the attribute or the property name, 
default is *CurrentCultureIgnoreCase*
* *SwitchPrefixes*:     the set of recognized switch prefixes, by default  *{"--", "-", "/" }
* *SwitchSeparators*:   The set of switch separators, by default *{ "=", ":" }*. *Note: white space is also a separator. 
And if you use = or :, there must be no white space between the switch, the separator, and the value
* *RequireAttributes*:  The switches must be attributed

## enums, bools and dates

As shown in the quick start you can have enums and bools as fields/properties.

For enums the parsing of the value is always ignore case, regardless of the *CaseMatching* settings.

For bools you can either have no value, in which case te presence of the switch in the command line will set the value to *true*,
 or you can specify *true* or *false* as a value, but only if you are using a separator. This does not work when space is used 
 as a separator.

Dates and times are parsed using *DateTime.Parse(string)*

## Parse Overloads

        public static ValueTuple<T, List<string>> Parse(string[] args))
        public static ValueTuple<T, List<string>> Parse(string[] args, int start)
        public static ValueTuple<T, List<string>> Parse(string[] args, ICommandLineSettings settings)
        public static ValueTuple<T, List<string>> Parse(string[] args, int start, ICommandLineSettings settings)

*start* allows you to set the starting position for the parse in the *args* array. This intended for applications like *git* 
where the first parameter is a specific command:

    git add [--verbose | -v] [--dry-run | -n] [--force | -f] [--interactive | -i] [--patch | -p]
	  [--edit | -e] [--[no-]all | --[no-]ignore-removal | [--update | -u]]
	  [--intent-to-add | -N] [--refresh] [--ignore-errors] [--ignore-missing]
	  [--chmod=(+|-)x] [--] [<pathspec>…​]

In this case the application would switch on the first parameter and start parsing at the second one (i.e. start = 1, since 
that is the index of the second parameter)

## Errors

On errors the Parse method will throw an *AggregateException*, for example is you have an int type and pass it an alphabetic value. 
The parser stores these errors until it has attempted to parse the entire command line, and will add each exception it gets to the 
*InnerExceptions* in the *Aggregate Exception*

## Help support

The content, formating and display of help information is something the application designer needs to decide on. But as a way of 
assisting in this the library will make available an enumeration of the descriptors:

    public static IEnumerable<IArgumentDescriptor> GetDescriptors(ICommandLineSettings settings)

This returns the descriptors sorted first by required/not required and then alphabetically.

*IArgumentDescriptor* is:

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

## Some things I might add/change

* Add a configuration setting to turn off space as a separator
* support for the *--no-* prefix to *bool* parameters
* *--* to indicate end of parsing of switches
* treat --help as a special case.  
