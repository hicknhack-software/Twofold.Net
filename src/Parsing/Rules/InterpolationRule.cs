using System;
using Twofold.Api;

namespace Twofold.Parsing.Rules
{
    public class InterpolationRule : AbstractParserRule
    {
        public InterpolationRule(IMessageHandler messageHandler)
            : base(messageHandler)
        { }

        public override void Parse(string value, int beginIndex, int nonSpaceBeginIndex, int endIndex, ICodeGenerator codeGenerator)
        {
            throw new NotImplementedException();
        }
    }
}
