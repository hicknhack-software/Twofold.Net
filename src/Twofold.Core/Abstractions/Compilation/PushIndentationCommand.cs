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

namespace HicknHack.Twofold.Abstractions.Compilation
{
    public class PushIndentationCommand : AsbtractRenderCommand
    {
        public readonly OriginalTextSpan Begin;
        public readonly OriginalTextSpan Indentation;
        public readonly OriginalTextSpan End;

        public PushIndentationCommand(OriginalTextSpan begin, OriginalTextSpan indentation, OriginalTextSpan end)
            : base(RenderCommands.PushIndentation)
        {
            this.Begin = begin;
            this.Indentation = indentation;
            this.End = end;
        }
    }
}