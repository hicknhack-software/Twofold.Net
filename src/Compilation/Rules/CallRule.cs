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

    /// <summary>
    /// Rule which handles a template method call: "= <method>"
    /// </summary>
    internal class CallRule : IParserRule
    {
        public List<AsbtractRenderCommand> Parse(FileLine line, IMessageHandler messageHandler)
        {
            var commands = new List<AsbtractRenderCommand>();

            //
            var beginSpan = line.CreateSourceTextSpan(line.BeginNonSpace, line.BeginNonSpace + 1); //skip matched character
            var scriptBegin = line.Text.IndexOfNot(beginSpan.End, line.End, CharExtensions.IsSpace);
            var indentationSpan = line.CreateSourceTextSpan(beginSpan.End, scriptBegin);
            if (indentationSpan.IsEmpty == false)
            {
                var endSpan = line.CreateSourceTextSpan(indentationSpan.End, indentationSpan.End);
                commands.Add(new PushIndentationCommand(beginSpan, indentationSpan, endSpan));
            }

            //
            var statementSpan = line.CreateSourceTextSpan(indentationSpan.End, line.End);
            var statementEndSpan = line.CreateSourceTextSpan(statementSpan.End, statementSpan.End);
            commands.Add(new StatementCommand(statementSpan, statementEndSpan));

            //
            if (indentationSpan.IsEmpty == false)
            {
                var popIndentationSpan = line.CreateSourceTextSpan(line.End, line.End);
                commands.Add(new PopIndentationCommand(popIndentationSpan));
            }

            return commands;
        }
    }
}