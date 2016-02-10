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
using System.Collections.Generic;
using Twofold.Api;
using Twofold.Api.Compilation.Generation;
using Twofold.Api.Compilation.Parsing;
using Twofold.Extensions;

namespace Twofold.Compilation.Parsing
{
    public class PreprocessorRule : IParserRule
    {
        public List<AsbtractCodeFragment> Parse(FileLine line, IMessageHandler messageHandler)
        {
            List<AsbtractCodeFragment> fragments = new List<AsbtractCodeFragment>();

            while (true) {
                var index = (line.BeginIndexNonSpace + 1); // Skip #

                // Find 'pragma'
                string preproDirective;
                index = this.MatchNextToken(line.Text, index, line.EndIndex, out preproDirective);
                if (index == -1) {
                    break;
                }
                if (string.Compare(preproDirective, "pragma") != 0) {
                    break;
                }

                // Find 'include'
                string pragmaName;
                index = this.MatchNextToken(line.Text, index, line.EndIndex, out pragmaName);
                if (index == -1) {
                    break;
                }
                if (string.Compare(pragmaName, "include") != 0) {
                    break;
                }

                // Find '"<Filename>"'
                var pragmaArgIndex = line.Text.IndexOf(index, line.EndIndex, ch => ch == '"');
                if (pragmaArgIndex == -1) {
                    break;
                }
                index = (pragmaArgIndex + 1);

                var pragmaArgEndIndex = BraceCounter.FindQuoteEnd(index, line.EndIndex, line.Text);
                if (pragmaArgEndIndex == -1) {
                    break;
                }
                index = (pragmaArgEndIndex + 1);

                string pragmaArgument = line.Text.Substring(pragmaArgIndex + 1, (pragmaArgEndIndex - pragmaArgIndex - 1));
                var textSpan = new TextSpan(line.BeginIndex, line.EndIndex, line.Text);
                fragments.Add(new OriginPragma(pragmaName, pragmaArgument, line, textSpan));

                break;
            }

            fragments.Add(new OriginScript(line, new TextSpan(line.BeginIndex, line.EndIndex, line.Text)));
            return fragments;
        }

        int MatchNextToken(string text, int beginIndex, int endIndex, out string token)
        {
            token = "";

            int index = beginIndex;
            var tokenIndex = text.IndexOfNot(index, endIndex, CharExtensions.IsSpace);
            if (tokenIndex == -1) {
                return -1;
            }
            index = (tokenIndex + 1);

            var tokenEndIndex = text.IndexOf(index, endIndex, CharExtensions.IsSpace);
            if (tokenIndex == -1) {
                return -1;
            }
            index = (tokenEndIndex + 1);

            token = text.Substring(tokenIndex, (tokenEndIndex - tokenIndex));

            return index;
        }
    }
}
