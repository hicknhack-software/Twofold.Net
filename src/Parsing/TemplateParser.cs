using System.Collections.Generic;
using System.Globalization;
using Twofold.Api;
using Twofold.Extensions;

namespace Twofold
{
    public class TemplateParser
    {
        readonly Dictionary<char, IParserRule> parseRules = new Dictionary<char, IParserRule>();
        readonly IParserRule fallbackRule;

        public TemplateParser(Dictionary<char, IParserRule> parseRules, IParserRule fallbackRule)
        {
            this.parseRules = parseRules;
            this.fallbackRule = fallbackRule;
        }

        public void Parse(string name, string twofoldText, ICodeGenerator codeGenerator, IMessageHandler messageHandler)
        {
            int beginIndex = 0;
            while (beginIndex != twofoldText.Length) {
                int firstNonSpaceIndex = twofoldText.IndexOfNot(beginIndex, twofoldText.Length, c => char.GetUnicodeCategory(c) == UnicodeCategory.SpaceSeparator || c == '\t');
                if (firstNonSpaceIndex == -1) {
                    firstNonSpaceIndex = twofoldText.Length;
                }

                int endIndex = twofoldText.IndexOf(firstNonSpaceIndex, twofoldText.Length, c => c == '\n' || c == '\r');
                if (endIndex == -1) {
                    endIndex = twofoldText.Length;
                }

                IParserRule parserRule;
                if (parseRules.TryGetValue(twofoldText[endIndex], out parserRule)) {
                    parserRule.Parse(twofoldText, beginIndex, firstNonSpaceIndex, endIndex, codeGenerator, messageHandler);
                }

                if (endIndex == twofoldText.Length) {
                    break;
                }

                char complementaryNewLineChar = ((twofoldText[endIndex] == '\n') ? '\r' : '\n');
                beginIndex = twofoldText.IndexOfNot(endIndex + 1, twofoldText.Length, c => c == complementaryNewLineChar);
                if (beginIndex == -1) {
                    beginIndex = twofoldText.Length;
                }
            }
        }
    }
}
