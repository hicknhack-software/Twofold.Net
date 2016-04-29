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

namespace Twofold.Compilation
{
    using Extensions;
    using Interface;
    using Interface.Compilation;
    using System.Collections.Generic;

    internal class CallRule : IParserRule
    {
        public List<AsbtractCodeFragment> Parse(FileLine line, IMessageHandler messageHandler)
        {
            List<AsbtractCodeFragment> fragments = new List<AsbtractCodeFragment>();

            var indentBegin = (line.BeginNonSpace + 1); //skip matched character
            var scriptBegin = line.Text.IndexOfNot(indentBegin, line.End, CharExtensions.IsSpace);

            fragments.Add(new TargetPushIndentation(line, new TextSpan(line.Text, indentBegin, scriptBegin)));
            fragments.Add(new OriginScript(line, new TextSpan(line.Text, scriptBegin, line.End)));
            fragments.Add(new TargetPopIndentation(line, new TextSpan(line.Text, line.End, line.End)));
            return fragments;
        }
    }
}