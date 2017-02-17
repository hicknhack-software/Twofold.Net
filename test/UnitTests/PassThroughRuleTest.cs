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

            Assert.AreEqual(1, fragments.Count);

            Assert.AreEqual(RenderCommandTypes.OriginScript, fragments[0].Type);
            Assert.AreEqual("", fragments[0].Span.Text);
        }

        [TestMethod]
        public void Text()
        {
            var line = Tools.CreateFileLine(@"A B", 0);
            var rule = new PassThroughRule();
            List<AsbtractRenderCommand> fragments = rule.Parse(line, new NullMessageHandler());

            Assert.AreEqual(1, fragments.Count);

            Assert.AreEqual(RenderCommandTypes.OriginScript, fragments[0].Type);
            Assert.AreEqual("A B", fragments[0].Span.Text);
        }
    }
}
