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
        static readonly TextRenderer renderer = new TextRenderer();

        internal static void SetTextWriter(TextWriter textWriter)
        {
            if (textWriter == null) {
                throw new ArgumentNullException("textWriter");
            }
            TargetRenderer.textWriter = textWriter;
        }

        /// <summary>
        /// Executes the given function which must return a string and appends
        /// the returned string to the text buffer.
        /// </summary>
        /// <param name="func">The function to execute.</param>
        public static void Append(Func<string> func)
        {
            string text = func();
            var textSpan = new TextSpan(text);
            TargetRenderer.renderer.Append(textSpan, TargetRenderer.textWriter);
        }

        /// <summary>
        /// Executes the given function which must return void.
        /// </summary>
        /// <param name="action">The function to execute.</param>
        public static void Append(Action action)
        {
            action();
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

        public static void PartIndentation(string indentation)
        {
            TargetRenderer.renderer.PartIndentation(indentation, TargetRenderer.textWriter);
        }

        public static void PushPartIndentation()
        {
            TargetRenderer.renderer.PushPartIndentation();
        }

        public static void PopPartIndentation()
        {
            TargetRenderer.renderer.PopPartIndentation();
        }
    }
}
