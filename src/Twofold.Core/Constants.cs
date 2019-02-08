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

namespace HicknHack.Twofold
{
    internal static class Constants
    {
        public static readonly string[] CompilerOptions = {
            "/langversion:5",
            "/nowarn:1633",     //Supress: Unknown pragma, used for #pragma include
        };

        public static readonly string[] CompilerAssemblies = {
            "System.dll",
            "System.Core.dll",
        };

        public const string EntryMethodName = "TwofoldMain";

        public const string DefaultNamespace = "HicknHack.Twofold";

        public static readonly string[] TargetCodeUsings = {
            $"using _Template = {DefaultNamespace}.TextRendering.TemplateRenderer;",
            $"using _Source = {DefaultNamespace}.Abstractions.SourceMapping.TextFilePosition;",
            $"using _Features = {DefaultNamespace}.Abstractions.SourceMapping.EntryFeatures;",
        };
    }
}