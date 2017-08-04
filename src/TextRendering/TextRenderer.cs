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
    using Interface.SourceMapping;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// This class is used by the TargetRenderer to produce the rendererd
    /// output.
    /// </summary>
    internal class TextRenderer
    {
        private class IndentationItem
        {
            public readonly string Indentation;
            public readonly TextFilePosition Original;
            public readonly int CallerIndex;
            public readonly EntryFeatures Features;

            public IndentationItem(string indentation, TextFilePosition original, int callerIndex, EntryFeatures features)
            {
                if (indentation == null)
                {
                    throw new ArgumentNullException(nameof(indentation));
                }
                this.Indentation = indentation;

                if (original == null)
                {
                    throw new ArgumentNullException(nameof(original));
                }
                this.Original = original;

                this.CallerIndex = callerIndex;
                this.Features = features;
            }
        }

        // Fields
        private readonly Stack<IndentationItem> IndentationStack;
        private readonly Stack<bool> PopIndendationStack;

        private readonly Stack<int> CallerIndexStack;
        //private IndentationItem partIndentation;
        private readonly TextWriter TextWriter;
        private readonly Mapping Mapping;

        /// <exception cref="ArgumentNullException">textWriter is null.</exception>
        public TextRenderer(TextWriter textWriter, Mapping mapping)
            : this(textWriter, mapping, Environment.NewLine)
        { }

        /// <exception cref="ArgumentNullException">textWriter is null.</exception>
        /// <exception cref="ArgumentNullException">newLine is null.</exception>
        /// <exception cref="ArgumentException">newLine is not \r or \r\n.</exception>
        public TextRenderer(TextWriter textWriter, Mapping mapping, string newLine)
        {
            if (textWriter == null)
            {
                throw new ArgumentNullException(nameof(textWriter));
            }
            this.TextWriter = textWriter;

            if (mapping == null)
            {
                throw new ArgumentNullException(nameof(mapping));
            }
            this.Mapping = mapping;

            if (newLine == null)
            {
                throw new ArgumentNullException(nameof(newLine));
            }
            if (string.Compare(newLine, "\n", StringComparison.Ordinal) != 0
                && string.Compare(newLine, "\r\n", StringComparison.Ordinal) != 0)
            {
                throw new ArgumentException("New line must either be \r or \r\n.", nameof(newLine));
            }
            this.TextWriter.NewLine = newLine;

            this.IndentationStack = new Stack<IndentationItem>();
            this.PopIndendationStack = new Stack<bool>();
            this.CallerIndexStack = new Stack<int>();
            this.ResetPosition();
        }

        /// <summary>
        /// Indicates whether the current line is blank.
        /// </summary>
        public bool IsLineBlank
        {
            get; private set;
        }

        /// <summary>
        /// Current line
        /// </summary>
        public int Line
        {
            get; private set;
        }

        /// <summary>
        /// Current column. 0 means invalid Column.
        /// </summary>
        public int Column
        {
            get; private set;
        }

        public void Write(string text, TextFilePosition original, EntryFeatures features = EntryFeatures.None)
        {
            // Skip empty spans
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            var index = 0;
            while (index < text.Length)
            {
                if (this.IsLineBlank)
                {
                    foreach (var indentationItem in this.IndentationStack.Reverse())
                    {
                        if (indentationItem.Indentation.Length > 0)
                        {
                            var entry = new MappingEntry(this.Position(), indentationItem.Original, indentationItem.CallerIndex, indentationItem.Features);
                            this.Mapping.Add(entry);
                            this.TextWriter.Write(indentationItem.Indentation);
                            this.Column += indentationItem.Indentation.Length;
                        }
                    }
                    this.IsLineBlank = false;
                }

                var callerIndex = (this.CallerIndexStack.Count == 0) ? -1 : this.CallerIndexStack.Peek();
                var lineBreakIndex = text.IndexOf('\n', index);
                if (lineBreakIndex == -1)
                { // No line break found
                    var len = (text.Length - index);
                    if (original.IsValid && (len > 0))
                    {
                        var entry = new MappingEntry(this.Position(), original, callerIndex, features);
                        this.Mapping.Add(entry);
                    }
                    this.TextWriter.Write(text.Substring(index, len));
                    this.Column += len;
                    index += len;
                }
                else
                {  // Line break found
                    ++lineBreakIndex;
                    var len = (lineBreakIndex - index);
                    if (original.IsValid && (len > 0))
                    {
                        var entry = new MappingEntry(this.Position(), original, callerIndex, features);
                        this.Mapping.Add(entry);
                    }
                    this.TextWriter.Write(text.Substring(index, len));
                    index += len;
                    ++this.Line;
                    this.IsLineBlank = true;
                    this.Column = 1;
                }
            }
        }

        public void WriteLine(TextFilePosition original)
        {
            this.TextWriter.WriteLine();
            this.IsLineBlank = true;
            ++this.Line;
            this.Column = 1;
            if (original.IsValid)
            {
                var callerIndex = (this.CallerIndexStack.Count == 0) ? -1 : this.CallerIndexStack.Peek();
                var entry = new MappingEntry(this.Position(), original, callerIndex, EntryFeatures.None);
                this.Mapping.Add(entry);
            }
        }

        /// <summary>
        /// Pushes an indentation level.
        /// </summary>
        /// <param name="indentation">The indentation text.</param>
        public void PushIndentation(string indentation, TextFilePosition original, EntryFeatures features = EntryFeatures.None)
        {
            if (indentation == null)
            {
                throw new ArgumentNullException(nameof(indentation));
            }

            if (original == null)
            {
                throw new ArgumentNullException(nameof(original));
            }

            if (this.IsLineBlank)
            {
                var callerIndex = (this.CallerIndexStack.Count == 0) ? -1 : this.CallerIndexStack.Peek();
                this.IndentationStack.Push(new IndentationItem(indentation, original, callerIndex, features));
                this.PopIndendationStack.Push(true);
            }
            else
            {
                // Compensation for InterpolationRule Indentation in case current line isn't blank anymore.
                // Uses helper stack to track if the current indentation must be really popped.
                this.Write(indentation, original, EntryFeatures.ColumnInterpolation);
                this.PopIndendationStack.Push(false);
            }
        }

        /// <summary>
        /// Pops the current indentation level.
        /// </summary>
        public void PopIndentation()
        {
            if (this.PopIndendationStack.Pop())
            {
                this.IndentationStack.Pop();
            }
        }

        public void PushCaller(TextFilePosition original)
        {
            var parentIndex = (this.CallerIndexStack.Count == 0) ? -1 : this.CallerIndexStack.Peek();

            int callerIndex = this.Mapping.AddCaller(original, parentIndex);
            this.CallerIndexStack.Push(callerIndex);
        }

        public void PopCaller()
        {
            this.CallerIndexStack.Pop();
        }

        public void ResetPosition()
        {
            this.IsLineBlank = true;
            this.Line = 1;
            this.Column = 1;
        }

        private TextPosition Position() => new TextPosition(this.Line, this.Column);
    }
}