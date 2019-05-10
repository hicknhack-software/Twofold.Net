﻿/* Twofold.Net
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

namespace HicknHack.Twofold.Abstractions
{
    using SourceMapping;
    using System;

    public struct FileLine
    {
        public readonly string Text;
        public readonly int Begin;
        public readonly int BeginNonSpace;
        public readonly int End;
        public readonly TextFilePosition Position;

        public FileLine(string text, int begin, int beginNonSpace, int end, TextFilePosition position)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }
            if (begin > beginNonSpace)
            {
                throw new ArgumentOutOfRangeException(nameof(begin), $"Must be less equal than {nameof(beginNonSpace)}.");
            }
            if (beginNonSpace > end)
            {
                throw new ArgumentOutOfRangeException(nameof(beginNonSpace), $"Must be less equal than {nameof(end)}.");
            }
            if (end > text.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(end), $"Must be less equal {nameof(text)} length.");
            }

            this.Text = text;
            this.Begin = begin;
            this.BeginNonSpace = beginNonSpace;
            this.End = end;
            this.Position = position;
        }

        public OriginalTextSpan CreateOriginalTextSpan(int begin, int end)
        {
            if (begin > end)
            {
                throw new ArgumentOutOfRangeException(nameof(begin), $"Must be less equal than {nameof(end)}.");
            }

            if (begin < this.Begin)
            {
                throw new ArgumentOutOfRangeException($"{nameof(begin)} is smaller than {nameof(FileLine)} {nameof(this.Begin)}!");
            }

            if (end > this.End)
            {
                throw new ArgumentOutOfRangeException($"{nameof(end)} is greater than {nameof(FileLine)} {nameof(this.End)}!");
            }

            var position = new TextPosition(this.Position.Line, begin - this.Begin + 1);
            var filePosition = new TextFilePosition(this.Position.Name, position);
            return new OriginalTextSpan(this.Text, begin, end, filePosition);
        }
    }
}