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
using Twofold.Extensions;

namespace Twofold.Compilation.Parsing
{
    public class BraceCounter
    {
        static bool IsBraceOrQuote(char ch)
        {
            switch (ch) {
                case '{':
                case '}':
                case '"':
                case '\'':
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Finds index of closing quote. Search must begin on index of opening quote.
        /// </summary>
        /// <returns>Index of end quote or -1 if nothing found.</returns>
        /// <exception cref="ArgumentOutOfRangeException">beginIndex is greater than endIndex.</exception>
        /// <exception cref="ArgumentOutOfRangeException">endIndex is greater than string length.</exception>
        /// <exception cref="InvalidOperationException">beginIndex must be on a ' order \ character.</exception>
        public static int FindQuoteEnd(int beginIndex, int endIndex, string text)
        {
            if (beginIndex > endIndex) {
                throw new ArgumentException("beginIndex must be less equal endIndex.");
            }
            if (endIndex > text.Length) {
                throw new ArgumentOutOfRangeException("endIndex must be less equal text length.");
            }
            char quoteChar = text[beginIndex];
            if (quoteChar != '"' && quoteChar != '\'') {
                throw new InvalidOperationException("beginIndex must be on a ' or \" char.");
            }

            ++beginIndex;
            while (beginIndex < endIndex) {
                var index = text.IndexOf(beginIndex, endIndex, (ch) => ch == quoteChar);
                if (index == -1) { // Invalid, not found
                    return index;
                }

                if ((index > beginIndex) && text[index - 1] != '\\') { //Success, found unescaped quote char
                    return index;
                }

                beginIndex = index + 1;
            }

            return -1;
        }

        /// <summary>
        /// Finds index of closing brace. Search must begin on index of opening brace.
        /// </summary>
        /// <returns>Index of closing brace or -1 if nothing found.</returns>
        /// <exception cref="ArgumentOutOfRangeException">beginIndex is greater than endIndex.</exception>
        /// <exception cref="ArgumentOutOfRangeException">endIndex is greater than string length.</exception>
        /// <exception cref="InvalidOperationException">beginIndex must be on a { character.</exception>
        public static int MatchBraces(int beginIndex, int endIndex, string text)
        {
            if (beginIndex > endIndex) {
                throw new ArgumentException("beginIndex must be less equal endIndex.");
            }
            if (endIndex > text.Length) {
                throw new ArgumentOutOfRangeException("endIndex must be less equal text length.");
            }
            char braceChar = text[beginIndex];
            if (braceChar != '{') {
                throw new InvalidOperationException("beginIndex must be on a '{' char.");
            }

            int depth = 0;
            while (beginIndex < endIndex) {

                var index = text.IndexOf(beginIndex, endIndex, BraceCounter.IsBraceOrQuote);
                if (index == -1) { // Error, nothing found
                    return -1;
                }

                switch (text[index]) {
                    case '{':
                        ++depth;
                        break;

                    case '}':
                        if (depth == 0) { //Error, found closing brace before any opening
                            return -1;
                        }
                        --depth;
                        if (depth == 0) {
                            return index; //Found, all closed
                        }
                        break;

                    default:
                        index = BraceCounter.FindQuoteEnd(index, endIndex, text);
                        if (index == -1) { //Error, found no closing quote
                            return index;
                        }
                        break;
                }
                beginIndex = index + 1;
            }

            return -1;
        }
    }

}
