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

namespace HicknHack.Twofold.Compilation.Rules
{
    using Extensions;
    using Abstractions;
    using Abstractions.Compilation;
    using System.Collections.Generic;

    /// <summary>
    /// Rule which handles a template method call: "= <statement>"
    /// </summary>
    internal class CallRule : IParserRule
    {
        public List<AsbtractRenderCommand> Parse(FileLine line, IMessageHandler messageHandler)
        {
            var commands = new List<AsbtractRenderCommand>();

            //
            var beginSpan = line.CreateOriginalTextSpan(line.BeginNonSpace, line.BeginNonSpace + 1); //skip matched character
            var scriptBegin = line.Text.IndexOfNot(beginSpan.End, line.End, CharExtensions.IsSpace);
            var indentationSpan = line.CreateOriginalTextSpan(beginSpan.End, scriptBegin);
            if (indentationSpan.IsEmpty == false)
            {
                var endSpan = line.CreateOriginalTextSpan(indentationSpan.End, indentationSpan.End);
                commands.Add(new PushIndentationCommand(beginSpan, indentationSpan, endSpan));
            }

            //
            var statementSpan = line.CreateOriginalTextSpan(indentationSpan.End, line.End);
            var statementEndSpan = line.CreateOriginalTextSpan(statementSpan.End, statementSpan.End);
            if (statementSpan.IsEmpty == false)
            {
                commands.Add(new StatementCommand(statementSpan, statementEndSpan));
            }

            //
            if (indentationSpan.IsEmpty == false)
            {
                var popIndentationSpan = line.CreateOriginalTextSpan(line.End, line.End);
                commands.Add(new PopIndentationCommand(popIndentationSpan));
            }

            return commands;
        }
    }
}