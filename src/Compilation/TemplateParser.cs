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

namespace Twofold.Compilation
{
    using Extensions;
    using Interface;
    using Interface.Compilation;
    using Interface.SourceMapping;
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

        public List<AsbtractRenderCommand> Parse(string sourceName, string text)
        {
            var fragments = new List<AsbtractRenderCommand>();

            int index = 0;
            int nonSpaceIndex = 0;
            int end = 0;
            int line = 1;
            while (index < text.Length)
            {
                nonSpaceIndex = text.IndexOfNot(index, text.Length, CharExtensions.IsSpace);
                end = text.IndexOf(nonSpaceIndex, text.Length, CharExtensions.IsNewline);

                IParserRule parserRule;
                List<AsbtractRenderCommand> ruleFragments;
                var textFilePostion = new TextFilePosition(sourceName, new TextPosition(line, 1));
                var fileLine = new FileLine(text, index, nonSpaceIndex, end, textFilePostion);
                if (this.ParseRules.TryGetValue(text[nonSpaceIndex], out parserRule))
                {
                    ruleFragments = parserRule.Parse(fileLine, this.MessageHandler);
                }
                else
                {
                    ruleFragments = this.FallbackRule.Parse(fileLine, this.MessageHandler);
                }
                fragments.AddRange(ruleFragments);

                if (end == text.Length)
                {
                    break;
                }

                char complementaryNewLineChar = ((text[end] == '\n') ? '\r' : '\n');
                index = text.IndexOfNot(end + 1, text.Length, c => c == complementaryNewLineChar);

                ++line;
            }
            return fragments;
        }
    }
}