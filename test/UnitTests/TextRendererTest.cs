using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using Twofold.Interface.SourceMapping;
using Twofold.TextRendering;

namespace UnitTests
{
    [TestClass]
    public class TextRendererTest
    {
        [TestMethod]
        public void WriteWithNewLine()
        {
            using (var writer = new StringWriter())
            {
                var textRenderer = new TextRenderer(writer, new Mapping(), "\n");
                textRenderer.PushIndentation("__", new TextFilePosition(), EntryFeatures.ColumnInterpolation);
                textRenderer.Write("ABC\nD", new TextFilePosition());

                var text = writer.ToString();
                Assert.AreEqual("__ABC\n__D", text);
            }
        }
    }
}
