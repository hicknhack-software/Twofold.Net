namespace Twofold.Api
{
    public interface IParserRule
    {
        void Parse(string value, int beginIndex, int nonSpaceBeginIndex, int endIndex, ICodeGenerator codeGenerator);
    }
}
