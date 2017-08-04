using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTests.Helper;
using Twofold.Compilation;
using Twofold.Interface.Compilation;
using System.Collections.Generic;
using Twofold.Compilation.Rules;

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
            List<AsbtractRenderCommand> fragments = rule.Parse(line, new NullMessageHandler());

            Assert.AreEqual(1, fragments.Count);

            Assert.IsInstanceOfType(fragments[0], typeof(ScriptCommand));
            Assert.AreEqual("#", (fragments[0] as ScriptCommand).Script.Text);
        }

        [TestMethod]
        public void UnknownDirective()
        {
            var line = Tools.CreateFileLine("#debug", 0);
            var rule = new PreprocessorRule();
            List<AsbtractRenderCommand> fragments = rule.Parse(line, new NullMessageHandler());

            Assert.AreEqual(1, fragments.Count);
            Assert.IsInstanceOfType(fragments[0], typeof(ScriptCommand));
            Assert.AreEqual("#debug", (fragments[0] as ScriptCommand).Script.Text);
        }

        [TestMethod]
        public void UnknownPragmaDirective()
        {
            var line = Tools.CreateFileLine("#pragma warning 120, off", 0);
            var rule = new PreprocessorRule();
            List<AsbtractRenderCommand> fragments = rule.Parse(line, new NullMessageHandler());

            Assert.AreEqual(1, fragments.Count);
            Assert.IsInstanceOfType(fragments[0], typeof(ScriptCommand));
            Assert.AreEqual("#pragma warning 120, off", (fragments[0] as ScriptCommand).Script.Text);
        }

        [TestMethod]
        public void IncludePragmaDirective()
        {
            var line = Tools.CreateFileLine("#pragma include \"File.cs\"", 0);
            var rule = new PreprocessorRule();
            List<AsbtractRenderCommand> fragments = rule.Parse(line, new NullMessageHandler());

            Assert.AreEqual(1, fragments.Count);
            Assert.IsInstanceOfType(fragments[0], typeof(PragmaCommand));
            Assert.AreEqual("#pragma include \"File.cs\"", (fragments[0] as PragmaCommand).Pragma.Text);
            Assert.AreEqual("include", (fragments[0] as PragmaCommand).Name);
            Assert.AreEqual("File.cs", (fragments[0] as PragmaCommand).Argument);
        }

        [TestMethod]
        public void EmptyIncludePragmaDirective()
        {
            var line = Tools.CreateFileLine("#pragma include \"\"", 0);
            var rule = new PreprocessorRule();
            List<AsbtractRenderCommand> fragments = rule.Parse(line, new NullMessageHandler());

            Assert.AreEqual(1, fragments.Count);
            Assert.IsInstanceOfType(fragments[0], typeof(PragmaCommand));
            Assert.AreEqual("#pragma include \"\"", (fragments[0] as PragmaCommand).Pragma.Text);
            Assert.AreEqual("include", (fragments[0] as PragmaCommand).Name);
            Assert.AreEqual("", (fragments[0] as PragmaCommand).Argument);
        }

        [TestMethod]
        public void IncludePragmaNoFileDirective()
        {
            var line = Tools.CreateFileLine("#pragma include", 0);
            var rule = new PreprocessorRule();
            List<AsbtractRenderCommand> fragments = rule.Parse(line, new NullMessageHandler());

            Assert.AreEqual(1, fragments.Count);
            Assert.IsInstanceOfType(fragments[0], typeof(ScriptCommand));
            Assert.AreEqual("#pragma include", (fragments[0] as ScriptCommand).Script.Text);
        }
    }
}
