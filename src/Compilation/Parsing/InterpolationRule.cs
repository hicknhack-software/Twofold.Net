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
using System.Diagnostics;
using Twofold.Interface;
using Twofold.Interface.Compilation.Generation;
using Twofold.Interface.Compilation.Parsing;
using Twofold.Extensions;

namespace Twofold.Compilation.Parsing
{
    public class InterpolationRule : IParserRule
    {
        public List<AsbtractCodeFragment> Parse(FileLine line, IMessageHandler messageHandler)
        {
            List<AsbtractCodeFragment> fragments = new List<AsbtractCodeFragment>();

            var beginIndexIndent = (line.BeginIndexNonSpace + 1); //skip matched character
            var index = line.Text.IndexOfNot(beginIndexIndent, line.EndIndex, CharExtensions.IsSpace);
            fragments.Add(new TargetIndentation(line, new TextSpan(line.Text, beginIndexIndent, index)));

            var endIndex = index;
            while (index < line.EndIndex) {
                index = line.Text.IndexOf(index, line.EndIndex, (ch) => ch == '#');
                if (index == line.EndIndex) { // reached line end
                    break;
                }

                var expressionBeginIndex = index + 1; //skip #
                if (expressionBeginIndex == line.EndIndex) { // reached line end
                    break;
                }

                switch (line.Text[expressionBeginIndex]) {
                    case '#':
                        fragments.Add(new OriginText(line, new TextSpan(line.Text, expressionBeginIndex, expressionBeginIndex + 1)));
                        index = endIndex = expressionBeginIndex + 1;
                        continue;

                    case '{':
                        var expressionEndIndex = BraceCounter.MatchBraces(line.Text, expressionBeginIndex, line.EndIndex);
                        if (expressionEndIndex == line.EndIndex) {
                            endIndex = line.EndIndex;
                            messageHandler.TemplateMessage(TraceLevel.Error, line.Position, "Missing a closing '}'.");
                            break;
                        }
                        fragments.Add(new OriginExpression(line, new TextSpan(line.Text, expressionBeginIndex + 1, expressionEndIndex - 1)));
                        index = endIndex = expressionEndIndex + 1;
                        continue;

                    default:
                        index = endIndex = expressionBeginIndex + 1;
                        continue;
                }
            }
            fragments.Add(new OriginText(line, new TextSpan(line.Text, endIndex, line.EndIndex)));
            return fragments;
        }
    }
}
