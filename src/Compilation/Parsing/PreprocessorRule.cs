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
                if (index == line.EndIndex) {
                    break;
                }
                if (string.Compare(preproDirective, "pragma") != 0) {
                    break;
                }

                // Find 'include'
                string pragmaName;
                index = this.MatchNextToken(line.Text, index, line.EndIndex, out pragmaName);
                if (index == line.EndIndex) {
                    break;
                }
                if (string.Compare(pragmaName, "include") != 0) {
                    break;
                }

                // Find '"<Filename>"'
                var pragmaArgIndex = line.Text.IndexOf(index, line.EndIndex, ch => ch == '"');
                if (pragmaArgIndex == line.EndIndex) {
                    break;
                }

                var pragmaArgEndIndex = BraceCounter.FindQuoteEnd(line.Text, index, line.EndIndex);
                if (pragmaArgEndIndex == line.EndIndex) {
                    break;
                }

                // Extract <Filename> from '"<Filename>"'
                string pragmaArgument = line.Text.Substring(pragmaArgIndex + 1, (pragmaArgEndIndex - pragmaArgIndex - 1));
                var textSpan = new TextSpan(line.Text, line.BeginIndex, line.EndIndex);
                fragments.Add(new OriginPragma(pragmaName, pragmaArgument, line, textSpan));

                index = (pragmaArgEndIndex + 1);

                break;
            }

            fragments.Add(new OriginScript(line, new TextSpan(line.Text, line.BeginIndex, line.EndIndex)));
            return fragments;
        }

        int MatchNextToken(string text, int beginIndex, int endIndex, out string token)
        {
            token = "";

            int index = beginIndex;
            var tokenIndex = text.IndexOfNot(index, endIndex, CharExtensions.IsSpace);
            index = (tokenIndex + 1);

            var tokenEndIndex = text.IndexOf(index, endIndex, CharExtensions.IsSpace);
            index = (tokenEndIndex + 1);

            token = text.Substring(tokenIndex, (tokenEndIndex - tokenIndex));

            return index;
        }
    }
}
