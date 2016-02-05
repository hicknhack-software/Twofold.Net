using System;

namespace Twofold.Extensions
{
    public static class StringExtensions
    {
        public static int IndexOf(this string value, int beginIndex, int endIndex, Func<char, bool> predicate)
        {
            if (beginIndex > value.Length) {
                throw new ArgumentOutOfRangeException("beginIndex");
            }
            if (beginIndex > endIndex) {
                throw new ArgumentOutOfRangeException("beginIndex", "Must be less than endIndex.");
            }
            if (endIndex > value.Length) {
                endIndex = value.Length;
            }
            var index = beginIndex;
            while (index < endIndex) {
                char ch = value[index];
                if (predicate(ch)) {
                    return index;
                }
                ++index;
            }
            return -1;
        }

        /// <exception cref="ArgumentOutOfRangeException">beginIndex is greater than string length.</exception>
        /// <exception cref="ArgumentOutOfRangeException">beginIndex is greater than endIndex.</exception>
        public static int IndexOfNot(this string value, int beginIndex, int endIndex, Func<char, bool> predicate)
        {
            if (beginIndex > value.Length) {
                throw new ArgumentOutOfRangeException("beginIndex");
            }
            if (beginIndex > endIndex) {
                throw new ArgumentOutOfRangeException("beginIndex", "Must be less than endIndex.");
            }
            if (endIndex > value.Length) {
                endIndex = value.Length;
            }
            var index = beginIndex;
            while (index < endIndex) {
                char ch = value[index];
                if (!predicate(ch)) {
                    return index;
                }
                ++index;
            }
            return -1;
        }
    }
}
