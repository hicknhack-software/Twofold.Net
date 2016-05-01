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

namespace Twofold.Interface
{
    using System;
    using System.Reflection;

    public class CompiledTemplate
    {
        public readonly Assembly @Assembly;
        public readonly string SourceName;
        public readonly string MainTypeName;

        public CompiledTemplate(string sourceName, Assembly assembly, string mainTypeName)
        {
            if (string.IsNullOrEmpty(sourceName))
            {
                throw new ArgumentException("Can't be null or empty.", nameof(sourceName));
            }
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }
            if (string.IsNullOrEmpty(mainTypeName))
            {
                throw new ArgumentException("Can't be null or empty.", nameof(mainTypeName));
            }

            this.SourceName = sourceName;
            this.Assembly = assembly;
            this.MainTypeName = mainTypeName;
        }
    }
}