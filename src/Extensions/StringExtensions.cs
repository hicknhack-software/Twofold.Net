using System;

namespace Twofold.Extensions
{
    public static class StringExtensions
    {
        /// <exception cref="ArgumentOutOfRangeException">beginIndex is greater than endIndex.</exception>
        /// <exception cref="ArgumentOutOfRangeException">endIndex is greater than string length.</exception>
        public static int IndexOf(this string value, int beginIndex, int endIndex, Func<char, bool> predicate)
        {
            if (beginIndex > endIndex) {
                throw new ArgumentOutOfRangeException("beginIndex", "Must be less equal than endIndex.");
            }
            if (endIndex > value.Length) {
                throw new ArgumentOutOfRangeException("endIndex", "endIndex must be less equal string length.");
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

        /// <exception cref="ArgumentOutOfRangeException">beginIndex is greater than endIndex.</exception>
        /// <exception cref="ArgumentOutOfRangeException">endIndex is greater than string length.</exception>
        public static int IndexOfNot(this string value, int beginIndex, int endIndex, Func<char, bool> predicate)
        {
            if (beginIndex > endIndex) {
                throw new ArgumentOutOfRangeException("beginIndex", "Must be less equal than endIndex.");
            }
            if (endIndex > value.Length) {
                throw new ArgumentOutOfRangeException("endIndex", "endIndex must be less equal string length.");
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
