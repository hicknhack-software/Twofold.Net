using System;
using Twofold.Api;

namespace Twofold.Parsing
{
    public class InterpolationRule : AbstractParserRule
    {
        public InterpolationRule(IMessageHandler messageHandler, CSharpGenerator csharpGenerator)
            : base(messageHandler, csharpGenerator)
        { }

        public override void Parse(string value, int beginIndex, int nonSpaceBeginIndex, int endIndex)
        {
            throw new NotImplementedException();
        }
    }
}
