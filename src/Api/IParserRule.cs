namespace Twofold.Api
{
    public interface IParserRule
    {
        void Parse(FileLine line, ICodeGenerator codeGenerator, IMessageHandler messageHandler);
    }
}
