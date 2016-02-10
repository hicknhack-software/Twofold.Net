﻿using System.Collections.Generic;
using System.Diagnostics;
using Twofold.Api;
using Twofold.Api.Compilation.Generation;
using Twofold.Api.Compilation.Parsing;
using Twofold.Extensions;

namespace Twofold.Compilation.Parsing
{
    public class InterpolationRule : IParserRule
    {
        public List<AsbtractCodeFragment> Parse(FileLine line, IMessageHandler messageHandler)
        {
            List<AsbtractCodeFragment> fragments = new List<AsbtractCodeFragment>();

            var beginIndexIndent = line.BeginIndexNonSpace + 1; //skip matched character
            var index = line.Text.IndexOfNot(beginIndexIndent, line.EndIndex, CharExtensions.IsSpace);
            if (index == -1) {
                index = line.EndIndex;
            }
            fragments.Add(new TargetIndentation(line, new TextSpan(beginIndexIndent, index, line.Text)));

            var endIndex = index;
            while (index < line.EndIndex) {
                index = line.Text.IndexOf(index, line.EndIndex, (ch) => ch == '#');
                if (index == -1) { // reached line end
                    break;
                }

                var expressionBeginIndex = index + 1; //skip #
                if (expressionBeginIndex == line.EndIndex) { // reached line end
                    break;
                }

                switch (line.Text[expressionBeginIndex]) {
                    case '#':
                        fragments.Add(new OriginText(line, new TextSpan(expressionBeginIndex, expressionBeginIndex + 1, line.Text)));
                        index = endIndex = expressionBeginIndex + 1;
                        continue;

                    case '{':
                        var expressionEndIndex = BraceCounter.MatchBraces(expressionBeginIndex, line.EndIndex, line.Text);
                        if (expressionEndIndex == -1) {
                            endIndex = line.EndIndex;
                            messageHandler.TemplateMessage(TraceLevel.Error, line.Position, "Missing a closing '}'.");
                            break;
                        }
                        fragments.Add(new OriginExpression(line, new TextSpan(expressionBeginIndex + 1, expressionEndIndex - 1, line.Text)));
                        index = endIndex = expressionEndIndex + 1;
                        continue;

                    default:
                        index = endIndex = expressionBeginIndex + 1;
                        continue;
                }
            }
            fragments.Add(new OriginText(line, new TextSpan(endIndex, line.EndIndex, line.Text)));
            return fragments;
        }
    }
}
