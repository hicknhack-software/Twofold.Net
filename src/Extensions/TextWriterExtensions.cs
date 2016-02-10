using System.IO;

namespace Twofold.Extensions
{
    public static class TextWriterExtensions
    {
        /// <exception cref="ArgumentOutOfRangeException">beginIndex is greater than endIndex.</exception>
        /// <exception cref="ArgumentOutOfRangeException">endIndex is greater than string length.</exception>
        public static void Write(this TextWriter textWriter, int beginIndex, int endIndex, string text)
        {
            var writeText = text.Substring(beginIndex, endIndex - beginIndex);
            textWriter.Write(writeText);
        }
    }
}
