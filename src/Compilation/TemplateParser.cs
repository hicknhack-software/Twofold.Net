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
using Twofold.Interface;
using Twofold.Interface.Compilation;
using Twofold.Interface.SourceMapping;
using Twofold.Extensions;

namespace Twofold.Compilation
{
    public sealed class TemplateParser : ITemplateParser
    {
        readonly Dictionary<char, IParserRule> parseRules = new Dictionary<char, IParserRule>();
        readonly IParserRule fallbackRule;
        readonly IMessageHandler messageHandler;

        public TemplateParser(Dictionary<char, IParserRule> parseRules, IParserRule fallbackRule, IMessageHandler messageHandler)
        {
            this.parseRules = parseRules;
            this.fallbackRule = fallbackRule;
            this.messageHandler = messageHandler;
        }

        public List<AsbtractCodeFragment> Parse(string sourceName, string text)
        {
            List<AsbtractCodeFragment> fragments = new List<AsbtractCodeFragment>();

            int index = 0;
            int nonSpaceIndex = 0;
            int endIndex = 0;
            int line = 1;
            while (index < text.Length) {

                nonSpaceIndex = text.IndexOfNot(index, text.Length, CharExtensions.IsSpace);
                endIndex = text.IndexOf(nonSpaceIndex, text.Length, CharExtensions.IsNewline);

                IParserRule parserRule;
                List<AsbtractCodeFragment> ruleFragments;
                var textFilePostion = new TextFilePosition(sourceName, new TextPosition(line, 1));
                var fileLine = new FileLine(text, index, nonSpaceIndex, endIndex, textFilePostion);
                if (parseRules.TryGetValue(text[nonSpaceIndex], out parserRule)) {
                    ruleFragments = parserRule.Parse(fileLine, messageHandler);
                }
                else {
                    ruleFragments = fallbackRule.Parse(fileLine, messageHandler);
                }
                fragments.AddRange(ruleFragments);

                if (endIndex == text.Length) {
                    break;
                }

                char complementaryNewLineChar = ((text[endIndex] == '\n') ? '\r' : '\n');
                index = text.IndexOfNot(endIndex + 1, text.Length, c => c == complementaryNewLineChar);

                ++line;
            }
            return fragments;
        }
    }
}
