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
using System;
using System.Collections.Generic;
using System.IO;
using Twofold.Extensions;
using Twofold.Interface;

namespace Twofold.TextRendering
{
    internal class TextRenderer
    {
        // Fields
        string newLine = Environment.NewLine;
        bool lineBlank = true;
        readonly Stack<Tuple<string, string>> indentationQueue = new Stack<Tuple<string, string>>();
        string partIndentation = "";

        /// <summary>
        /// Indicates whether the current line is blank.
        /// </summary>
        public bool IsLineBlank { get { return lineBlank; } }

        /// <summary>
        /// Current line
        /// </summary>
        public int Line { get; private set; } = 1;

        /// <summary>
        /// Current column. 0 means invalid Column.
        /// </summary>
        public int Column { get; private set; } = 1;

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
                newLine = value;
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

            string indentation = (indentationQueue.Count > 0) ? indentationQueue.Peek().Item2 : "";

            var index = textSpan.Begin;
            while (index < textSpan.End) {
                if (lineBlank) {
                    writer.Write(indentation);
                    Column += indentation.Length;
                    lineBlank = false;
                }

                var lineBreakIndex = textSpan.OriginalText.IndexOf(index, textSpan.End, ch => ch == '\n');
                if (lineBreakIndex == textSpan.End) { // No line break found
                    writer.Write(index, textSpan.End, textSpan.OriginalText);
                    index += (textSpan.End - index);
                }
                else {  // Line break found
                    ++lineBreakIndex;
                    writer.Write(index, lineBreakIndex, textSpan.OriginalText);
                    index += (lineBreakIndex - index);

                    ++Line;
                    lineBlank = true;
                    Column = 1;
                }
            }
        }

        /// <summary>
        /// Append the NewLine string.
        /// </summary>
        public void AppendNewLine(TextWriter writer)
        {
            writer.Write(NewLine);
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
            if (indentation == null) {
                throw new ArgumentNullException("indentation");
            }

            string fullIndentation = indentation;
            if (indentationQueue.Count > 0) {
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

        public void PartIndentation(string indentation, TextWriter writer)
        {
            if (indentation == null) {
                throw new ArgumentNullException("indentation");
            }

            if (IsLineBlank) {
                partIndentation = indentation;
            }
            this.Append(new TextSpan(indentation), writer);
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
