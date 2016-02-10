using System;

namespace Twofold.Api
{
    public class TextSpan
    {
        /// <summary>
        /// Begin of the text span in the original text.
        /// </summary>
        public readonly int BeginIndex;
        /// <summary>
        /// End + 1 of the text span in the original text.
        /// </summary>
        public readonly int EndIndex;
        /// <summary>
        /// The complete original text.
        /// </summary>
        public readonly string OriginalText;
        /// <summary>
        /// The text span from the original text.
        /// </summary>
        public readonly string Text;

        public bool IsEmpty { get { return (BeginIndex == EndIndex); } }

        public TextSpan(int beginIndex, int endIndex, string text)
        {
            if (beginIndex > endIndex) {
                throw new ArgumentOutOfRangeException("beginIndex", "Must be less equal than endIndex.");
            }
            if (text == null) {
                throw new ArgumentNullException("text");
            }
            if (endIndex > text.Length) {
                throw new ArgumentOutOfRangeException("endIndex", "endIndex must be less equal string length.");
            }
            BeginIndex = beginIndex;
            EndIndex = endIndex;
            OriginalText = text;
            Text = OriginalText.Substring(BeginIndex, EndIndex - BeginIndex);
        }
    }
}
