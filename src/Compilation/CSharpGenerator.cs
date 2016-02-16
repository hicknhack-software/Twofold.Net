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
using Microsoft.CSharp;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Twofold.Interface;
using Twofold.Interface.Compilation;
using Twofold.TextRendering;

namespace Twofold.Compilation
{
    internal sealed class CSharpGenerator : AbstractCodeGenerator
    {
        static readonly char[] hexDigit = "0123456789abcdef".ToCharArray();
        readonly TextRenderer textWriterController;
        readonly TextWriter textWriter;
        readonly List<string> includedFiles;


        public CSharpGenerator(TemplateParser templateParser, TextWriter textWriter, List<string> includedFiles)
            : base(templateParser)
        {
            this.textWriterController = new TextRenderer();
            this.textWriter = textWriter;
            this.includedFiles = includedFiles;
        }

        protected override void Generate(TargetIndentation fragment)
        {
            textWriterController.Append(fragment.Span, textWriter);
            textWriterController.AppendNewLine(textWriter);
        }

        protected override void Generate(TargetPopIndentation fragment)
        {
            textWriterController.Append(fragment.Span, textWriter);
            textWriterController.AppendNewLine(textWriter);
        }

        protected override void Generate(TargetPushIndentation fragment)
        {
            textWriterController.Append(fragment.Span, textWriter);
            textWriterController.AppendNewLine(textWriter);
        }

        protected override void Generate(TargetNewLine fragment)
        {
            string renderCmd = $"TargetRenderer.NewLine();";
            textWriterController.Append(new TextSpan(renderCmd), textWriter);
        }

        protected override void Generate(OriginScript fragment)
        {
            textWriterController.Append(fragment.Span, textWriter);
            textWriterController.AppendNewLine(textWriter);
        }

        protected override void Generate(OriginExpression fragment)
        {
            if (fragment.Span.IsEmpty) {
                return;
            }

            string renderCmd = $"TargetRenderer.Append({fragment.Span.Text});";
            textWriterController.Append(new TextSpan(renderCmd), textWriter);
            textWriterController.AppendNewLine(textWriter);
        }

        protected override void Generate(OriginText fragment)
        {
            if (fragment.Span.IsEmpty) {
                return;
            }

            string escapedContent = this.EscapeString(fragment.Span.Text);
            string renderCmd = $"TargetRenderer.Append(\"{escapedContent}\");";
            textWriterController.Append(new TextSpan(renderCmd), textWriter);
            textWriterController.AppendNewLine(textWriter);
        }

        protected override void Generate(OriginPragma fragment)
        {
            switch (fragment.Name) {
                case "include": {
                        if (string.IsNullOrEmpty(fragment.Argument) == false) {
                            includedFiles.Add(fragment.Argument);
                        }
                    }
                    break;

                default:
                    break;
            }
            textWriterController.Append(fragment.Span, textWriter);
            textWriterController.AppendNewLine(textWriter);
        }

        protected override void PreGeneration(string sourceName, string text)
        {
            var lineDirective = $"#line 1 \"{sourceName}\"";
            textWriterController.Append(new TextSpan(lineDirective), textWriter);
            textWriterController.AppendNewLine(textWriter);

            foreach (string targetCodeUsing in Constants.TargetCodeUsings) {
                textWriterController.Append(new TextSpan(targetCodeUsing), textWriter);
                textWriterController.AppendNewLine(textWriter);
            }
        }

        string EscapeString(string text)
        {
            var sb = new StringBuilder(text.Length);
            var len = text.Length;
            for (int c = 0; c < len; ++c) {
                char ch = text[c];
                switch (ch) {
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
                    default: {
                            switch (char.GetUnicodeCategory(ch)) {
                                case UnicodeCategory.Control: {
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
