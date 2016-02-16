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

namespace Twofold.Compilation
{
    internal class BraceCounter
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
        /// <returns>Index of end quote or end if nothing found.</returns>
        /// <exception cref="ArgumentOutOfRangeException">begin is greater than end.</exception>
        /// <exception cref="ArgumentOutOfRangeException">end is greater than string length.</exception>
        /// <exception cref="InvalidOperationException">begin must be on a ' order \ character.</exception>
        public static int FindQuoteEnd(string text, int begin, int end)
        {
            if (begin > end) {
                throw new ArgumentException("begin must be less equal end.");
            }
            if (end > text.Length) {
                throw new ArgumentOutOfRangeException("end must be less equal text length.");
            }
            char quoteChar = text[begin];
            if (quoteChar != '"' && quoteChar != '\'') {
                throw new InvalidOperationException("begin must be on a ' or \" char.");
            }

            ++begin;
            while (begin < end) {
                var index = text.IndexOf(begin, end, (ch) => ch == quoteChar);
                if (index == end) { // Invalid, not found
                    return index;
                }

                if ((index > begin) && text[index - 1] == '\\') { //Success, found unescaped quote char
                    begin = (index + 1);
                }
                else {
                    return index;
                }
            }

            return end;
        }

        /// <summary>
        /// Finds index of closing brace. Search must begin on index of opening brace.
        /// </summary>
        /// <returns>Index of closing brace or end if nothing found.</returns>
        /// <exception cref="ArgumentOutOfRangeException">begin is greater than end.</exception>
        /// <exception cref="ArgumentOutOfRangeException">end is greater than string length.</exception>
        /// <exception cref="InvalidOperationException">begin must be on a { character.</exception>
        public static int MatchBraces(string text, int begin, int end)
        {
            if (begin > end) {
                throw new ArgumentException("begin must be less equal end.");
            }
            if (end > text.Length) {
                throw new ArgumentOutOfRangeException("end must be less equal text length.");
            }
            char braceChar = text[begin];
            if (braceChar != '{') {
                throw new InvalidOperationException("begin must be on a '{' char.");
            }

            int depth = 0;
            while (begin < end) {

                var index = text.IndexOf(begin, end, BraceCounter.IsBraceOrQuote);
                if (index == end) { // Error, nothing found
                    return end;
                }

                switch (text[index]) {
                    case '{':
                        ++depth;
                        break;

                    case '}':
                        if (depth == 0) { //Error, found closing brace before any opening
                            return end;
                        }
                        --depth;
                        if (depth == 0) {
                            return index; //Found, all closed
                        }
                        break;

                    default:
                        index = BraceCounter.FindQuoteEnd(text, index, end);
                        if (index == end) { //Error, found no closing quote
                            return end;
                        }
                        break;
                }
                begin = (index + 1);
            }

            return end;
        }
    }

}
