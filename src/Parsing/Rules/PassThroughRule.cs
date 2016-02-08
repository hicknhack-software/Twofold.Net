using System;
using Twofold.Api;

namespace Twofold.Parsing.Rules
{
    public class PassThroughRule : IParserRule
    {
        public void Parse(string value, int beginIndex, int nonSpaceBeginIndex, int endIndex, ICodeGenerator codeGenerator, IMessageHandler messageHandler)
        {
            throw new NotImplementedException();
        }
    }
}
