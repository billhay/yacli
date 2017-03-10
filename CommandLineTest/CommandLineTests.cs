// ------------------------------------------------------------------------------------------------- 
// <copyright file="CommandLineTests.cs" company="Bill Hay">
// Copyright (c) William D Hay
// </copyright>
// -------------------------------------------------------------------------------------------------
namespace CommandLineParser.Tests
{
    using System;
    using System.Collections.Generic;
    using CommandLine;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    internal class TestingClass
    {
#pragma warning disable 0649 
        [Argument("attribute1", ShortName = "a1", Required = true, HelpText = "This specifies string field")]
        private string stringField;

        [Argument("attribute2", ShortName = "a2")]
        public int IntField;

        [Argument("attribute3", ShortName = "a3")]
        public string Param { get; set; }

        [Argument("attribute4", ShortName = "a4")]
        public int IntParam { get; set; }
#pragma warning restore 0649

        public static StaticParse Parse(string param)
        {
            return new StaticParse { Param = param };
        }

        public string GetStringField()
        {
            return this.stringField;
        }
    }

    internal class UnattributedTestingClass
    {
#pragma warning disable 0649 
        private string stringField;

        public int IntField;

        public string Param { get; set; }

        public int IntParam { get; set; }
#pragma warning restore 0649

        public static StaticParse Parse(string param)
        {
            return new StaticParse { Param = param };
        }

        public string GetStringField()
        {
            return this.stringField;
        }
    }

    internal class Person
    {
        [Argument("firstname", Required = true)]
        public string Name { get; set; } = "not set";

        [Argument("")]
        public int Age { get; set; } = 999;
    }

    internal class BooleanTesting
    {
        [Argument("b1")]
        public bool B1 { get; set; } = false;

        [Argument("b2")]
        public bool B2 { get; set; } = true;

        public bool B3 { get; set; } = false;

        public bool B4 { get; set; } = true;


        [Argument("dummy", ShortName = "d")]
        public string Dummy { get; set; }
    }

    internal enum Colors
    {
        Red,
        White,
        Blue
    };

    internal class ClassWithEnums
    {
#pragma warning disable 0649
        internal Colors Flag1 { get; set; } = Colors.Red;

        internal Colors Flag2 = Colors.Red;
#pragma warning restore 0649
    }

    internal class ClassWithIntList
    {
        internal String TestString { get; set; }
        internal List<int> Numbers { get; set; }
    }

    internal class Calendar
    {
        internal DateTime Start { get; set; }
    }

    internal class MoreDateTimes
    {
        internal DateTime Start { get; private set; }

        internal String Command { get; private set; }

        internal Colors Color { get; private set; }

        internal bool Verbose { get; private set; }
    }


    [TestClass()]
    public class CommandLineTests
    {
        [TestMethod]
        public void TestDummy()
        {
            string[] args = { "/attribute1=101", "/a2=102", "/attribute3=103", "/a4=104", @"c:\temp\file.txt" };
            (TestingClass commandLineSwitches, List<string> unparsedCommandLineElements) = CommandLine.CommandLineParser<TestingClass>.Parse(args);
        }

        [TestMethod]
        public void CommandLineTest()
        {
            string[] args = { "/attribute1=101", "/a2=102", "/attribute3=103", "/a4=104", @"c:\temp\file.txt" };
            (TestingClass commandLineSwitches, List<string> unparsedCommandLineElements) result = CommandLine.CommandLineParser<TestingClass>.Parse(args);

            TestingClass testingClass = result.commandLineSwitches;
            Assert.AreEqual("101", testingClass.GetStringField());
            Assert.AreEqual(102, testingClass.IntField);
            Assert.AreEqual("103", testingClass.Param);
            Assert.AreEqual(104, testingClass.IntParam);
            Assert.AreEqual(1, result.unparsedCommandLineElements.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(AggregateException))]
        public void CommandLineExceptionTest()
        {
            string[] args = { "/attribute1=101", "/a2=102", "/attribute3=103", "/a4=aaa" };
            (TestingClass commandLineSwitches, List<string> unparsedCommandLineElements) testingClass 
                = CommandLine.CommandLineParser<TestingClass>.Parse(args);
        }

