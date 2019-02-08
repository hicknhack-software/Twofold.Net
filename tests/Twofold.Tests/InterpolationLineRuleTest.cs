namespace UnitTests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Collections.Generic;
    using Twofold.Abstractions.Compilation;
    using UnitTests.Helper;
    using Twofold.Compilation.Rules;

    [TestClass]
    public class InterpolationLineRuleTest
    {
        [TestMethod]
        public void Empty()
        {
            var line = Tools.CreateFileLine(@"|", 0);
            var rule = new InterpolationLineRule();
            List<AsbtractRenderCommand> fragments = rule.Parse(line, new NullMessageHandler());

            Assert.AreEqual(3, fragments.Count);
            // ...
            // First elements are tested by InterpolationLine
            // ...
            Assert.AreEqual(RenderCommandTypes.TargetNewLine, fragments[2].Type);
            Assert.AreEqual("", fragments[2].Span.Text);
        }
    }
}
