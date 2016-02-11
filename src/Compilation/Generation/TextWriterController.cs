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
                if (lineBreakIndex == textSpan.EndIndex) { // No line break found
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
            writer.Write(NewLine);
            ++Line;
            currentLineEmpty = true;
            Column = 1;
        }
    }
}
