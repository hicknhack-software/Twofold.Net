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

namespace Twofold.Extensions
{
    using System;
    using System.Globalization;
    using System.Text;

    internal static class StringExtensions
    {
        /// <exception cref="ArgumentOutOfRangeException">begin is greater than end.</exception>
        /// <exception cref="ArgumentOutOfRangeException">end is greater than string length.</exception>
        public static int IndexOf(this string value, int begin, int end, Func<char, bool> predicate)
        {
            if (begin > end)
            {
                throw new ArgumentOutOfRangeException(nameof(begin), "Must be less equal than end.");
            }
            if (end > value.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(end), "end must be less equal string length.");
            }
            var index = begin;
            while (index < end)
            {
                char ch = value[index];
                if (predicate(ch))
                {
                    return index;
                }
                ++index;
            }
            return end;
        }

        /// <exception cref="ArgumentOutOfRangeException">begin is greater than endIndex.</exception>
        /// <exception cref="ArgumentOutOfRangeException">end is greater than string length.</exception>
        public static int IndexOfNot(this string value, int begin, int end, Func<char, bool> predicate)
        {
            if (begin > end)
            {
                throw new ArgumentOutOfRangeException(nameof(begin), "Must be less equal than end.");
            }
            if (end > value.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(end), "end must be less equal string length.");
            }
            var index = begin;
            while (index < end)
            {
                char ch = value[index];
                if (!predicate(ch))
                {
                    return index;
                }
                ++index;
            }
            return end;
        }

        public static string Escape(this string text)
        {
            char[] HexDigit = "0123456789abcdef".ToCharArray();
            var sb = new StringBuilder(text.Length);
            var len = text.Length;
            for (int c = 0; c < len; ++c)
            {
                char ch = text[c];
                switch (ch)
                {
                    case '\'': sb.Append(@"\'"); break;
                    case '\"': sb.Append("\\\""); break;
                    case '\\': sb.Append(@"\\"); break;
                    case '\0': sb.Append(@"\0"); break;
                    case '\a': sb.Append(@"\a"); break;
                    case '\b': sb.Append(@"\b"); break;
                    case '\f': sb.Append(@"\f"); break;
                    case '\n': sb.Append(@"\n"); break;
                    case '\r': sb.Append(@"\r"); break;
                    case '\t': sb.Append(@"\t"); break;
                    case '\v': sb.Append(@"\v"); break;
                    default:
                        {
                            switch (char.GetUnicodeCategory(ch))
                            {
                                case UnicodeCategory.Control:
                                    {
                                        var c1 = HexDigit[(ch >> 12) & 0x0F];
                                        var c2 = HexDigit[(ch >> 8) & 0x0F];
                                        var c3 = HexDigit[(ch >> 4) & 0x0F];
                                        var c4 = HexDigit[ch & 0x0F];
                                        sb
                                            .Append(@"\x")
                                            .Append(c1)
                                            .Append(c2)
                                            .Append(c3)
                                            .Append(c4);
                                    }
                                    break;

                                default:
                                    sb.Append(ch);
                                    break;
                            }
                        }
                        break;
                }
            }
            return sb.ToString();
        }
    }
}