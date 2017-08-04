using Microsoft.VisualStudio.TestTools.UnitTesting;
using Twofold.Interface;
using Twofold.Interface.SourceMapping;
using System.Collections.Generic;
using Twofold.Compilation;
using Twofold.Interface.Compilation;
using UnitTests.Helper;
using Twofold.Compilation.Rules;

namespace UnitTests
{
    [TestClass]
    public class InterpolationLineRuleTest
    {
        [TestMethod]
        public void Empty()
        {
            var line = Tools.CreateFileLine(@"|", 0);
            var rule = new InterpolationLineRule();
            List<AsbtractRenderCommand> fragments = rule.Parse(line, new NullMessageHandler());

            Assert.AreEqual(1, fragments.Count);
            Assert.AreEqual(RenderCommands.NewLine, fragments[0].Type);
            Assert.AreEqual("|", (fragments[0] as NewLineCommand).NewLine.Text);
        }
    }
}
