using System;
using Twofold.Api;

namespace Twofold.Parsing
{
    public abstract class AbstractParserRule : IParserRule
    {
        readonly IMessageHandler messageHandler;

        public IMessageHandler MessageHandler
        {
            get { return messageHandler; }
        }

        public AbstractParserRule(IMessageHandler messageHandler)
        {
            this.messageHandler = messageHandler;
        }

        public abstract void Parse(string value, int beginIndex, int nonSpaceBeginIndex, int endIndex, ICodeGenerator codeGenerator);
    }
}
