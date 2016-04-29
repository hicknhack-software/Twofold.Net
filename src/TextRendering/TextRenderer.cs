/* Twofold.Net
 * (C) Copyright 2016 HicknHack Software GmbH
 *
 * The original code can be found at:
 *     https://github.com/hicknhack-software/Twofold.Net
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace Twofold.TextRendering
{
    using Extensions;
    using Interface;
    using System;
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// This class is used by the TargetRenderer to produce the rendererd
    /// output.
    /// </summary>
    internal class TextRenderer
    {
        // Fields
        private string newLine = Environment.NewLine;
        private bool lineBlank = true;
        private readonly Stack<Tuple<string, string>> indentationQueue = new Stack<Tuple<string, string>>();
        private string partIndentation = string.Empty;
        private readonly TextWriter textWriter;

        /// <exception cref="ArgumentNullException">textWriter is null.</exception>
        public TextRenderer(TextWriter textWriter)
            : this(textWriter, Environment.NewLine)
        { }

        /// <exception cref="ArgumentNullException">textWriter is null.</exception>
        /// <exception cref="ArgumentNullException">newLine is null.</exception>
        /// <exception cref="ArgumentException">newLine is not \r or \r\n.</exception>
        public TextRenderer(TextWriter textWriter, string newLine)
        {
            if(textWriter == null)
            {
                throw new ArgumentNullException(nameof(textWriter));
            }
            this.textWriter = textWriter;

            if (newLine == null)
            {
                throw new ArgumentNullException(nameof(newLine));
            }
            if (string.Compare(newLine, "\n", StringComparison.Ordinal) != 0 
                && string.Compare(newLine, "\r\n", StringComparison.Ordinal) != 0)
            {
                throw new ArgumentException("New line must either be \r or \r\n.", nameof(newLine));
            }
            this.newLine = newLine;
        }

        /// <summary>
        /// Indicates whether the current line is blank.
        /// </summary>
        public bool IsLineBlank
        {
            get { return lineBlank; }
        }

        /// <summary>
        /// Current line
        /// </summary>
        public int Line { get; private set; } = 1;

        /// <summary>
        /// Current column. 0 means invalid Column.
        /// </summary>
        public int Column { get; private set; } = 1;

        /// <summary>
        /// Append text.
        /// </summary>
        public void Append(TextSpan textSpan)
        {
            // Skip empty spans
            if (textSpan.IsEmpty)
            {
                return;
            }

            string indentation = (indentationQueue.Count > 0) ? indentationQueue.Peek().Item2 : "";

            var index = textSpan.Begin;
            while (index < textSpan.End)
            {
                if (lineBlank)
                {
                    this.textWriter.Write(indentation);
                    Column += indentation.Length;
                    lineBlank = false;
                }

                var lineBreakIndex = textSpan.OriginalText.IndexOf(index, textSpan.End, ch => ch == '\n');
                if (lineBreakIndex == textSpan.End)
                { // No line break found
                    this.textWriter.Write(index, textSpan.End, textSpan.OriginalText);
                    index += (textSpan.End - index);
                }
                else
                {  // Line break found
                    ++lineBreakIndex;
                    this.textWriter.Write(index, lineBreakIndex, textSpan.OriginalText);
                    index += (lineBreakIndex - index);

                    ++Line;
                    lineBlank = true;
                    Column = 1;
                }
            }
        }

        public void Append(string text)
        {
            var textSpan = new TextSpan(text);
            this.Append(textSpan);
        }

        /// <summary>
        /// Append the NewLine string.
        /// </summary>
        public void AppendNewLine()
        {
            this.textWriter.Write(newLine);
            ++Line;
            lineBlank = true;
            Column = 1;
        }

        /// <summary>
        /// Pushes an indentation level.
        /// </summary>
        /// <param name="indentation">The indentation text.</param>
        public void PushIndentation(string indentation)
        {
            if (indentation == null)
            {
                throw new ArgumentNullException(nameof(indentation));
            }

            string fullIndentation = indentation;
            if (indentationQueue.Count > 0)
            {
                fullIndentation = fullIndentation.Insert(0, indentationQueue.Peek().Item2);
            }
            indentationQueue.Push(Tuple.Create(indentation, fullIndentation));
        }

        /// <summary>
        /// Pops the current indentation level.
        /// </summary>
        public void PopIndentation()
        {
            indentationQueue.Pop();
        }

        public void PartIndentation(string indentation)
        {
            if (indentation == null)
            {
                throw new ArgumentNullException(nameof(indentation));
            }

            if (IsLineBlank)
            {
                partIndentation = indentation;
            }
            this.Append(indentation);
        }

        public void PushPartIndentation()
        {
            this.PushIndentation(partIndentation);
        }

        public void PopPartIndentation()
        {
            partIndentation = indentationQueue.Peek().Item1;
            this.PopIndentation();
        }
    }
}