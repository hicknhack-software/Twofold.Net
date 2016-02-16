﻿/* Twofold.Net
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
        public readonly int Begin;
        public readonly int BeginNonSpace;
        public readonly int End;
        public readonly TextFilePosition Position;

        public FileLine(string text, int begin, int beginNonSpace, int end, TextFilePosition position)
        {
            if (text == null) {
                throw new ArgumentNullException("text");
            }
            if (begin > beginNonSpace) {
                throw new ArgumentOutOfRangeException("begin", "Must be less equal than beginNonSpace.");
            }
            if (beginNonSpace > end) {
                throw new ArgumentOutOfRangeException("beginNonSpace", "Must be less equal than end.");
            }
            if (end > text.Length) {
                throw new ArgumentOutOfRangeException("end", "end must be less equal string length.");
            }

            Text = text;
            Begin = begin;
            BeginNonSpace = beginNonSpace;
            End = end;
            Position = position;
        }
    }
}