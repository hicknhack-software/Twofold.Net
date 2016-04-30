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
    using System.Collections.Generic;
    using System.Diagnostics;

    /// <summary>
    /// Rule which handles template interpolation: "\ #{...} ... #{...}"
    /// </summary>
    internal class InterpolationRule : IParserRule
    {
        public virtual List<AsbtractRenderCommand> Parse(FileLine line, IMessageHandler messageHandler)
        {
            var fragments = new List<AsbtractRenderCommand>();

            var beginSpan = new TextSpan(line.Text, line.BeginNonSpace, line.BeginNonSpace + 1); //skip matched character
            var index = line.Text.IndexOfNot(beginSpan.End, line.End, CharExtensions.IsSpace);
            var indentationSpan = new TextSpan(line.Text, beginSpan.End, index);
            var endSpan = new TextSpan(line.Text, indentationSpan.End, indentationSpan.End);
            fragments.Add(new TargetIndentation(line, beginSpan, indentationSpan, endSpan));

            var end = index;
            while (index < line.End)
            {
                index = line.Text.IndexOf(index, line.End, (ch) => ch == '#');
                if (index == line.End)
                { // reached line end
                    break;
                }

                if ((index + 1) >= line.End)
                {
                    break;
                }

                switch (line.Text[index + 1])
                {
                    case '#':
                        {
                            var escapeBegin = (index + 1); //skip #
                            fragments.Add(new OriginText(line, new TextSpan(line.Text, escapeBegin, escapeBegin + 1), new TextSpan(line.Text, escapeBegin + 1, escapeBegin + 1)));
                            index = end = (escapeBegin + 1);
                            continue;
                        }

                    case '{':
                        {
                            fragments.Add(new OriginText(line, new TextSpan(line.Text, end, index), new TextSpan(line.Text, index, index)));

                            var expressionBegin = (index + 1);
                            var expressionEnd = BraceCounter.MatchBraces(line.Text, expressionBegin, line.End);
                            if (expressionEnd == line.End)
                            {
                                end = line.End;
                                messageHandler.Message(TraceLevel.Error, "Missing closing '}'.", line.Position.SourceName, line.Position);
                                break;
                            }
                            var expressionBeginSpan = new TextSpan(line.Text, index, expressionBegin + 1);
                            var expressionSpan = new TextSpan(line.Text, expressionBeginSpan.End, expressionEnd);
                            var expressionEndSpan = new TextSpan(line.Text, expressionSpan.End, expressionEnd + 1);
                            fragments.Add(new OriginExpression(line, expressionBeginSpan, expressionSpan, expressionEndSpan));
                            index = end = (expressionEnd + 1);
                            continue;
                        }

                    default:
                        {
                            index = end = (index + 1);
                            continue;
                        }
                }
            }
            fragments.Add(new OriginText(line, new TextSpan(line.Text, end, line.End), new TextSpan(line.Text, line.End, line.End)));
            return fragments;
        }
    }
}