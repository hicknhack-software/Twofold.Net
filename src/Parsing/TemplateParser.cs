using System.Collections.Generic;
using Twofold.Api;
using Twofold.Api.SourceMapping;
using Twofold.Extensions;

namespace Twofold.Parsing
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
            FileLine fileLine = new FileLine(twofoldText, 0, 0, 0, new TextFilePosition(name, new TextPosition(0, 0)));

            while (fileLine.BeginIndex != twofoldText.Length) {
                ++fileLine.Position.Line;

                fileLine.BeginIndexNonSpace = twofoldText.IndexOfNot(fileLine.BeginIndex, twofoldText.Length, CharExtensions.IsSpace);
                if (fileLine.BeginIndexNonSpace == -1) {
                    fileLine.BeginIndexNonSpace = twofoldText.Length;
                }

                fileLine.EndIndex = twofoldText.IndexOf(fileLine.BeginIndexNonSpace, twofoldText.Length, CharExtensions.IsNewline);
                if (fileLine.EndIndex == -1) {
                    fileLine.EndIndex = twofoldText.Length;
                }

                IParserRule parserRule;
                if (parseRules.TryGetValue(twofoldText[fileLine.EndIndex], out parserRule)) {
                    parserRule.Parse(fileLine, codeGenerator, messageHandler);
                }

                if (fileLine.EndIndex == twofoldText.Length) {
                    break;
                }

                char complementaryNewLineChar = ((twofoldText[fileLine.EndIndex] == '\n') ? '\r' : '\n');
                fileLine.BeginIndex = twofoldText.IndexOfNot(fileLine.EndIndex + 1, twofoldText.Length, c => c == complementaryNewLineChar);
                if (fileLine.BeginIndex == -1) {
                    fileLine.BeginIndex = twofoldText.Length;
                }
            }
        }
    }
}
