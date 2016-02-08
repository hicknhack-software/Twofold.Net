using System.Globalization;

namespace Twofold.Extensions
{
    public static class CharExtensions
    {
        public static bool IsSpace(char ch)
        {
            return (char.GetUnicodeCategory(ch) == UnicodeCategory.SpaceSeparator) || (ch == '\t');
        }

        public static bool IsNewline(char ch)
        {
            return (ch == '\n') || (ch == '\r');
        }
    }
}
