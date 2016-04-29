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
    using Interface;
    using Interface.Compilation;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using TextRendering;

    internal sealed class CSharpGenerator : AbstractCodeGenerator
    {
        private static readonly char[] hexDigit = "0123456789abcdef".ToCharArray();
        private readonly TextRenderer textRenderer;
        private readonly List<string> includedFiles;

        public CSharpGenerator(TemplateParser templateParser, TextWriter textWriter, List<string> includedFiles)
            : base(templateParser)
        {
            this.textRenderer = new TextRenderer(textWriter);
            this.includedFiles = includedFiles;
        }

        protected override void Generate(TargetIndentation fragment)
        {
            //if (fragment.Span.IsEmpty) {
            //    return;
            //}

            string escapedContent = this.EscapeString(fragment.Span.Text);
            string renderCmd = $"TargetRenderer.PartIndentation(\"{escapedContent}\");";
            textRenderer.Append(renderCmd);
            textRenderer.AppendNewLine();
        }

        protected override void Generate(TargetPopIndentation fragment)
        {
            string renderCmd = $"TargetRenderer.PopIndentation();";
            textRenderer.Append(renderCmd);
            textRenderer.AppendNewLine();
        }

        protected override void Generate(TargetPushIndentation fragment)
        {
            string escapedContent = this.EscapeString(fragment.Span.Text);
            string renderCmd = $"TargetRenderer.PushIndentation(\"{escapedContent}\");";
            textRenderer.Append(renderCmd);
            textRenderer.AppendNewLine();
        }

        protected override void Generate(TargetNewLine fragment)
        {
            string renderCmd = $"TargetRenderer.NewLine();";
            textRenderer.Append(renderCmd);
        }

        protected override void Generate(OriginScript fragment)
        {
            textRenderer.Append(fragment.Span);
            textRenderer.AppendNewLine();
        }

        protected override void Generate(OriginExpression fragment)
        {
            if (fragment.Span.IsEmpty)
            {
                return;
            }

            var renderCmdBuilder = new StringBuilder();
            renderCmdBuilder.Append("TargetRenderer.PushPartIndentation();");
            renderCmdBuilder.Append($"TargetRenderer.Append(() => {fragment.Span.Text});");
            renderCmdBuilder.Append("TargetRenderer.PopPartIndentation();");

            string renderCmd = renderCmdBuilder.ToString();
            textRenderer.Append(renderCmd);
            textRenderer.AppendNewLine();
        }

        protected override void Generate(OriginText fragment)
        {
            if (fragment.Span.IsEmpty)
            {
                return;
            }

            string escapedContent = this.EscapeString(fragment.Span.Text);
            string renderCmd = $"TargetRenderer.Append(() => \"{escapedContent}\");";
            textRenderer.Append(renderCmd);
            textRenderer.AppendNewLine();
        }

        protected override void Generate(OriginPragma fragment)
        {
            switch (fragment.Name)
            {
                case "include":
                    {
                        if (string.IsNullOrEmpty(fragment.Argument) == false)
                        {
                            includedFiles.Add(fragment.Argument);
                        }
                    }
                    break;

                default:
                    break;
            }
            textRenderer.Append(fragment.Span);
            textRenderer.AppendNewLine();
        }

        protected override void PreGeneration(string sourceName, string text)
        {
            var lineDirective = $"#line 1 \"{sourceName}\"";
            textRenderer.Append(lineDirective);
            textRenderer.AppendNewLine();

            foreach (string targetCodeUsing in Constants.TargetCodeUsings)
            {
                textRenderer.Append(targetCodeUsing);
                textRenderer.AppendNewLine();
            }
        }

        private string EscapeString(string text)
        {
            var sb = new StringBuilder(text.Length);
            var len = text.Length;
            for (int c = 0; c < len; ++c)
            {
                char ch = text[c];
                switch (ch)
                {
                    case '\'': sb.Append(@"\'"); break;
                    case '\"': sb.Append("\\\""); break;
                    case '\\': sb.Append(@"\\"); break;
                    case '\0': sb.Append(@"\0"); break;
                    case '\a': sb.Append(@"\a"); break;
                    case '\b': sb.Append(@"\b"); break;
                    case '\f': sb.Append(@"\f"); break;
                    case '\n': sb.Append(@"\n"); break;
                    case '\r': sb.Append(@"\r"); break;
                    case '\t': sb.Append(@"\t"); break;
                    case '\v': sb.Append(@"\v"); break;
                    default:
                        {
                            switch (char.GetUnicodeCategory(ch))
                            {
                                case UnicodeCategory.Control:
                                    {
                                        var c1 = hexDigit[(ch >> 12) & 0x0F];
                                        var c2 = hexDigit[(ch >> 8) & 0x0F];
                                        var c3 = hexDigit[(ch >> 4) & 0x0F];
                                        var c4 = hexDigit[ch & 0x0F];
                                        sb
                                            .Append(@"\x")
                                            .Append(c1)
                                            .Append(c2)
                                            .Append(c3)
                                            .Append(c4);
                                    }
                                    break;

                                default:
                                    sb.Append(ch);
                                    break;
                            }
                        }
                        break;
                }
            }
            return sb.ToString();
        }
    }
}