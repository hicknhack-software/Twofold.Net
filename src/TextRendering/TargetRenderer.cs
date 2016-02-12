using System;
using System.IO;
using Twofold.Interface;

namespace Twofold.TextRendering
{
    /// <summary>
    /// This is the renderer which will be exposed to the generated
    /// CSharp target code.
    /// </summary>
    public static class TargetRenderer
    {
        static TextWriter textWriter = new StringWriter();
        static TextRenderer renderer = new TextRenderer();

        internal static void SetTextWriter(TextWriter textWriter)
        {
            if (textWriter == null) {
                throw new ArgumentNullException("textWriter");
            }
            TargetRenderer.textWriter = textWriter;
        }

        public static void Append(string text)
        {
            TargetRenderer.renderer.Append(new TextSpan(text), TargetRenderer.textWriter);
        }

        public static void NewLine()
        {
            TargetRenderer.renderer.AppendNewLine(TargetRenderer.textWriter);
        }

        public static void PushIndentation(string indentation)
        {
            TargetRenderer.renderer.PushIndentation(indentation);
        }

        public static void PopIndentation()
        {
            TargetRenderer.renderer.PopIndentation();
        }
    }
}
