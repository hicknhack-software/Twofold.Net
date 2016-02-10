using System.Collections.Generic;
using Twofold.Api;
using Twofold.Api.Compilation.Generation;
using Twofold.Api.Compilation.Parsing;

namespace Twofold.Compilation.Parsing
{
    public class PassThroughRule : IParserRule
    {
        public List<AsbtractCodeFragment> Parse(FileLine line, IMessageHandler messageHandler)
        {
            List<AsbtractCodeFragment> fragments = new List<AsbtractCodeFragment>();
            fragments.Add(new OriginScript(line, new TextSpan(line.BeginIndex, line.EndIndex, line.Text)));
            return fragments;
        }
    }
}
