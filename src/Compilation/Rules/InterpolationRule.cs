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
    using Interface.SourceMapping;
    using System.Collections.Generic;
    using System.Diagnostics;

    /// <summary>
    /// Rule which handles template interpolation: "\ #{...} ... #{...}"
    /// </summary>
    internal class InterpolationRule : IParserRule
    {
        public virtual List<AsbtractRenderCommand> Parse(FileLine line, IMessageHandler messageHandler)
        {
            var commands = new List<AsbtractRenderCommand>();

            var beginSpan = line.CreateOriginalTextSpan(line.BeginNonSpace, line.BeginNonSpace + 1); //skip matched character
            var index = line.Text.IndexOfNot(beginSpan.End, line.End, CharExtensions.IsSpace);
            var indentationSpan = line.CreateOriginalTextSpan(beginSpan.End, index);
            if (indentationSpan.IsEmpty == false)
            {
                var endSpan = line.CreateOriginalTextSpan(indentationSpan.End, indentationSpan.End);
                commands.Add(new PushIndentationCommand(beginSpan, indentationSpan, endSpan));
            }

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
                            var textSpan = line.CreateOriginalTextSpan(end, index);
                            if (textSpan.IsEmpty == false)
                            {
                                var textEndSpan = line.CreateOriginalTextSpan(index, index);
                                commands.Add(new TextCommand(textSpan, textEndSpan));
                            }

                            var escapeBegin = (index + 1); //skip #
                            var escapeSpan = line.CreateOriginalTextSpan(escapeBegin, escapeBegin + 1);
                            if (escapeSpan.IsEmpty == false) 
                            {
                                var escapeEndSpan = line.CreateOriginalTextSpan(escapeSpan.End, escapeSpan.End);
                                commands.Add(new TextCommand(escapeSpan, escapeEndSpan));
                            }
                            index = end = (escapeBegin + 1);
                            continue;
                        }

                    case '{':
                        {
                            var textSpan = line.CreateOriginalTextSpan(end, index);
                            if (textSpan.IsEmpty == false)
                            {
                                var textEndSpan = line.CreateOriginalTextSpan(index, index);
                                commands.Add(new TextCommand(textSpan, textEndSpan));
                            }

                            var expressionBegin = (index + 1);
                            var expressionEnd = BraceCounter.MatchBraces(line.Text, expressionBegin, line.End);
                            if (expressionEnd == line.End)
                            {
                                index = end = line.End;
                                var errorPosition = new TextPosition(line.Position.Line, 1 + (line.End - line.Begin));
                                messageHandler.Message(TraceLevel.Error, "Missing '}'.", line.Position.Name, errorPosition);
                                break;
                            }
                            var expressionBeginSpan = line.CreateOriginalTextSpan(index, expressionBegin + 1);
                            var expressionSpan = line.CreateOriginalTextSpan(expressionBeginSpan.End, expressionEnd);
                            if (expressionSpan.IsEmpty == false)
                            {
                                var expressionEndSpan = line.CreateOriginalTextSpan(expressionSpan.End, expressionEnd + 1);
                                commands.Add(new ExpressionCommand(expressionBeginSpan, expressionSpan, expressionEndSpan));
                            }
                            index = end = (expressionEnd + 1);
                            continue;
                        }

                    default:
                        {
                            index = end = (index + 2);
                            continue;
                        }
                }
            }
            var lastTextSpan = line.CreateOriginalTextSpan(end, line.End);
            if (lastTextSpan.IsEmpty == false)
            {
                var textEndSpan = line.CreateOriginalTextSpan(line.End, line.End);
                commands.Add(new TextCommand(lastTextSpan, textEndSpan));
            }

            if (indentationSpan.IsEmpty == false)
            {
                var popIndentationSpan = line.CreateOriginalTextSpan(line.End, line.End);
                commands.Add(new PopIndentationCommand(popIndentationSpan));
            }

            return commands;
        }
    }
}