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

namespace HicknHack.Twofold.Compilation
{
    using Abstractions.SourceMapping;
    using System;

    public sealed class GeneratedCode
    {
        public readonly string TemplatePath;
        public readonly string Code;
        public readonly Mapping SourceMap;

        public GeneratedCode(string templatePath, string code, Mapping sourceMap)
        {
            this.TemplatePath = templatePath ?? throw new ArgumentNullException(nameof(templatePath));
            this.Code = code ?? throw new ArgumentNullException(nameof(code));
            this.SourceMap = sourceMap ?? throw new ArgumentNullException(nameof(sourceMap));
        }
    }
}