using System.Collections.Generic;
using Twofold.Api;
using Twofold.Api.Compilation.Generation;

namespace Twofold.Compilation.Parsing
{
    public class InterpolateLineRule : InterpolationRule
    {
        public new List<AsbtractCodeFragment> Parse(FileLine line, IMessageHandler messageHandler)
        {
            List<AsbtractCodeFragment> fragments = base.Parse(line, messageHandler);
            fragments.Add(new TargetNewLine(line, new TextSpan(line.EndIndex, line.EndIndex, line.Text)));
            return fragments;
        }
    }
}
