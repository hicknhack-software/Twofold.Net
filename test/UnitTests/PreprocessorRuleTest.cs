using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTests.Helper;
using Twofold.Compilation;
using Twofold.Interface.Compilation;
using System.Collections.Generic;

namespace UnitTests
{
    [TestClass]
    public class PreprocessorRuleTest
    {
        [TestMethod]
        public void Empty()
        {
            var line = Tools.CreateFileLine("#", 0);
            var rule = new PreprocessorRule();
            List<AsbtractCodeFragment> fragments = rule.Parse(line, new NullMessageHandler());

            Assert.AreEqual(1, fragments.Count);

            Assert.AreEqual(CodeFragmentTypes.OriginScript, fragments[0].Type);
            Assert.AreEqual("#", fragments[0].Span.Text);
        }

        [TestMethod]
        public void UnknownDirective()
        {
            var line = Tools.CreateFileLine("#debug", 0);
            var rule = new PreprocessorRule();
            List<AsbtractCodeFragment> fragments = rule.Parse(line, new NullMessageHandler());

            Assert.AreEqual(1, fragments.Count);

            Assert.AreEqual(CodeFragmentTypes.OriginScript, fragments[0].Type);
            Assert.AreEqual("#debug", fragments[0].Span.Text);
        }

        [TestMethod]
        public void UnknownPragmaDirective()
        {
            var line = Tools.CreateFileLine("#pragma warning 120, off", 0);
            var rule = new PreprocessorRule();
            List<AsbtractCodeFragment> fragments = rule.Parse(line, new NullMessageHandler());

            Assert.AreEqual(1, fragments.Count);

            Assert.AreEqual(CodeFragmentTypes.OriginScript, fragments[0].Type);
            Assert.AreEqual("#pragma warning 120, off", fragments[0].Span.Text);
        }

        [TestMethod]
        public void IncludePragmaDirective()
        {
            var line = Tools.CreateFileLine("#pragma include \"File.cs\"", 0);
            var rule = new PreprocessorRule();
            List<AsbtractCodeFragment> fragments = rule.Parse(line, new NullMessageHandler());

            Assert.AreEqual(1, fragments.Count);

            Assert.AreEqual(CodeFragmentTypes.OriginPragma, fragments[0].Type);
            Assert.AreEqual("#pragma include \"File.cs\"", fragments[0].Span.Text);
            Assert.AreEqual("include", ((OriginPragma)fragments[0]).Name);
            Assert.AreEqual("File.cs", ((OriginPragma)fragments[0]).Argument);
        }

        [TestMethod]
        public void EmptyIncludePragmaDirective()
        {
            var line = Tools.CreateFileLine("#pragma include \"\"", 0);
            var rule = new PreprocessorRule();
            List<AsbtractCodeFragment> fragments = rule.Parse(line, new NullMessageHandler());

            Assert.AreEqual(1, fragments.Count);

            Assert.AreEqual(CodeFragmentTypes.OriginPragma, fragments[0].Type);
            Assert.AreEqual("#pragma include \"\"", fragments[0].Span.Text);
            Assert.AreEqual("include", ((OriginPragma)fragments[0]).Name);
            Assert.AreEqual("", ((OriginPragma)fragments[0]).Argument);
        }

        [TestMethod]
        public void IncludePragmaNoFileDirective()
        {
            var line = Tools.CreateFileLine("#pragma include", 0);
            var rule = new PreprocessorRule();
            List<AsbtractCodeFragment> fragments = rule.Parse(line, new NullMessageHandler());

            Assert.AreEqual(1, fragments.Count);

            Assert.AreEqual(CodeFragmentTypes.OriginScript, fragments[0].Type);
            Assert.AreEqual("#pragma include", fragments[0].Span.Text);
        }
    }
}
