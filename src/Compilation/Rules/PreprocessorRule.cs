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

namespace Twofold.Compilation.Rules
{
    using Extensions;
    using Interface;
    using Interface.Compilation;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Rule which handles template preprocessor statement: "#<name> <args>?"
    /// </summary>
    internal class PreprocessorRule : IParserRule
    {
        public List<AsbtractRenderCommand> Parse(FileLine line, IMessageHandler messageHandler)
        {
            var fragments = new List<AsbtractRenderCommand>();

            while (true)
            {
                var index = (line.BeginNonSpace + 1); // Skip #

                // Find 'pragma'
                string preproDirective;
                index = this.MatchNextToken(line.Text, index, line.End, out preproDirective);
                if (index == line.End)
                {
                    break;
                }
                if (string.Compare(preproDirective, "pragma", StringComparison.Ordinal) != 0)
                {
                    break;
                }

                // Find 'include'
                string pragmaName;
                index = this.MatchNextToken(line.Text, index, line.End, out pragmaName);
                if (index == line.End)
                {
                    break;
                }
                if (string.Compare(pragmaName, "include", StringComparison.Ordinal) != 0)
                {
                    break;
                }

                // Find '"<Filename>"'
                var pragmaArgBegin = line.Text.IndexOf(index, line.End, ch => ch == '"');
                if (pragmaArgBegin == line.End)
                {
                    break;
                }

                var pragmaArgEnd = BraceCounter.FindQuoteEnd(line.Text, index, line.End);
                if (pragmaArgEnd == line.End)
                {
                    break;
                }

                // Extract <Filename> from '"<Filename>"'
                string pragmaArgument = line.Text.Substring(pragmaArgBegin + 1, (pragmaArgEnd - pragmaArgBegin - 1));
                var pragmaSpan = new TextSpan(line.Text, line.Begin, line.End);
                fragments.Add(new OriginPragma(line, pragmaSpan, pragmaName, pragmaArgument));

                index = (pragmaArgEnd + 1);

                break;
            }

            // No pragma detected, pass line through
            if (fragments.Count == 0)
            {
                fragments.Add(new OriginScript(line, new TextSpan(line.Text, line.Begin, line.End)));
            }

            return fragments;
        }

        private int MatchNextToken(string text, int begin, int end, out string token)
        {
            if (begin > end)
            {
                throw new ArgumentOutOfRangeException(nameof(begin), "Must be less equal than end.");
            }
            if (end > text.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(end), "end must be less equal string length.");
            }

            token = "";

            int index = begin;
            var tokenIndex = text.IndexOfNot(index, end, CharExtensions.IsSpace);
            if (tokenIndex == end)
            {
                return end;
            }
            index = (tokenIndex + 1);

            var tokenEndIndex = text.IndexOf(index, end, CharExtensions.IsSpace);
            if (tokenEndIndex == end)
            {
                return end;
            }
            index = (tokenEndIndex + 1);

            token = text.Substring(tokenIndex, tokenEndIndex - tokenIndex);

            return index;
        }
    }
}