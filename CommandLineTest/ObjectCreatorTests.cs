// ------------------------------------------------------------------------------------------------- 
// <copyright file="ObjectCreatorTests.cs" company="Bill Hay">
// Copyright (c) William D Hay
// </copyright>
// -------------------------------------------------------------------------------------------------
namespace CommandLineParser.Tests
{
    using System;
    using System.Reflection;
    using CommandLine.Implementation;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    internal class StringConstructor
    {
        public StringConstructor(string param)
        {
            this.Param = param;
        }

        public string Param { get; set; }
    }

    internal class StaticParse
    {
#pragma warning disable 0649
        public string StringField;

        public int IntField;

        public string Param { get; set; }

        public int IntParam { get; set; }
#pragma warning restore 0469

        public static StaticParse Parse(string param)
        {
            return new StaticParse { Param = param };
        }
    }

    [TestClass()]
    public class ObjectCreatorTests
    {
        [TestMethod()]
        public void StringConstructorTest()
        {
            string param = "How now brown cow";
            StringConstructor tc = ObjectCreator.Create<StringConstructor>(param);
            Assert.AreEqual(param, tc.Param);
        }

        [TestMethod]
        public void StaticParseTest()
        {
            string param = "How now brown cow";
            StaticParse staticParse = ObjectCreator.Create<StaticParse>(param);
            Assert.AreEqual(param, staticParse.Param);
        }

        [TestMethod]
        public void StaticParseIntTest()
        {
            string param = "123";
            int actual = ObjectCreator.Create<int>(param);
            Assert.AreEqual(123, actual);
        }

        [TestMethod]
        public void ParseDateTest()
        {
            DateTime expected = DateTime.Now;
            string param = expected.ToString("G");
            expected = DateTime.Parse(param);
            DateTime actual = ObjectCreator.Create<DateTime>(param);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void SetStringIntoClassTest()
        {
            string expected = "The rain in Spain";
            StaticParse obj = new StaticParse();
            PropertyInfo paramInfo = typeof (StaticParse).GetProperty("Param");
            ObjectCreator.Assign(obj, paramInfo, expected);
            Assert.AreEqual(expected, obj.Param);
        }

        [TestMethod]
        public void SetIntegerIntoClassTest()
        {
            int expected = 12345;
            StaticParse obj = new StaticParse();
            PropertyInfo paramInfo = typeof(StaticParse).GetProperty("IntParam");
            ObjectCreator.Assign(obj, paramInfo, expected.ToString());
            Assert.AreEqual(expected, obj.IntParam);
        }

        [TestMethod]
        public void SetStringFieldIntoClassTest()
        {
            string expected = "The rain in Spain";
            StaticParse obj = new StaticParse();
            MemberInfo paramInfo = typeof(StaticParse).GetField("StringField");
            ObjectCreator.Assign(obj, paramInfo, expected);
            Assert.AreEqual(expected, obj.StringField);
        }
    }
}