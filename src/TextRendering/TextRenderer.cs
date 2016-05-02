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
    using Interface.SourceMapping;
    using System;
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// This class is used by the TargetRenderer to produce the rendererd
    /// output.
    /// </summary>
    internal class TextRenderer
    {
        class IndentationItem
        {
            public readonly string Indentation;
            public readonly TextFilePosition Source;
            public readonly int CallerIndex;

            public IndentationItem(string indentation, TextFilePosition source, int callerIndex)
            {
                if (indentation == null)
                {
                    throw new ArgumentNullException(nameof(indentation));
                }
                this.Indentation = indentation;

                if (source == null)
                {
                    throw new ArgumentNullException(nameof(source));
                }
                this.Source = source;

                this.CallerIndex = callerIndex;
            }
        }

        // Fields
        private string NewLine = Environment.NewLine;
        private readonly Stack<IndentationItem> IndentationStack;
        private readonly Stack<int> CallerIndexStack;
        private IndentationItem partIndentation;
        private readonly TextWriter TextWriter;
        private readonly SourceMap SourceMap;

        /// <exception cref="ArgumentNullException">textWriter is null.</exception>
        public TextRenderer(TextWriter textWriter, SourceMap sourceMap)
            : this(textWriter, sourceMap, Environment.NewLine)
        { }

        /// <exception cref="ArgumentNullException">textWriter is null.</exception>
        /// <exception cref="ArgumentNullException">newLine is null.</exception>
        /// <exception cref="ArgumentException">newLine is not \r or \r\n.</exception>
        public TextRenderer(TextWriter textWriter, SourceMap sourceMap, string newLine)
        {
            if (textWriter == null)
            {
                throw new ArgumentNullException(nameof(textWriter));
            }
            this.TextWriter = textWriter;

            if (sourceMap == null)
            {
                throw new ArgumentNullException(nameof(sourceMap));
            }
            this.SourceMap = sourceMap;

            if (newLine == null)
            {
                throw new ArgumentNullException(nameof(newLine));
            }
            if (string.Compare(newLine, "\n", StringComparison.Ordinal) != 0
                && string.Compare(newLine, "\r\n", StringComparison.Ordinal) != 0)
            {
                throw new ArgumentException("New line must either be \r or \r\n.", nameof(newLine));
            }
            this.NewLine = newLine;

            this.IndentationStack = new Stack<IndentationItem>();
            this.CallerIndexStack = new Stack<int>();
        }

        /// <summary>
        /// Indicates whether the current line is blank.
        /// </summary>
        public bool IsLineBlank
        {
            get; private set;
        } = true;

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
        public void Write(TextSpan textSpan)
        {
            this.Write(textSpan, new TextFilePosition());
        }

        public void Write(TextSpan textSpan, TextFilePosition source)
        {
            // Skip empty spans
            if (textSpan.IsEmpty)
            {
                return;
            }

            var index = textSpan.Begin;
            while (index < textSpan.End)
            {
                if (this.IsLineBlank)
                {
                    foreach (var indentationItem in this.IndentationStack)
                    {
                        if (indentationItem.Indentation.Length > 0)
                        {
                            var mapping = new SourceMap.Mapping(new TextPosition(this.Line, this.Column), indentationItem.Source, indentationItem.CallerIndex);
                            this.SourceMap.AddMapping(mapping);
                            this.TextWriter.Write(indentationItem.Indentation);
                            this.Column += indentationItem.Indentation.Length;
                        }
                    }
                    this.IsLineBlank = false;
                }

                var callerIndex = (this.CallerIndexStack.Count == 0) ? -1 : this.CallerIndexStack.Peek();
                var lineBreakIndex = textSpan.OriginalText.IndexOf(index, textSpan.End, ch => ch == '\n');
                if (lineBreakIndex == textSpan.End)
                { // No line break found
                    var len = (textSpan.End - index);
                    if (source.IsValid && (len > 0))
                    {
                        var mapping = new SourceMap.Mapping(new TextPosition(this.Line, this.Column), source, callerIndex);
                        this.SourceMap.AddMapping(mapping);
                    }
                    this.TextWriter.Write(index, textSpan.End, textSpan.OriginalText);
                    Column += len;
                    index += len;
                }
                else
                {  // Line break found
                    ++lineBreakIndex;
                    var len = (lineBreakIndex - index);
                    if (source.IsValid && (len > 0))
                    {
                        var mapping = new SourceMap.Mapping(new TextPosition(this.Line, this.Column), source, callerIndex);
                        this.SourceMap.AddMapping(mapping);
                    }
                    this.TextWriter.Write(index, lineBreakIndex, textSpan.OriginalText);
                    index += len;
                    ++Line;
                    this.IsLineBlank = true;
                    this.Column = 1;
                }
            }
        }

        public void Write(string text)
        {
            this.Write(text, new TextFilePosition());
        }

        public void Write(string text, TextFilePosition source)
        {
            var textSpan = new TextSpan(text);
            this.Write(textSpan, source);
        }

        public void WriteLine(TextSpan textSpan)
        {
            this.WriteLine(textSpan, new TextFilePosition());
        }

        public void WriteLine(TextSpan textSpan, TextFilePosition source)
        {
            this.Write(textSpan, source);
            this.WriteLine();
        }

        public void WriteLine(string text)
        {
            this.WriteLine(text, new TextFilePosition());
        }

        public void WriteLine(string text, TextFilePosition source)
        {
            this.Write(text, source);
            this.WriteLine();
        }

        /// <summary>
        /// Append the NewLine string.
        /// </summary>
        public void WriteLine()
        {
            this.WriteLine(new TextFilePosition());
        }

        public void WriteLine(TextFilePosition source)
        {
            if(source.IsValid)
            {
                var callerIndex = (this.CallerIndexStack.Count == 0) ? -1 : this.CallerIndexStack.Peek();
                var mapping = new SourceMap.Mapping(new TextPosition(this.Line, this.Column), source, callerIndex);
                this.SourceMap.AddMapping(mapping);
            }
            this.TextWriter.Write(this.NewLine);
            ++Line;
            this.IsLineBlank = true;
            this.Column = 1;
        }

        /// <summary>
        /// Pushes an indentation level.
        /// </summary>
        /// <param name="indentation">The indentation text.</param>
        public void PushIndentation(string indentation, TextFilePosition source)
        {
            if (indentation == null)
            {
                throw new ArgumentNullException(nameof(indentation));
            }

            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            var callerIndex = (this.CallerIndexStack.Count == 0) ? -1 : this.CallerIndexStack.Peek();
            this.IndentationStack.Push(new IndentationItem(indentation, source, callerIndex));
        }

        /// <summary>
        /// Pops the current indentation level.
        /// </summary>
        public void PopIndentation()
        {
            this.IndentationStack.Pop();
        }

        public void LocalIndentation(string indentation, TextFilePosition source)
        {
            if (indentation == null)
            {
                throw new ArgumentNullException(nameof(indentation));
            }

            if (IsLineBlank)
            {
                var callerIndex = (this.CallerIndexStack.Count == 0) ? -1 : this.CallerIndexStack.Peek();
                partIndentation = new IndentationItem(indentation, source, callerIndex);
            }
            this.Write(indentation, source);
        }

        public void PushLocalIndentation()
        {
            if (partIndentation != null)
            {
                this.PushIndentation(partIndentation.Indentation, partIndentation.Source);
            }
        }

        public void PopLocalIndentation()
        {
            partIndentation = null;
            if (this.IndentationStack.Count > 0)
            {
                partIndentation = this.IndentationStack.Peek();
                this.PopIndentation();
            }
        }

        public void PushCaller(TextFilePosition source)
        {
            var parentIndex = (this.CallerIndexStack.Count == 0) ? -1 : this.CallerIndexStack.Peek();
            int callerIndex = this.SourceMap.AddCaller(source, parentIndex);
            this.CallerIndexStack.Push(callerIndex);
        }

        public void PopCaller()
        {
            this.CallerIndexStack.Pop();
        }
    }
}