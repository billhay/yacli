

namespace CommandLineTest
{
    using System;
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    internal class WithBool
    {
        internal string Alpha { get; set; }
        internal bool Beta { get; set; }
        internal string Gamma { get; set; }
    }

    internal class WithBoolTrue
    {
        internal string Alpha { get; set; }
        internal bool Beta { get; set; } = true;
        internal string Gamma { get; set; }
    }

    [TestClass]
    public class FalseBoolUnitTests
    {
        [TestMethod]
        public void NotOnCommandLineTest()
        {
            string[] args = new[] { "-alpha:start", "-gamma:end" };
            WithBool wb = FalseBoolUnitTests.Parse(args);
            Assert.IsFalse(wb.Beta);
        }

        [TestMethod]
        public void TrueNotOnCommandLineTest()
        {
            string[] args = new[] { "-alpha:start", "-gamma:end" };
            WithBool wb = FalseBoolUnitTests.Parse(args);
            Assert.IsFalse(wb.Beta);
        }

        [TestMethod]
        public void OnCommandLineTest()
        {
            string[] args = new[] { "-alpha:start", "-beta", "-gamma:end" };
            WithBool wb = FalseBoolUnitTests.Parse(args);
            Assert.IsTrue(wb.Beta);
        }

        [TestMethod]
        public void OnCommandLineWithNonSwitchTest()
        {
            string[] args = new[] { "-alpha:start", "-beta", "dummy", "-gamma:end" };
            WithBool wb = FalseBoolUnitTests.Parse(args);
            Assert.IsTrue(wb.Beta);
        }

        [TestMethod]
        public void OnCommandLineWithNoPrefixTest()
        {
            string[] args = new[] { "-alpha:start", "-no-beta", "dummy", "-gamma:end" };
            WithBool wb = FalseBoolUnitTests.Parse(args);
            Assert.IsFalse(wb.Beta);
        }

        [TestMethod]
        public void WithTrueTest()
        {
            string[] args = new[] { "-alpha:start", "-beta:true", "dummy", "-gamma:end" };
            WithBool wb = FalseBoolUnitTests.Parse(args);
            Assert.IsTrue(wb.Beta);
        }

        [TestMethod]
        public void WithFalseTest()
        {
            string[] args = new[] { "-alpha:start", "-beta:false", "dummy", "-gamma:end" };
            WithBool wb = FalseBoolUnitTests.Parse(args);
            Assert.IsFalse(wb.Beta);
        }

        private static WithBool Parse(string[] args)
        {
            ValueTuple<WithBool, List<string>> result = CommandLine.CommandLineParser<WithBool>.Parse(args);
            return result.Item1;
        }
    }

    [TestClass]
    public class TrueBoolUnitTests
    {
        [TestMethod]
        public void NotOnCommandLineTest()
        {
            string[] args = new[] { "-alpha:start", "-gamma:end" };
            WithBoolTrue wb = TrueBoolUnitTests.Parse(args);
            Assert.IsTrue(wb.Beta);
        }

        [TestMethod]
        public void OnCommandLineTest()
        {
            string[] args = new[] { "-alpha:start", "-beta", "-gamma:end" };
            WithBoolTrue wb = TrueBoolUnitTests.Parse(args);
            Assert.IsTrue(wb.Beta);
        }

        [TestMethod]
        public void OnCommandLineWithNonSwitchTest()
        {
            string[] args = new[] { "-alpha:start", "-beta", "dummy", "-gamma:end" };
            WithBoolTrue wb = TrueBoolUnitTests.Parse(args);
            Assert.IsTrue(wb.Beta);
        }

        [TestMethod]
        public void OnCommandLineWithNoPrefixTest()
        {
            string[] args = new[] { "-alpha:start", "-no-beta", "dummy", "-gamma:end" };
            WithBoolTrue wb = TrueBoolUnitTests.Parse(args);
            Assert.IsFalse(wb.Beta);
        }

        [TestMethod]
        public void WithTrueTest()
        {
            string[] args = new[] { "-alpha:start", "-beta:true", "dummy", "-gamma:end" };
            WithBoolTrue wb = TrueBoolUnitTests.Parse(args);
            Assert.IsTrue(wb.Beta);
        }

        [TestMethod]
        public void WithFalseTest()
        {
            string[] args = new[] { "-alpha:start", "-beta:false", "dummy", "-gamma:end" };
            WithBoolTrue wb = TrueBoolUnitTests.Parse(args);
            Assert.IsFalse(wb.Beta);
        }

        private static WithBoolTrue Parse(string[] args)
        {
            ValueTuple<WithBoolTrue, List<string>> result = CommandLine.CommandLineParser<WithBoolTrue>.Parse(args);
            return result.Item1;
        }
    }
}
