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

namespace HicknHack.Twofold.TextRendering
{
    using Abstractions.SourceMapping;
    using System;
    using System.IO;
    using Abstractions;

    /// <summary>
    /// This is the renderer which will be exposed to the generated
    /// CSharp target code.
    /// </summary>
    public static class TemplateRenderer
    {
        private static TextRenderer renderer;

        [HideStackTrace]
        internal static void SetTextWriter(TextWriter textWriter, Mapping sourceMap)
        {
            TemplateRenderer.renderer = new TextRenderer(textWriter, sourceMap);
        }

        /// <summary>
        /// Executes the given function which must return a string and appends
        /// the returned string to the text buffer.
        /// </summary>
        /// <param name="func">The function to execute.</param>
        [HideStackTrace]
        public static void Write(Func<string> func, TextFilePosition source, EntryFeatures features)
        {
            if (TemplateRenderer.renderer == null)
            {
                return;
            }
            TemplateRenderer.renderer.PushCaller(source);
            string text = func();
            TemplateRenderer.renderer.PopCaller();

            TemplateRenderer.renderer.Write(text, source, features);
        }

        /// <summary>
        /// Executes the given function which must return void.
        /// </summary>
        /// <param name="action">The function to execute.</param>
        [HideStackTrace]
        public static void Write(Action action, TextFilePosition source, EntryFeatures features)
        {
            if (TemplateRenderer.renderer == null)
            {
                return;
            }
            TemplateRenderer.renderer.PushCaller(source);
            action();
            TemplateRenderer.renderer.PopCaller();

#pragma warning disable CS1717 // Assignment made to same variable
            features = features;
#pragma warning restore CS1717 // Assignment made to same variable
        }

        [HideStackTrace]
        public static void WriteLine(TextFilePosition source)
        {
            if (TemplateRenderer.renderer == null)
            {
                return;
            }
            TemplateRenderer.renderer.WriteLine(source);
        }

        [HideStackTrace]
        public static void PushIndentation(string indentation, TextFilePosition source, EntryFeatures features)
        {
            if (TemplateRenderer.renderer == null)
            {
                return;
            }
            TemplateRenderer.renderer.PushIndentation(indentation, source, features);
        }

        [HideStackTrace]
        public static void PopIndentation()
        {
            if (TemplateRenderer.renderer == null)
            {
                return;
            }
            TemplateRenderer.renderer.PopIndentation();
        }

        [HideStackTrace]
        public static void PushCaller(TextFilePosition source)
        {
            if (TemplateRenderer.renderer == null)
            {
                return;
            }
            TemplateRenderer.renderer.PushCaller(source);
        }

        [HideStackTrace]
        public static void PopCaller()
        {
            if (TemplateRenderer.renderer == null)
            {
                return;
            }
            TemplateRenderer.renderer.PopCaller();
        }
    }
}