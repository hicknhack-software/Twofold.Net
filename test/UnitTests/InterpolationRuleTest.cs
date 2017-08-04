using Microsoft.VisualStudio.TestTools.UnitTesting;
using Twofold.Compilation;
using System.Collections.Generic;
using Twofold.Interface.Compilation;
using UnitTests.Helper;
using Twofold.Compilation.Rules;

namespace UnitTests
{
    [TestClass]
    public class InterpolationRuleTest
    {
        [TestMethod]
        public void Empty()
        {
            var line = Tools.CreateFileLine(@"\", 0);
            var rule = new InterpolationRule();
            List<AsbtractRenderCommand> fragments = rule.Parse(line, new NullMessageHandler());

            Assert.AreEqual(0, fragments.Count);
        }

        [TestMethod]
        public void EmptyIndentation()
        {
            var line = Tools.CreateFileLine(@"\   ", 0);
            var rule = new InterpolationRule();
            List<AsbtractRenderCommand> fragments = rule.Parse(line, new NullMessageHandler());

            Assert.AreEqual(2, fragments.Count);

            Assert.IsInstanceOfType(fragments[0], typeof(PushIndentationCommand));
            Assert.AreEqual("   ", (fragments[0] as PushIndentationCommand).Indentation.Text);

            Assert.IsInstanceOfType(fragments[1], typeof(PopIndentationCommand));
            Assert.AreEqual("", (fragments[1] as PopIndentationCommand).Pop.Text);
        }

        [TestMethod]
        public void Text()
        {
            var line = Tools.CreateFileLine(@"\A", 0);
            var rule = new InterpolationRule();
            List<AsbtractRenderCommand> fragments = rule.Parse(line, new NullMessageHandler());

            Assert.AreEqual(1, fragments.Count);

            Assert.IsInstanceOfType(fragments[0], typeof(TextCommand));
            Assert.AreEqual("A", (fragments[0] as TextCommand).Text.Text);
        }

        [TestMethod]
        public void IndentationText()
        {
            var line = Tools.CreateFileLine(@"\   A", 0);
            var rule = new InterpolationRule();
            List<AsbtractRenderCommand> fragments = rule.Parse(line, new NullMessageHandler());

            Assert.AreEqual(3, fragments.Count);

            Assert.IsInstanceOfType(fragments[0], typeof(PushIndentationCommand));
            Assert.AreEqual("   ", (fragments[0] as PushIndentationCommand).Indentation.Text);

            Assert.IsInstanceOfType(fragments[1], typeof(TextCommand));
            Assert.AreEqual("A", (fragments[1] as TextCommand).Text.Text);

            Assert.IsInstanceOfType(fragments[2], typeof(PopIndentationCommand));
            Assert.AreEqual("", (fragments[2] as PopIndentationCommand).Pop.Text);
        }

        [TestMethod]
        public void Expression()
        {
            var line = Tools.CreateFileLine(@"\#{A}", 0);
            var rule = new InterpolationRule();
            List<AsbtractRenderCommand> fragments = rule.Parse(line, new NullMessageHandler());

            Assert.AreEqual(1, fragments.Count);

            Assert.IsInstanceOfType(fragments[0], typeof(ExpressionCommand));
            Assert.AreEqual("A", (fragments[0] as ExpressionCommand).Expression.Text);
        }

        [TestMethod]
        public void TextAndExpression()
        {
            var line = Tools.CreateFileLine(@"\A#{B}", 0);
            var rule = new InterpolationRule();
            List<AsbtractRenderCommand> fragments = rule.Parse(line, new NullMessageHandler());

            Assert.AreEqual(2, fragments.Count);

            Assert.IsInstanceOfType(fragments[0], typeof(TextCommand));
            Assert.AreEqual("A", (fragments[0] as TextCommand).Text.Text);

            Assert.IsInstanceOfType(fragments[1], typeof(ExpressionCommand));
            Assert.AreEqual("B", (fragments[1] as ExpressionCommand).Expression.Text);
        }

        [TestMethod]
        public void TextAndExpressionAndText()
        {
            var line = Tools.CreateFileLine(@"\A#{B}C", 0);
            var rule = new InterpolationRule();
            List<AsbtractRenderCommand> fragments = rule.Parse(line, new NullMessageHandler());

            Assert.AreEqual(3, fragments.Count);

            Assert.IsInstanceOfType(fragments[0], typeof(TextCommand));
            Assert.AreEqual("A", (fragments[0] as TextCommand).Text.Text);

            Assert.IsInstanceOfType(fragments[1], typeof(ExpressionCommand));
            Assert.AreEqual("B", (fragments[1] as ExpressionCommand).Expression.Text);

            Assert.IsInstanceOfType(fragments[2], typeof(TextCommand));
            Assert.AreEqual("C", (fragments[2] as TextCommand).Text.Text);
        }

        [TestMethod]
        public void TextAndExpressionInterleaved()
        {
            var line = Tools.CreateFileLine(@"\A#{B}C#{D}E", 0);
            var rule = new InterpolationRule();
            List<AsbtractRenderCommand> fragments = rule.Parse(line, new NullMessageHandler());

            Assert.AreEqual(5, fragments.Count);

            Assert.IsInstanceOfType(fragments[0], typeof(TextCommand));
            Assert.AreEqual("A", (fragments[0] as TextCommand).Text.Text);

            Assert.IsInstanceOfType(fragments[1], typeof(ExpressionCommand));
            Assert.AreEqual("B", (fragments[1] as ExpressionCommand).Expression.Text);

            Assert.IsInstanceOfType(fragments[2], typeof(TextCommand));
            Assert.AreEqual("C", (fragments[2] as TextCommand).Text.Text);

            Assert.IsInstanceOfType(fragments[3], typeof(ExpressionCommand));
            Assert.AreEqual("D", (fragments[3] as ExpressionCommand).Expression.Text);

            Assert.IsInstanceOfType(fragments[4], typeof(TextCommand));
            Assert.AreEqual("E", (fragments[4] as TextCommand).Text.Text);
        }

        [TestMethod]
        public void EscapedSharp()
        {
            var line = Tools.CreateFileLine(@"\##include #{A}", 0);
            var rule = new InterpolationRule();
            List<AsbtractRenderCommand> fragments = rule.Parse(line, new NullMessageHandler());

            Assert.AreEqual(3, fragments.Count);

            Assert.IsInstanceOfType(fragments[0], typeof(TextCommand));
            Assert.AreEqual("#", (fragments[0] as TextCommand).Text.Text);

            Assert.IsInstanceOfType(fragments[1], typeof(TextCommand));
            Assert.AreEqual("include ", (fragments[1] as TextCommand).Text.Text);

            Assert.IsInstanceOfType(fragments[2], typeof(ExpressionCommand));
            Assert.AreEqual("A", (fragments[2] as ExpressionCommand).Expression.Text);
        }
    }
}