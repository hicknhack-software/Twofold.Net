using System.Collections.Generic;
using Twofold.Api;
using Twofold.Api.Compilation.Generation;
using Twofold.Api.Compilation.Parsing;
using Twofold.Api.SourceMapping;
using Twofold.Extensions;

namespace Twofold.Compilation.Parsing
{
    public class TemplateParser : ITemplateParser
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

        public List<AsbtractCodeFragment> Parse(string name, string text)
        {
            List<AsbtractCodeFragment> fragments = new List<AsbtractCodeFragment>();
            FileLine fileLine = new FileLine(text, 0, 0, 0, new TextFilePosition(name, new TextPosition(0, 0)));

            while (fileLine.BeginIndex != text.Length) {
                ++fileLine.Position.Line;

                fileLine.BeginIndexNonSpace = text.IndexOfNot(fileLine.BeginIndex, text.Length, CharExtensions.IsSpace);
                if (fileLine.BeginIndexNonSpace == -1) {
                    fileLine.BeginIndexNonSpace = text.Length;
                }

                fileLine.EndIndex = text.IndexOf(fileLine.BeginIndexNonSpace, text.Length, CharExtensions.IsNewline);
                if (fileLine.EndIndex == -1) {
                    fileLine.EndIndex = text.Length;
                }

                IParserRule parserRule;
                if (parseRules.TryGetValue(text[fileLine.EndIndex], out parserRule)) {
                    List<AsbtractCodeFragment> ruleFragments = parserRule.Parse(fileLine, messageHandler);
                    fragments.AddRange(ruleFragments);
                }

                if (fileLine.EndIndex == text.Length) {
                    break;
                }

                char complementaryNewLineChar = ((text[fileLine.EndIndex] == '\n') ? '\r' : '\n');
                fileLine.BeginIndex = text.IndexOfNot(fileLine.EndIndex + 1, text.Length, c => c == complementaryNewLineChar);
                if (fileLine.BeginIndex == -1) {
                    fileLine.BeginIndex = text.Length;
                }
            }
            return fragments;
        }
    }
}
