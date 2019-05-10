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

namespace HicknHack.Twofold.Compilation
{
    using Extensions;
    using Abstractions;
    using Abstractions.Compilation;
    using Abstractions.SourceMapping;
    using System.Collections.Generic;

    internal sealed class TemplateParser : ITemplateParser
    {
        private readonly Dictionary<char, IParserRule> ParseRules;
        private readonly IParserRule FallbackRule;
        private readonly IMessageHandler MessageHandler;

        public TemplateParser(Dictionary<char, IParserRule> parseRules, IParserRule fallbackRule, IMessageHandler messageHandler)
        {
            this.ParseRules = parseRules;
            this.FallbackRule = fallbackRule;
            this.MessageHandler = messageHandler;
        }

        public List<AsbtractRenderCommand> Parse(string originalName, string text)
        {
            var commands = new List<AsbtractRenderCommand>();

            int index = 0;
            int nonSpaceIndex = 0;
            int end = 0;
            int line = 1;
            while (index < text.Length)
            {
                nonSpaceIndex = text.IndexOfNot(index, text.Length, CharExtensions.IsSpace);
                end = text.IndexOf(nonSpaceIndex, text.Length, CharExtensions.IsNewline);

                List<AsbtractRenderCommand> ruleCommands;
                var textFilePostion = new TextFilePosition(originalName, new TextPosition(line, 1));
                var fileLine = new FileLine(text, index, nonSpaceIndex, end, textFilePostion);
                if (this.ParseRules.TryGetValue(text[nonSpaceIndex], out IParserRule parserRule))
                {
                    ruleCommands = parserRule.Parse(fileLine, this.MessageHandler);
                }
                else
                {
                    ruleCommands = this.FallbackRule.Parse(fileLine, this.MessageHandler);
                }
                commands.AddRange(ruleCommands);

                if (end == text.Length)
                {
                    break;
                }

                char complementaryNewLineChar = ((text[end] == '\n') ? '\r' : '\n');
                index = text.IndexOfNot(end + 1, text.Length, c => c == complementaryNewLineChar);

                ++line;
            }
            return commands;
        }
    }
}