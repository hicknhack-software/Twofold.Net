using System;
using System.IO;
using Twofold.Api;
using Twofold.Extensions;

namespace Twofold.Compilation.Generation
{
    public class TextWriterController
    {
        // Fields
        string indentation = "";
        string newLine = Environment.NewLine;
        bool currentLineEmpty = true;

        /// <summary>
        /// Current line
        /// </summary>
        public int Line { get; private set; } = 1;
        /// <summary>
        /// Current column. 0 means invalid Column.
        /// </summary>
        public int Column { get; private set; } = 1;
        /// <summary>
        /// String to use for indentation
        /// </summary>
        public string Indentation
        {
            get { return indentation; }
            set
            {
                if (value == null) {
                    throw new ArgumentNullException("Indentation");
                }
                indentation = value;
            }
        }
        /// <summary>
        /// New line signature either \n or \r\n.
        /// </summary>
        /// <exception cref="ArgumentNullException">NewLine is null.</exception>
        /// <exception cref="ArgumentException">NewLine is not \r or \r\n.</exception>
        public string NewLine
        {
            get { return newLine; }
            set
            {
                if (value == null) {
                    throw new ArgumentNullException("NewLine");
                }
                if (string.Compare(value, "\n") != 0 && string.Compare(value, "\r\n") != 0) {
                    throw new ArgumentException("New line must either be \r or \r\n.", "NewLine");
                }
                indentation = value;
            }
        }

        /// <summary>
        /// Append text.
        /// </summary>
        public void Append(TextSpan textSpan, TextWriter writer)
        {
            // Skip empty spans
            if (textSpan.IsEmpty) {
                return;
            }

            var index = textSpan.BeginIndex;
            while (index < textSpan.EndIndex) {
                if (currentLineEmpty) {
                    writer.Write(indentation);
                    Column += indentation.Length;
                    currentLineEmpty = false;
                }

                var lineBreakIndex = textSpan.OriginalText.IndexOf(index, textSpan.EndIndex, ch => ch == '\n');
                if (lineBreakIndex == -1) { // No line break found
                    writer.Write(index, textSpan.EndIndex, textSpan.OriginalText);
                    index += (textSpan.EndIndex - index);
                }
                else {  // Line break found
                    ++lineBreakIndex;
                    writer.Write(index, lineBreakIndex, textSpan.OriginalText);
                    index += (lineBreakIndex - index);

                    ++Line;
                    currentLineEmpty = true;
                    Column = 1;
                }
            }
        }

        /// <summary>
        /// Append the NewLine string.
        /// </summary>
        public void AppendNewLine(TextWriter writer)
        {
            writer.WriteAsync(NewLine);
            ++Line;
            currentLineEmpty = true;
            Column = 0;
        }
    }
}
