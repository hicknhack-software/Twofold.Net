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
using System.Collections.Generic;
using Twofold.Api;
using Twofold.Api.Compilation.Generation;
using Twofold.Api.Compilation.Parsing;
using Twofold.Extensions;

namespace Twofold.Compilation.Parsing
{
    public class CallRule : IParserRule
    {
        public List<AsbtractCodeFragment> Parse(FileLine line, IMessageHandler messageHandler)
        {
            List<AsbtractCodeFragment> fragments = new List<AsbtractCodeFragment>();

            var indexBeginIndent = (line.BeginIndexNonSpace + 1); //skip matched character
            var scriptBeginIndex = line.Text.IndexOfNot(indexBeginIndent, line.EndIndex, CharExtensions.IsSpace);
            if (scriptBeginIndex == line.EndIndex) {
                return fragments;
            }

            fragments.Add(new TargetPushIndentation(line, new TextSpan(line.Text, indexBeginIndent, scriptBeginIndex)));
            fragments.Add(new OriginScript(line, new TextSpan(line.Text, scriptBeginIndex, line.EndIndex)));
            fragments.Add(new TargetPopIndentation(line, new TextSpan(line.Text, indexBeginIndent, scriptBeginIndex)));
            return fragments;
        }
    }
}
