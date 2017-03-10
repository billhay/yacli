

namespace CommandLineTest
{
    using System;
    using System.Linq;
    using CommandLine;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    public enum Colors
    {
        Red,
        White,
        Blue
    };

    [TestClass]
    public class DescriptorTests
    {
        class TestClass
        {
            [Argument("String", ShortName = "s", Required = true, HelpText = "a random string")]
            public string String { get; set; }

            [Argument("Color", ShortName = "c", HelpText = "Red White or Blue")]
            public Colors Color { get; set; }

            [Argument("Verbose", ShortName = "v", HelpText = "True for verbose output")]
            public bool Verbose { get; set; }
        }

        [TestMethod]
        public void BasicStrictCaseTest()
        {
            ICommandLineSettings settings = CommandLineParser<TestClass>.CommandLineSettings();
            settings.CaseMatching = StringComparison.CurrentCulture;

            IArgumentDescriptor[] descriptors = CommandLineParser<TestClass>.GetDescriptors(settings).ToArray();

            Assert.AreEqual(3, descriptors.Length);

            Assert.AreEqual("String", descriptors[0].Name);
            Assert.AreEqual("s", descriptors[0].ShortName);
            Assert.AreEqual(typeof(string), descriptors[0].MemberType);
            Assert.AreEqual("a random string", descriptors[0].Help);

            Assert.AreEqual("Color", descriptors[1].Name);
            Assert.AreEqual("c", descriptors[1].ShortName);
            Assert.AreEqual(typeof(Colors), descriptors[1].MemberType);
            Assert.AreEqual("Red White or Blue", descriptors[1].Help);

            Assert.AreEqual("Verbose", descriptors[2].Name);
            Assert.AreEqual("v", descriptors[2].ShortName);
            Assert.AreEqual(typeof(bool), descriptors[2].MemberType);
            Assert.AreEqual("True for verbose output", descriptors[2].Help);
        }

        [TestMethod]
        public void BasicIgnoreCaseTest()
        {
            ICommandLineSettings settings = CommandLineParser<TestClass>.CommandLineSettings();
            settings.CaseMatching = StringComparison.CurrentCultureIgnoreCase;
            IArgumentDescriptor[] descriptors = CommandLineParser<TestClass>.GetDescriptors(settings).ToArray();

            Assert.AreEqual(3, descriptors.Length);

            Assert.AreEqual("string", descriptors[0].Name);
            Assert.AreEqual("s", descriptors[0].ShortName);
            Assert.AreEqual(typeof(string), descriptors[0].MemberType);
            Assert.AreEqual("a random string", descriptors[0].Help);

            Assert.AreEqual("color", descriptors[1].Name);
            Assert.AreEqual("c", descriptors[1].ShortName);
            Assert.AreEqual(typeof(Colors), descriptors[1].MemberType);
            Assert.AreEqual("Red White or Blue", descriptors[1].Help);

            Assert.AreEqual("verbose", descriptors[2].Name);
            Assert.AreEqual("v", descriptors[2].ShortName);
            Assert.AreEqual(typeof(bool), descriptors[2].MemberType);
            Assert.AreEqual("True for verbose output", descriptors[2].Help);
        }

    }
}
