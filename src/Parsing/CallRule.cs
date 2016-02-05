using System;
using Twofold.Api;

namespace Twofold.Parsing
{
    public class CallRule : AbstractParserRule
    {
        public CallRule(IMessageHandler messageHandler)
            : base(messageHandler)
        { }

        public override void Parse(string value, int beginIndex, int nonSpaceBeginIndex, int endIndex, ICodeGenerator codeGenerator)
        {
            throw new NotImplementedException();
        }
    }
}
