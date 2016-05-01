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

namespace Twofold.Interface
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
                throw new ArgumentOutOfRangeException(nameof(begin), "Must be less equal than beginNonSpace.");
            }
            if (beginNonSpace > end)
            {
                throw new ArgumentOutOfRangeException(nameof(beginNonSpace), "Must be less equal than end.");
            }
            if (end > text.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(end), "end must be less equal string length.");
            }

            this.Text = text;
            this.Begin = begin;
            this.BeginNonSpace = beginNonSpace;
            this.End = end;
            this.Position = position;
        }

        public TextFilePosition CreateFilePosition(TextSpan textSpan)
        {
            if (textSpan == null)
            {
                throw new ArgumentNullException(nameof(textSpan));
            }

            if (textSpan.Begin < this.Begin)
            {
                throw new ArgumentOutOfRangeException($"{nameof(textSpan.Begin)} of {nameof(textSpan)} is smaller than {nameof(FileLine)} {nameof(this.Begin)}!");
            }

            if (textSpan.End > this.End)
            {
                throw new ArgumentOutOfRangeException($"{nameof(textSpan.End)} of {nameof(textSpan)} is larger than {nameof(FileLine)} {nameof(this.End)}!");
            }

            return new TextFilePosition(this.Position.SourceName, new TextPosition(this.Position.Line, textSpan.Begin - this.Begin + 1));
        }
    }
}