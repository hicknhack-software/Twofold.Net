namespace HicknHack.Twofold.Tests.Helper
{
    using HicknHack.Twofold.Abstractions;
    using HicknHack.Twofold.Abstractions.SourceMapping;

    public static class Tools
    {
        public static FileLine CreateFileLine(string text, int beginNonSpace)
        {
            return new FileLine(text, 0, beginNonSpace, text.Length, new TextFilePosition("Test", new TextPosition(1, 1)));
        }
    }
}
