/* Twofold.Net
 * (C) Copyright 2016 HicknHack Software GmbH
 *
 * The original code can be found at:
 *     https://github.com/hicknhack-software/Twofold.Net
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System;
using System.IO;

namespace Twofold.Extensions
{
    public static class TextWriterExtensions
    {
        /// <exception cref="ArgumentOutOfRangeException">beginIndex is greater than endIndex.</exception>
        /// <exception cref="ArgumentOutOfRangeException">endIndex is greater than string length.</exception>
        /// <exception cref="ArgumentNullException">text is null.</exception>
        public static void Write(this TextWriter textWriter, int beginIndex, int endIndex, string text)
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
            var writeText = text.Substring(beginIndex, endIndex - beginIndex);
            textWriter.Write(writeText);
        }
    }
}
