using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTests.Helper;
using Twofold.Compilation;
using Twofold.Interface.Compilation;
using System.Collections.Generic;
using Twofold.Compilation.Rules;

namespace UnitTests
{
    [TestClass]
    public class PassThroughRuleTest
    {
        [TestMethod]
        public void Empty()
        {
            var line = Tools.CreateFileLine(@"", 0);
            var rule = new PassThroughRule();
            List<AsbtractRenderCommand> fragments = rule.Parse(line, new NullMessageHandler());

            Assert.AreEqual(0, fragments.Count);
        }

        [TestMethod]
        public void Text()
        {
            var line = Tools.CreateFileLine(@"A B", 0);
            var rule = new PassThroughRule();
            List<AsbtractRenderCommand> fragments = rule.Parse(line, new NullMessageHandler());

            Assert.AreEqual(1, fragments.Count);
            Assert.IsInstanceOfType(fragments[0], typeof(ScriptCommand));
            Assert.AreEqual("A B", (fragments[0] as ScriptCommand).Script.Text);
        }
    }
}