        [TestMethod]
        public void BadParameterExceptionTest()
        {
            string[] args = { "/attribute1=101", "/a2=102", "/attribute3=103", "/a4=aaa" };

            try
            {
                (TestingClass commandLineSwitches, List<string> unparsedCommandLineElements) result = CommandLine.CommandLineParser<TestingClass>.Parse(args);
            }
            catch (AggregateException aex)
            {
                CommandLineException ex = aex.InnerExceptions[0] as CommandLineException;
                Assert.IsNotNull(ex, "unexpected exception type in AggregateException");
                Assert.AreEqual("a4", ex.ParameterName);
                Assert.AreEqual("aaa", ex.ParameterValue);
                Assert.AreEqual(typeof(int), ex.ParameterType);
                Assert.AreEqual("Exception parsing comand line parameter: name = a4, value = aaa, type = Int32 exception = Input string was not in a correct format.", ex.Message);
                return;
            }

            Assert.Fail("Expected AggregateException not thrown");
        }

        [TestMethod]
        public void EndMarkerTest()
        {
            string[] args = { "-firstname", "richard", "--", "-age", "101" };
            (Person commandLineSwitches, List<string> unparsedCommandLineElements) = CommandLine.CommandLineParser<Person>.Parse(args);
            Assert.AreEqual("richard", commandLineSwitches.Name);
            Assert.AreEqual(999, commandLineSwitches.Age);
            Assert.AreEqual("age", unparsedCommandLineElements[0]);
            Assert.AreEqual("101", unparsedCommandLineElements[1]);
        }

        [TestMethod]
        public void UnknownParameterExceptionTest()
        {
            string[] args = { "/attribute1=101", "/aa=102", "/attribute3=103", "/a4=104" };

            try
            {
                (TestingClass commandLineSwitches, List<string> unparsedCommandLineElements) result = CommandLine.CommandLineParser<TestingClass>.Parse(args);
            }
            catch (AggregateException aex)
            {
                CommandLineException ex = aex.InnerExceptions[0] as CommandLineException;
                Assert.IsNotNull(ex, "unexpected exception type in AggregateException");
                Assert.AreEqual("aa", ex.ParameterName);
                Assert.AreEqual("102", ex.ParameterValue);
                Assert.IsNull(ex.ParameterType);
                Assert.AreEqual("Exception parsing comand line parameter: name = aa, value = 102, reason = Unknow command line switch: aa", ex.Message);
                return;
            }

            Assert.Fail("Expected AggregateException not thrown");
        }

        [TestMethod]
        public void FromCommandLine1Test()
        {
            string[] args = { "/attribute1=101", "/a2=102", "/attribute3=103", "/a4=104" };
            (TestingClass commandLineSwitches, List<string> unparsedCommandLineElements)  = CommandLine.CommandLineParser<TestingClass>.Parse(args);

            Assert.AreEqual("101", commandLineSwitches.GetStringField());
            Assert.AreEqual(102, commandLineSwitches.IntField);
            Assert.AreEqual("103", commandLineSwitches.Param);
            Assert.AreEqual(104, commandLineSwitches.IntParam);
        }

        [TestMethod]
        public void FromCommandWithNoAtributesTest()
        {
            string[] args = { "/stringfield=101", "/intfield=102", "/param=103", "/intparam=104" };
            (UnattributedTestingClass testingClass, List<string> unparsedCommandLineElements) = CommandLine.CommandLineParser<UnattributedTestingClass>.Parse(args);

            Assert.AreEqual("101", testingClass.GetStringField());
            Assert.AreEqual(102, testingClass.IntField);
            Assert.AreEqual("103", testingClass.Param);
            Assert.AreEqual(104, testingClass.IntParam);
        }

        [TestMethod]
        public void EnumTest()
        {
            string[] args = { "test", "--flag1:white", "--flag2:blue" };
            (ClassWithEnums cwe, List<string> unparsedCommandLineElements) = CommandLine.CommandLineParser<ClassWithEnums>.Parse(args);

            Assert.AreEqual(Colors.White, cwe.Flag1);
            Assert.AreEqual(Colors.Blue, cwe.Flag2);
        }

