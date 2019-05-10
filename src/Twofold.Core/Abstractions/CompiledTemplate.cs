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

namespace HicknHack.Twofold.Abstractions
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Twofold.Compilation;

    public class CompiledTemplate
    {
        public readonly Assembly @Assembly;
        public readonly string OriginalName;
        public readonly string MainTypeName;
        public readonly List<GeneratedCode> GeneratedCodes;

        public CompiledTemplate(string sourceName, List<GeneratedCode> generatedCodes)
        {
            this.OriginalName = sourceName ?? throw new ArgumentNullException(nameof(sourceName));

            this.MainTypeName = string.Empty;
            this.GeneratedCodes = generatedCodes ?? throw new ArgumentNullException(nameof(generatedCodes));
        }

        public CompiledTemplate(string sourceName, Assembly assembly, string mainTypeName, List<GeneratedCode> generatedCodes)
        {
            this.OriginalName = sourceName ?? throw new ArgumentNullException(nameof(sourceName));
            this.Assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
            this.MainTypeName = mainTypeName ?? throw new ArgumentNullException(nameof(mainTypeName));
            this.GeneratedCodes = generatedCodes ?? throw new ArgumentNullException(nameof(generatedCodes));
        }

        public bool IsValid
        {
            get { return this.Assembly != null; }
        }
    }
}