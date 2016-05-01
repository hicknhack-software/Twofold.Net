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
    using Interface.Compilation;
    using Interface.SourceMapping;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using TextRendering;

    internal sealed class CSharpGenerator : AbstractCodeGenerator
    {
        private static readonly char[] HexDigit = "0123456789abcdef".ToCharArray();
        private readonly TextRenderer TextRenderer;
        private readonly SourceMap SourceMap;
        private readonly List<string> IncludedFiles;

        public CSharpGenerator(TemplateParser templateParser, TextWriter textWriter, SourceMap sourceMap, List<string> includedFiles)
            : base(templateParser)
        {
            this.TextRenderer = new TextRenderer(textWriter);
            this.SourceMap = sourceMap;
            this.IncludedFiles = includedFiles;
        }

        protected override void Generate(TargetIndentation command)
        {
            //if (fragment.Span.IsEmpty) {
            //    return;
            //}

            if (command.IndentationSpan.IsEmpty == false)
            {
                var indentationPosition = command.Line.CreateFilePosition(command.IndentationSpan);
                this.TextRenderer.WriteLine($"TargetRenderer.AddMapping(new TextFilePosition(\"{indentationPosition.SourceName}\", new TextPosition({indentationPosition.Line}, {indentationPosition.Column})));");
            }

            this.SourceMap.AddMapping(this.TextRendererPosition(), command.Line.CreateFilePosition(command.BeginSpan));
            this.TextRenderer.Write("TargetRenderer.PartIndentation(\"");

            if (command.IndentationSpan.IsEmpty == false)
            {
                //TODO: Interpolation 1:1
                this.SourceMap.AddMapping(this.TextRendererPosition(), command.Line.CreateFilePosition(command.IndentationSpan));
                var escapedText = this.EscapeString(command.IndentationSpan.Text);
                this.TextRenderer.Write(escapedText);
            }

            this.SourceMap.AddMapping(this.TextRendererPosition(), command.Line.CreateFilePosition(command.EndSpan));
            this.TextRenderer.WriteLine("\");");

            this.TextRenderer.WriteLine();
        }

        protected override void Generate(TargetPopIndentation command)
        {
            this.SourceMap.AddMapping(this.TextRendererPosition(), command.Line.CreateFilePosition(command.IndentationSpan));
            this.TextRenderer.WriteLine("TargetRenderer.PopIndentation();");
            this.TextRenderer.WriteLine();
        }

        protected override void Generate(TargetPushIndentation command)
        {
            if (command.IndentationSpan.IsEmpty == false)
            {
                var indentationPosition = command.Line.CreateFilePosition(command.IndentationSpan);
                this.TextRenderer.WriteLine($"TargetRenderer.AddMapping(new TextFilePosition(\"{indentationPosition.SourceName}\", new TextPosition({indentationPosition.Line}, {indentationPosition.Column})));");
            }

            this.SourceMap.AddMapping(this.TextRendererPosition(), command.Line.CreateFilePosition(command.BeginSpan));
            this.TextRenderer.Write("TargetRenderer.PushIndentation(\"");

            if (command.IndentationSpan.IsEmpty == false)
            {
                //TODO: Interpolation 1:1
                this.SourceMap.AddMapping(this.TextRendererPosition(), command.Line.CreateFilePosition(command.IndentationSpan));
                var escapedText = this.EscapeString(command.IndentationSpan.Text);
                this.TextRenderer.Write(escapedText);
            }

            this.SourceMap.AddMapping(this.TextRendererPosition(), command.Line.CreateFilePosition(command.EndSpan));
            this.TextRenderer.WriteLine("\");");
            this.TextRenderer.WriteLine();
        }

        protected override void Generate(TargetNewLine command)
        {
            this.SourceMap.AddMapping(this.TextRendererPosition(), command.Line.CreateFilePosition(command.NewLineSpan));
            this.TextRenderer.WriteLine("TargetRenderer.WriteLine();");
            this.TextRenderer.WriteLine();
        }

        protected override void Generate(OriginScript command)
        {
            if (command.ScriptSpan.IsEmpty == false)
            {
                //TODO: Interpolation 1:1
                this.SourceMap.AddMapping(this.TextRendererPosition(), command.Line.CreateFilePosition(command.ScriptSpan));
                this.TextRenderer.WriteLine(command.ScriptSpan);
            }
        }

        protected override void Generate(OriginExpression command)
        {
            if (command.ExpressionSpan.IsEmpty)
            {
                return;
            }

            var expressionPosition = command.Line.CreateFilePosition(command.ExpressionSpan);
            this.TextRenderer.WriteLine($"TargetRenderer.AddMapping(new TextFilePosition(\"{expressionPosition.SourceName}\", new TextPosition({expressionPosition.Line}, {expressionPosition.Column})));");

            this.SourceMap.AddMapping(this.TextRendererPosition(), command.Line.CreateFilePosition(command.BeginSpan));
            this.TextRenderer.Write("TargetRenderer.PushPartIndentation();TargetRenderer.Write(() => ");

            //TODO: Interpolation 1:1
            this.SourceMap.AddMapping(this.TextRendererPosition(), command.Line.CreateFilePosition(command.ExpressionSpan));
            this.TextRenderer.Write(command.ExpressionSpan.Text);

            this.SourceMap.AddMapping(this.TextRendererPosition(), command.Line.CreateFilePosition(command.EndSpan));
            this.TextRenderer.WriteLine(");TargetRenderer.PopPartIndentation();");
            this.TextRenderer.WriteLine();
        }

        protected override void Generate(OriginText command)
        {
            if (command.TextSpan.IsEmpty)
            {
                return;
            }

            if(command.TextSpan.IsEmpty == false)
            {
                var textPosition = command.Line.CreateFilePosition(command.TextSpan);
                this.TextRenderer.WriteLine($"TargetRenderer.AddMapping(new TextFilePosition(\"{textPosition.SourceName}\", new TextPosition({textPosition.Line}, {textPosition.Column})));");
            }

            this.SourceMap.AddMapping(this.TextRendererPosition(), command.Line.CreateFilePosition(command.TextSpan));
            this.TextRenderer.Write("TargetRenderer.Write(() => \"");

            if (command.TextSpan.IsEmpty == false)
            {
                //TODO: Interpolation 1:1
                this.SourceMap.AddMapping(this.TextRendererPosition(), command.Line.CreateFilePosition(command.TextSpan));
                var escapedText = this.EscapeString(command.TextSpan.Text);
                this.TextRenderer.Write(escapedText);
            }

            this.SourceMap.AddMapping(this.TextRendererPosition(), command.Line.CreateFilePosition(command.EndSpan));
            this.TextRenderer.WriteLine("\");");
            this.TextRenderer.WriteLine();
        }

        protected override void Generate(OriginPragma command)
        {
            switch (command.Name)
            {
                case "include":
                    {
                        if (string.IsNullOrEmpty(command.Argument) == false)
                        {
                            IncludedFiles.Add(command.Argument);
                        }
                    }
                    break;

                default:
                    break;
            }

            //TODO: Interpolation 1:1
            this.SourceMap.AddMapping(this.TextRendererPosition(), command.Line.CreateFilePosition(command.PragmaSpan));
            this.TextRenderer.WriteLine(command.PragmaSpan);
        }

        protected override void PreGeneration(string sourceName, string text)
        {
            this.TextRenderer.WriteLine($"// {sourceName}");

            foreach (string targetCodeUsing in Constants.TargetCodeUsings)
            {
                this.TextRenderer.WriteLine(targetCodeUsing);
            }
            this.TextRenderer.WriteLine($"#line 1 \"{sourceName}\"");
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
                                        var c1 = HexDigit[(ch >> 12) & 0x0F];
                                        var c2 = HexDigit[(ch >> 8) & 0x0F];
                                        var c3 = HexDigit[(ch >> 4) & 0x0F];
                                        var c4 = HexDigit[ch & 0x0F];
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

        private TextPosition TextRendererPosition() => new TextPosition(this.TextRenderer.Line, this.TextRenderer.Column);

    }
}