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

namespace HicknHack.Twofold.Compilation.Rules
{
    using Abstractions;
    using Abstractions.Compilation;
    using System.Collections.Generic;

    /// <summary>
    /// Rule which handles template interpolation and appends a new line to output: "| #{...} ... #{...}"
    /// </summary>
    internal class InterpolationLineRule : InterpolationRule
    {
        public override List<AsbtractRenderCommand> Parse(FileLine line, IMessageHandler messageHandler)
        {
            List<AsbtractRenderCommand> commands = base.Parse(line, messageHandler);
            var newLineSpan = line.CreateOriginalTextSpan(line.BeginNonSpace, line.BeginNonSpace + 1);
            commands.Add(new NewLineCommand(newLineSpan));
            return commands;
        }
    }
}