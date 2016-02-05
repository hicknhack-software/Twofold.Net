using System;
using Twofold.Api;

namespace Twofold.Parsing
{
    public abstract class AbstractParserRule : IParserRule
    {
        readonly IMessageHandler messageHandler;
        readonly CSharpGenerator csharpGenerator;

        public IMessageHandler MessageHandler
        {
            get { return messageHandler; }
        }

        public CSharpGenerator CSharpGenerator
        {
            get { return csharpGenerator; }
        }

        public AbstractParserRule(IMessageHandler messageHandler, CSharpGenerator csharpGenerator)
        {
            this.messageHandler = messageHandler;
            this.csharpGenerator = csharpGenerator;

        }

        public abstract void Parse(string value, int beginIndex, int nonSpaceBeginIndex, int endIndex);
    }
}
