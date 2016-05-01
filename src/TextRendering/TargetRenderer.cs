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

namespace Twofold.TextRendering
{
    using Interface;
    using Interface.SourceMapping;
    using System;
    using System.IO;

    /// <summary>
    /// This is the renderer which will be exposed to the generated
    /// CSharp target code.
    /// </summary>
    public static class TargetRenderer
    {
        private static TextRenderer renderer;

        internal static void SetTextWriter(TextWriter textWriter, SourceMap sourceMap)
        {
            TargetRenderer.renderer = new TextRenderer(textWriter, sourceMap);
        }

        /// <summary>
        /// Executes the given function which must return a string and appends
        /// the returned string to the text buffer.
        /// </summary>
        /// <param name="func">The function to execute.</param>
        public static void Execute(Func<string> func, TextFilePosition source)
        {
            if (TargetRenderer.renderer == null)
            {
                return;
            }
            TargetRenderer.renderer.PushCaller(source);
            string text = func();
            TargetRenderer.renderer.PopCaller();

            var textSpan = new TextSpan(text);
            TargetRenderer.renderer.Write(textSpan, source);
        }

        /// <summary>
        /// Executes the given function which must return void.
        /// </summary>
        /// <param name="action">The function to execute.</param>
        public static void Execute(Action action, TextFilePosition source)
        {
            if (TargetRenderer.renderer == null)
            {
                return;
            }
            TargetRenderer.renderer.PushCaller(source);
            action();
            TargetRenderer.renderer.PopCaller();
        }

        public static void WriteLine()
        {
            if (TargetRenderer.renderer == null)
            {
                return;
            }
            TargetRenderer.renderer.WriteLine();
        }

        public static void PushIndentation(string indentation, TextFilePosition source)
        {
            if (TargetRenderer.renderer == null)
            {
                return;
            }
            TargetRenderer.renderer.PushIndentation(indentation, source);
        }

        public static void PopIndentation()
        {
            if (TargetRenderer.renderer == null)
            {
                return;
            }
            TargetRenderer.renderer.PopIndentation();
        }

        public static void LocalIndentation(string indentation, TextFilePosition source)
        {
            if (TargetRenderer.renderer == null)
            {
                return;
            }
            TargetRenderer.renderer.LocalIndentation(indentation, source);
        }

        public static void PushLocalIndentation()
        {
            if (TargetRenderer.renderer == null)
            {
                return;
            }
            TargetRenderer.renderer.PushLocalIndentation();
        }

        public static void PopLocalIndentation()
        {
            if (TargetRenderer.renderer == null)
            {
                return;
            }
            TargetRenderer.renderer.PopLocalIndentation();
        }

        public static void PushCaller(TextFilePosition source)
        {
            if (TargetRenderer.renderer == null)
            {
                return;
            }
            TargetRenderer.renderer.PushCaller(source);
        }

        public static void PopCaller()
        {
            if (TargetRenderer.renderer == null)
            {
                return;
            }
            TargetRenderer.renderer.PopCaller();
        }
    }
}