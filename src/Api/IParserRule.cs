namespace Twofold.Api
{
    public interface IParserRule
    {
        void Parse(string line, int beginIndex, int nonSpaceBeginIndex, int endIndex, ICodeGenerator codeGenerator, IMessageHandler messageHandler);
    }
}
