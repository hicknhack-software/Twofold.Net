using System.Collections.Generic;
using Twofold.Api;
using Twofold.Api.Compilation.Generation;
using Twofold.Api.Compilation.Parsing;
using Twofold.Extensions;

namespace Twofold.Compilation.Parsing
{
    public class CallRule : IParserRule
    {
        public List<AsbtractCodeFragment> Parse(FileLine line, IMessageHandler messageHandler)
        {
            List<AsbtractCodeFragment> fragments = new List<AsbtractCodeFragment>();

            var indexBeginIndent = (line.BeginIndexNonSpace + 1); //skip matched character
            var scriptBeginIndex = line.Text.IndexOfNot(indexBeginIndent, line.EndIndex, CharExtensions.IsSpace);
            if (scriptBeginIndex == -1) {
                return fragments;
            }

            fragments.Add(new TargetPushIndentation(line, new TextSpan(indexBeginIndent, scriptBeginIndex, line.Text)));
            fragments.Add(new OriginScript(line, new TextSpan(scriptBeginIndex, line.EndIndex, line.Text)));
            fragments.Add(new TargetPopIndentation(line, new TextSpan(indexBeginIndent, scriptBeginIndex, line.Text)));
            return fragments;
        }
    }
}