        [TestMethod]
        [ExpectedException(typeof(AggregateException))]
        public void RequiredParameterNotSetTest()
        {
            string[] args = { "-name", "bill", "-age", "21" };
            var result = CommandLine.CommandLineParser<Person>.Parse(args);
        }

        [TestMethod]
        public void TestBoolean()
        {
            string randomString = "the quick brown fox";
            string[] args = { "-b1:true", "-b2:false", "-d", randomString };

            (BooleanTesting bt, List<string> unparsedCommandLineElements) = CommandLine.CommandLineParser<BooleanTesting>.Parse(args);

            Assert.IsTrue(bt.B1, "1");
            Assert.IsFalse(bt.B2, "2");
            Assert.AreEqual(randomString, bt.Dummy, "3");

            args = new [] { "-b3:true", "-b4:false", "-d", randomString};
            (bt, unparsedCommandLineElements) = CommandLine.CommandLineParser<BooleanTesting>.Parse(args);
            Assert.IsTrue(bt.B3, "4");
            Assert.IsFalse(bt.B4, "5");
            Assert.AreEqual(randomString, bt.Dummy, "6");

            args = new []{ "-b1", "-b2", "-d", randomString };
            (bt, unparsedCommandLineElements) = CommandLine.CommandLineParser<BooleanTesting>.Parse(args);

            Assert.IsTrue(bt.B1, "7");
            Assert.IsTrue(bt.B2, "8");
            Assert.AreEqual(randomString, bt.Dummy, "9");

            args = new[] { "-d", randomString, "-b2", "-b1" };
            (bt, unparsedCommandLineElements) = CommandLine.CommandLineParser<BooleanTesting>.Parse(args);

            Assert.IsTrue(bt.B1, "10");
            Assert.IsTrue(bt.B2, "11");
            Assert.AreEqual(randomString, bt.Dummy, "12");
        }

        [TestMethod()]
        public void FirstWithListTest()
        {
            string[] args = { "-numbers", "1" };
            (ClassWithIntList temp, List<string> unparsedCommandLineElements) = CommandLineParser<ClassWithIntList>.Parse(args);
            ClassWithIntList arguments = temp;
            Assert.IsNotNull(arguments.Numbers);
            Assert.IsInstanceOfType(arguments.Numbers, typeof(List<int>));
            Assert.AreEqual(1, arguments.Numbers.Count);
            Assert.AreEqual(1, arguments.Numbers[0]);
        }

        [TestMethod()]
        public void SecondWithListTest()
        {
            string[] args = { "-numbers", "1", "2" };
            (ClassWithIntList temp, List<string> unparsedCommandLineElements) = CommandLineParser<ClassWithIntList>.Parse(args);
            ClassWithIntList arguments = temp;
            Assert.IsNotNull(arguments.Numbers);
            Assert.IsInstanceOfType(arguments.Numbers, typeof(List<int>));
            Assert.AreEqual(2, arguments.Numbers.Count);
            Assert.AreEqual(1, arguments.Numbers[0]);
            Assert.AreEqual(2, arguments.Numbers[1]);
        }

        [TestMethod()]
        public void DateTimeTest()
        {
            string[] args = { "-Start", "03/12/2017 15:40:23" };
            (Calendar calendar, List<string> files) = CommandLineParser<Calendar>.Parse(args);
        }

        [TestMethod()]
        public void MoreDateTimeTest()
        {
            try
            {
                string[] args = { "-command", "transform", "-start", "3/12/2017 21:22:19", "-Verbose", "-color", "red",  @"c:\temp\fred.txt", @"c:\temp\jim.txt"};
                (MoreDateTimes calendar, List<string> files) = CommandLineParser<MoreDateTimes>.Parse(args);
            }
            catch (AggregateException aggregateException)
            {
                foreach (Exception ex in aggregateException.InnerExceptions)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }
}