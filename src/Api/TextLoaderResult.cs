namespace Twofold.Api
{
    public struct TextLoaderResult
    {
        public string Name { get; private set; }
        public string Text { get; private set; }

        public TextLoaderResult(string name, string text)
        {
            Name = name;
            Text = text;
        }
    }
}
