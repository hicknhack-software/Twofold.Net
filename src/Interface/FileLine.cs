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
using Twofold.Interface.SourceMapping;

namespace Twofold.Interface
{
    public struct FileLine
    {
        public readonly string Text;
        public readonly int BeginIndex;
        public readonly int BeginIndexNonSpace;
        public readonly int EndIndex;
        public readonly TextFilePosition Position;

        public FileLine(string text, int beginIndex, int beginIndexNonSpace, int endIndex, TextFilePosition position)
        {
            if (text == null) {
                throw new ArgumentNullException("text");
            }
            if (beginIndex > beginIndexNonSpace) {
                throw new ArgumentOutOfRangeException("beginIndex", "Must be less equal than beginIndexNonSpace.");
            }
            if (beginIndexNonSpace > endIndex) {
                throw new ArgumentOutOfRangeException("beginIndexNonSpace", "Must be less equal than endIndex.");
            }
            if (endIndex > text.Length) {
                throw new ArgumentOutOfRangeException("endIndex", "endIndex must be less equal string length.");
            }

            Text = text;
            BeginIndex = beginIndex;
            BeginIndexNonSpace = beginIndexNonSpace;
            EndIndex = endIndex;
            Position = position;
        }
    }
}
