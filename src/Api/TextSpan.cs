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

namespace Twofold.Api
{
    public class TextSpan
    {
        /// <summary>
        /// Begin of the text span in the original text.
        /// </summary>
        public readonly int BeginIndex;
        /// <summary>
        /// End + 1 of the text span in the original text.
        /// </summary>
        public readonly int EndIndex;
        /// <summary>
        /// The complete original text.
        /// </summary>
        public readonly string OriginalText;
        /// <summary>
        /// The text span from the original text.
        /// </summary>
        public readonly string Text;

        public bool IsEmpty { get { return (BeginIndex == EndIndex); } }

        public TextSpan(string text)
        {
            if (text == null) {
                throw new ArgumentNullException("text");
            }
            BeginIndex = 0;
            EndIndex = text.Length;
            OriginalText = text;
            Text = text;
        }

        public TextSpan(string text, int beginIndex, int endIndex)
        {
            if (text == null) {
                throw new ArgumentNullException("text");
            }
            if (beginIndex > endIndex) {
                throw new ArgumentOutOfRangeException("beginIndex", "Must be less equal than endIndex.");
            }
            if (endIndex > text.Length) {
                throw new ArgumentOutOfRangeException("endIndex", "endIndex must be less equal string length.");
            }
            BeginIndex = beginIndex;
            EndIndex = endIndex;
            OriginalText = text;
            Text = OriginalText.Substring(BeginIndex, EndIndex - BeginIndex);
        }
    }
}
