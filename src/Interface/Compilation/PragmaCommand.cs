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

namespace Twofold.Interface.Compilation
{
    /// <summary>
    /// A pargma directive in the template.
    /// </summary>
    public class PragmaCommand : AsbtractRenderCommand
    {
        public readonly TextSpan PragmaSpan;
        public readonly string Name;
        public readonly string Argument;

        public PragmaCommand(FileLine line, TextSpan pragmaSpan, string name, string argument)
            : base(RenderCommands.Pragma, line)
        {
            this.PragmaSpan = pragmaSpan;
            this.Name = name;
            this.Argument = argument;
        }
    }
}