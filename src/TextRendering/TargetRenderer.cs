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

namespace Twofold.TextRendering
{
    using Interface;
    using System;
    using System.IO;

    /// <summary>
    /// This is the renderer which will be exposed to the generated
    /// CSharp target code.
    /// </summary>
    public static class TargetRenderer
    {
        private static TextWriter textWriter = new StringWriter();
        private static readonly TextRenderer renderer = new TextRenderer();

        internal static void SetTextWriter(TextWriter newTextWriter)
        {
            if (newTextWriter == null)
            {
                throw new ArgumentNullException(nameof(newTextWriter));
            }
            TargetRenderer.textWriter = newTextWriter;
        }

        /// <summary>
        /// Executes the given function which must return a string and appends
        /// the returned string to the text buffer.
        /// </summary>
        /// <param name="func">The function to execute.</param>
        public static void Append(Func<string> func)
        {
            string text = func();
            var textSpan = new TextSpan(text);
            TargetRenderer.renderer.Append(textSpan, TargetRenderer.textWriter);
        }

        /// <summary>
        /// Executes the given function which must return void.
        /// </summary>
        /// <param name="action">The function to execute.</param>
        public static void Append(Action action)
        {
            action();
        }

        public static void NewLine()
        {
            TargetRenderer.renderer.AppendNewLine(TargetRenderer.textWriter);
        }

        public static void PushIndentation(string indentation)
        {
            TargetRenderer.renderer.PushIndentation(indentation);
        }

        public static void PopIndentation()
        {
            TargetRenderer.renderer.PopIndentation();
        }

        public static void PartIndentation(string indentation)
        {
            TargetRenderer.renderer.PartIndentation(indentation, TargetRenderer.textWriter);
        }

        public static void PushPartIndentation()
        {
            TargetRenderer.renderer.PushPartIndentation();
        }

        public static void PopPartIndentation()
        {
            TargetRenderer.renderer.PopPartIndentation();
        }
    }
}