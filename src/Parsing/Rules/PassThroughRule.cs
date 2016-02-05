using System;
using Twofold.Api;

namespace Twofold.Parsing.Rules
{
    public class PassThroughRule : AbstractParserRule
    {
        public PassThroughRule(IMessageHandler messageHandler)
            : base(messageHandler)
        { }

        public override void Parse(string value, int beginIndex, int nonSpaceBeginIndex, int endIndex, ICodeGenerator codeGenerator)
        {
            throw new NotImplementedException();
        }
    }
}
