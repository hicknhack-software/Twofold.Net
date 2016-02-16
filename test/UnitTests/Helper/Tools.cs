using Twofold.Interface;
using Twofold.Interface.SourceMapping;

namespace UnitTests.Helper
{
    public static class Tools
    {
        public static FileLine CreateFileLine(string text, int beginNonSpace)
        {
            return new FileLine(text, 0, beginNonSpace, text.Length, new TextFilePosition("Test", new TextPosition(1, 1)));
        }
    }
}
