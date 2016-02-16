using Microsoft.VisualStudio.TestTools.UnitTesting;
using Twofold.Compilation;
using System.Collections.Generic;
using Twofold.Interface.Compilation;
using UnitTests.Helper;

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
            List<AsbtractCodeFragment> fragments = rule.Parse(line, new NullMessageHandler());

            Assert.AreEqual(2, fragments.Count);
            Assert.AreEqual(CodeFragmentTypes.TargetIndentation, fragments[0].Type);
            Assert.AreEqual("", fragments[0].Span.Text);

            Assert.AreEqual(CodeFragmentTypes.OriginText, fragments[1].Type);
            Assert.AreEqual("", fragments[1].Span.Text);
        }

        [TestMethod]
        public void EmptyIndentation()
        {
            var line = Tools.CreateFileLine(@"\   ", 0);
            var rule = new InterpolationRule();
            List<AsbtractCodeFragment> fragments = rule.Parse(line, new NullMessageHandler());

            Assert.AreEqual(2, fragments.Count);
            Assert.AreEqual(CodeFragmentTypes.TargetIndentation, fragments[0].Type);
            Assert.AreEqual("   ", fragments[0].Span.Text);

            Assert.AreEqual(CodeFragmentTypes.OriginText, fragments[1].Type);
            Assert.AreEqual("", fragments[1].Span.Text);
        }

        [TestMethod]
        public void Text()
        {
            var line = Tools.CreateFileLine(@"\A", 0);
            var rule = new InterpolationRule();
            List<AsbtractCodeFragment> fragments = rule.Parse(line, new NullMessageHandler());

            Assert.AreEqual(2, fragments.Count);
            Assert.AreEqual(CodeFragmentTypes.TargetIndentation, fragments[0].Type);
            Assert.AreEqual("", fragments[0].Span.Text);

            Assert.AreEqual(CodeFragmentTypes.OriginText, fragments[1].Type);
            Assert.AreEqual("A", fragments[1].Span.Text);
        }

        [TestMethod]
        public void IndentationText()
        {
            var line = Tools.CreateFileLine(@"\   A", 0);
            var rule = new InterpolationRule();
            List<AsbtractCodeFragment> fragments = rule.Parse(line, new NullMessageHandler());

            Assert.AreEqual(2, fragments.Count);
            Assert.AreEqual(CodeFragmentTypes.TargetIndentation, fragments[0].Type);
            Assert.AreEqual("   ", fragments[0].Span.Text);

            Assert.AreEqual(CodeFragmentTypes.OriginText, fragments[1].Type);
            Assert.AreEqual("A", fragments[1].Span.Text);
        }

        [TestMethod]
        public void Expression()
        {
            var line = Tools.CreateFileLine(@"\#{A}", 0);
            var rule = new InterpolationRule();
            List<AsbtractCodeFragment> fragments = rule.Parse(line, new NullMessageHandler());

            Assert.AreEqual(4, fragments.Count);
            Assert.AreEqual(CodeFragmentTypes.TargetIndentation, fragments[0].Type);
            Assert.AreEqual("", fragments[0].Span.Text);

            Assert.AreEqual(CodeFragmentTypes.OriginText, fragments[1].Type);
            Assert.AreEqual("", fragments[1].Span.Text);

            Assert.AreEqual(CodeFragmentTypes.OriginExpression, fragments[2].Type);
            Assert.AreEqual("A", fragments[2].Span.Text);

            Assert.AreEqual(CodeFragmentTypes.OriginText, fragments[3].Type);
            Assert.AreEqual("", fragments[3].Span.Text);
        }

        [TestMethod]
        public void TextAndExpression()
        {
            var line = Tools.CreateFileLine(@"\A#{B}", 0);
            var rule = new InterpolationRule();
            List<AsbtractCodeFragment> fragments = rule.Parse(line, new NullMessageHandler());

            Assert.AreEqual(4, fragments.Count);
            Assert.AreEqual(CodeFragmentTypes.TargetIndentation, fragments[0].Type);
            Assert.AreEqual("", fragments[0].Span.Text);

            Assert.AreEqual(CodeFragmentTypes.OriginText, fragments[1].Type);
            Assert.AreEqual("A", fragments[1].Span.Text);

            Assert.AreEqual(CodeFragmentTypes.OriginExpression, fragments[2].Type);
            Assert.AreEqual("B", fragments[2].Span.Text);

            Assert.AreEqual(CodeFragmentTypes.OriginText, fragments[3].Type);
            Assert.AreEqual("", fragments[3].Span.Text);
        }

        [TestMethod]
        public void TextAndExpressionAndText()
        {
            var line = Tools.CreateFileLine(@"\A#{B}C", 0);
            var rule = new InterpolationRule();
            List<AsbtractCodeFragment> fragments = rule.Parse(line, new NullMessageHandler());

            Assert.AreEqual(4, fragments.Count);
            Assert.AreEqual(CodeFragmentTypes.TargetIndentation, fragments[0].Type);
            Assert.AreEqual("", fragments[0].Span.Text);

            Assert.AreEqual(CodeFragmentTypes.OriginText, fragments[1].Type);
            Assert.AreEqual("A", fragments[1].Span.Text);

            Assert.AreEqual(CodeFragmentTypes.OriginExpression, fragments[2].Type);
            Assert.AreEqual("B", fragments[2].Span.Text);

            Assert.AreEqual(CodeFragmentTypes.OriginText, fragments[3].Type);
            Assert.AreEqual("C", fragments[3].Span.Text);
        }

        [TestMethod]
        public void TextAndExpressionInterleaved()
        {
            var line = Tools.CreateFileLine(@"\A#{B}C#{D}E", 0);
            var rule = new InterpolationRule();
            List<AsbtractCodeFragment> fragments = rule.Parse(line, new NullMessageHandler());

            Assert.AreEqual(6, fragments.Count);
            Assert.AreEqual(CodeFragmentTypes.TargetIndentation, fragments[0].Type);
            Assert.AreEqual("", fragments[0].Span.Text);

            Assert.AreEqual(CodeFragmentTypes.OriginText, fragments[1].Type);
            Assert.AreEqual("A", fragments[1].Span.Text);

            Assert.AreEqual(CodeFragmentTypes.OriginExpression, fragments[2].Type);
            Assert.AreEqual("B", fragments[2].Span.Text);

            Assert.AreEqual(CodeFragmentTypes.OriginText, fragments[3].Type);
            Assert.AreEqual("C", fragments[3].Span.Text);

            Assert.AreEqual(CodeFragmentTypes.OriginExpression, fragments[4].Type);
            Assert.AreEqual("D", fragments[4].Span.Text);

            Assert.AreEqual(CodeFragmentTypes.OriginText, fragments[5].Type);
            Assert.AreEqual("E", fragments[5].Span.Text);
        }

        [TestMethod]
        public void EscapedSharp()
        {
            var line = Tools.CreateFileLine(@"\##include #{A}", 0);
            var rule = new InterpolationRule();
            List<AsbtractCodeFragment> fragments = rule.Parse(line, new NullMessageHandler());

            Assert.AreEqual(5, fragments.Count);
            Assert.AreEqual(CodeFragmentTypes.TargetIndentation, fragments[0].Type);
            Assert.AreEqual("", fragments[0].Span.Text);

            Assert.AreEqual(CodeFragmentTypes.OriginText, fragments[1].Type);
            Assert.AreEqual("#", fragments[1].Span.Text);

            Assert.AreEqual(CodeFragmentTypes.OriginText, fragments[2].Type);
            Assert.AreEqual("include ", fragments[2].Span.Text);

            Assert.AreEqual(CodeFragmentTypes.OriginExpression, fragments[3].Type);
            Assert.AreEqual("A", fragments[3].Span.Text);

            Assert.AreEqual(CodeFragmentTypes.OriginText, fragments[4].Type);
            Assert.AreEqual("", fragments[4].Span.Text);
        }
    }
}