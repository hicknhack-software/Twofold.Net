using Twofold.Api.SourceMapping;

namespace Twofold.Api
{
    public class FileLine
    {
        public string Text { get; private set; }
        public int BeginIndex { get; set; }
        public int BeginIndexNonSpace { get; set; }
        public int EndIndex { get; set; }
        public TextFilePosition Position { get; set; }

        public FileLine(string text, int beginIndex, int beginIndexNonSpace, int endIndex, TextFilePosition position)
        {
            Text = text;
            BeginIndex = beginIndex;
            BeginIndexNonSpace = beginIndexNonSpace;
            EndIndex = endIndex;
            Position = position;
        }
    }
}
