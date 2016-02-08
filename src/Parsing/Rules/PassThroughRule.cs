using System;
using Twofold.Api;

namespace Twofold.Parsing.Rules
{
    public class PassThroughRule : IParserRule
    {
        public void Parse(FileLine line, ICodeGenerator codeGenerator, IMessageHandler messageHandler)
        {
            throw new NotImplementedException();
        }
    }
}
