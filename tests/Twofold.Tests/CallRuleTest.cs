namespace UnitTests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using UnitTests.Helper;
    using Twofold.Abstractions.Compilation;
    using System.Collections.Generic;
    using Twofold.Compilation.Rules;

    [TestClass]
    public class CallRuleTest
    {
        [TestMethod]
        public void Empty()
        {
            var line = Tools.CreateFileLine(@"=", 0);
            var rule = new CallRule();
            List<AsbtractRenderCommand> fragments = rule.Parse(line, new NullMessageHandler());

            Assert.AreEqual(3, fragments.Count);

            Assert.AreEqual(RenderCommandTypes.TargetPushIndentation, fragments[0].Type);
            Assert.AreEqual("", fragments[0].Span.Text);

            Assert.AreEqual(RenderCommandTypes.OriginScript, fragments[1].Type);
            Assert.AreEqual("", fragments[1].Span.Text);

            Assert.AreEqual(RenderCommandTypes.TargetPopIndentation, fragments[2].Type);
            Assert.AreEqual("", fragments[2].Span.Text);
        }

        [TestMethod]
        public void NoIndentation()
        {
            var line = Tools.CreateFileLine(@"=A;", 0);
            var rule = new CallRule();
            List<AsbtractRenderCommand> fragments = rule.Parse(line, new NullMessageHandler());

            Assert.AreEqual(3, fragments.Count);

            Assert.AreEqual(RenderCommandTypes.TargetPushIndentation, fragments[0].Type);
            Assert.AreEqual("", fragments[0].Span.Text);

            Assert.AreEqual(RenderCommandTypes.OriginScript, fragments[1].Type);
            Assert.AreEqual("A;", fragments[1].Span.Text);

            Assert.AreEqual(RenderCommandTypes.TargetPopIndentation, fragments[2].Type);
            Assert.AreEqual("", fragments[2].Span.Text);
        }

        [TestMethod]
        public void Indentation()
        {
            var line = Tools.CreateFileLine(@"=   A;", 0);
            var rule = new CallRule();
            List<AsbtractRenderCommand> fragments = rule.Parse(line, new NullMessageHandler());

            Assert.AreEqual(3, fragments.Count);

            Assert.AreEqual(RenderCommandTypes.TargetPushIndentation, fragments[0].Type);
            Assert.AreEqual("   ", fragments[0].Span.Text);

            Assert.AreEqual(RenderCommandTypes.OriginScript, fragments[1].Type);
            Assert.AreEqual("A;", fragments[1].Span.Text);

            Assert.AreEqual(RenderCommandTypes.TargetPopIndentation, fragments[2].Type);
            Assert.AreEqual("", fragments[2].Span.Text);
        }
    }
}
