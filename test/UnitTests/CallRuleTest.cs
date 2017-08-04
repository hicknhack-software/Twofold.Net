using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTests.Helper;
using Twofold.Compilation;
using Twofold.Interface.Compilation;
using System.Collections.Generic;
using Twofold.Compilation.Rules;

namespace UnitTests
{
    [TestClass]
    public class CallRuleTest
    {
        [TestMethod]
        public void Empty()
        {
            var line = Tools.CreateFileLine(@"=", 0);
            var rule = new CallRule();
            List<AsbtractRenderCommand> fragments = rule.Parse(line, new NullMessageHandler());

            Assert.AreEqual(0, fragments.Count);
        }

        [TestMethod]
        public void NoIndentation()
        {
            var line = Tools.CreateFileLine(@"=A;", 0);
            var rule = new CallRule();
            List<AsbtractRenderCommand> fragments = rule.Parse(line, new NullMessageHandler());

            Assert.AreEqual(1, fragments.Count);

            Assert.IsInstanceOfType(fragments[0], typeof(StatementCommand));
            Assert.AreEqual("A;", (fragments[0] as StatementCommand).Statement.Text);
        }

        [TestMethod]
        public void Indentation()
        {
            var line = Tools.CreateFileLine(@"=   A;", 0);
            var rule = new CallRule();
            List<AsbtractRenderCommand> fragments = rule.Parse(line, new NullMessageHandler());

            Assert.AreEqual(3, fragments.Count);

            Assert.IsInstanceOfType(fragments[0], typeof(PushIndentationCommand));
            Assert.AreEqual("   ", (fragments[0] as PushIndentationCommand).Indentation.Text);

            Assert.IsInstanceOfType(fragments[1], typeof(StatementCommand));
            Assert.AreEqual("A;", (fragments[1] as StatementCommand).Statement.Text);

            Assert.IsInstanceOfType(fragments[2], typeof(PopIndentationCommand));
            Assert.AreEqual("", (fragments[2] as PopIndentationCommand).Pop.Text);
        }
    }
}
