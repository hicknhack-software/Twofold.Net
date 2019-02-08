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

namespace HicknHack.Twofold.Abstractions
{
    using SourceMapping;
    using System;

    public class OriginalTextSpan
    {
        /// <summary>
        /// Begin of the text span in the OriginalText.
        /// </summary>
        public readonly int Begin;

        /// <summary>
        /// End + 1 of the text span in the OriginalText.
        /// </summary>
        public readonly int End;

        /// <summary>
        /// The complete original text.
        /// </summary>
        public readonly string OriginalText;

        /// <summary>
        /// The text span [Begin, End) from the OriginalText.
        /// </summary>
        public readonly string Text;

        public readonly TextFilePosition Position;

        public OriginalTextSpan(string text, TextFilePosition position)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }
            this.Begin = 0;
            this.End = text.Length;
            this.OriginalText = text;
            this.Text = text;

            if (position == null)
            {
                throw new ArgumentNullException(nameof(position));
            }
            this.Position = position;
        }

        public OriginalTextSpan(string text, int begin, int end, TextFilePosition position)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }
            if (begin > end)
            {
                throw new ArgumentOutOfRangeException(nameof(begin), "Must be less equal than end.");
            }
            if (end > text.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(end), "end must be less equal string length.");
            }
            this.Begin = begin;
            this.End = end;
            this.OriginalText = text;
            this.Text = OriginalText.Substring(Begin, End - Begin);

            if (position == null)
            {
                throw new ArgumentNullException(nameof(position));
            }
            this.Position = position;
        }

        public bool IsEmpty
        {
            get { return (Begin == End); }
        }

        public int Length
        {
            get { return (this.End - this.Begin); }
        }
    }
}