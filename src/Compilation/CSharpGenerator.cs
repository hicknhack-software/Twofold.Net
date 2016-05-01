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
        private readonly List<string> IncludedFiles;

        public CSharpGenerator(TemplateParser templateParser, TextWriter textWriter, SourceMap sourceMap, List<string> includedFiles)
            : base(templateParser)
        {
            this.TextRenderer = new TextRenderer(textWriter, sourceMap);
            this.IncludedFiles = includedFiles;
        }

        protected override void Generate(TargetIndentation command)
        {
            this.TextRenderer.Write("TargetRenderer.PartIndentation(\"", command.Line.CreateFilePosition(command.BeginSpan));

            //TODO: Interpolation 1:1
            var escapedText = this.EscapeString(command.IndentationSpan.Text);
            var indentationPosition = command.Line.CreateFilePosition(command.IndentationSpan);
            this.TextRenderer.Write(escapedText, indentationPosition);

            this.TextRenderer.WriteLine($"\", {this.X(indentationPosition)});", command.Line.CreateFilePosition(command.EndSpan));

            this.TextRenderer.WriteLine();
        }

        protected override void Generate(TargetPopIndentation command)
        {
            this.TextRenderer.WriteLine("TargetRenderer.PopIndentation();", command.Line.CreateFilePosition(command.IndentationSpan));
            this.TextRenderer.WriteLine();
        }

        protected override void Generate(TargetPushIndentation command)
        {
            this.TextRenderer.Write("TargetRenderer.PushIndentation(\"", command.Line.CreateFilePosition(command.BeginSpan));

            //TODO: Interpolation 1:1
            var escapedText = this.EscapeString(command.IndentationSpan.Text);
            var indentationPosition = command.Line.CreateFilePosition(command.IndentationSpan);
            this.TextRenderer.Write(escapedText, indentationPosition);


            this.TextRenderer.WriteLine($"\", {this.X(indentationPosition)});", command.Line.CreateFilePosition(command.EndSpan));
            this.TextRenderer.WriteLine();
        }

        protected override void Generate(TargetNewLine command)
        {
            this.TextRenderer.WriteLine("TargetRenderer.WriteLine();", command.Line.CreateFilePosition(command.NewLineSpan));
            this.TextRenderer.WriteLine();
        }

        protected override void Generate(OriginScript command)
        {
            if (command.ScriptSpan.IsEmpty)
            {
                return;
            }

            //TODO: Interpolation 1:1
            this.TextRenderer.WriteLine(command.ScriptSpan, command.Line.CreateFilePosition(command.ScriptSpan));
        }

        protected override void Generate(OriginExpression command)
        {
            if (command.ExpressionSpan.IsEmpty)
            {
                return;
            }

            this.TextRenderer.Write("TargetRenderer.PushPartIndentation();TargetRenderer.Write(() => ", command.Line.CreateFilePosition(command.BeginSpan));

            //TODO: Interpolation 1:1
            this.TextRenderer.Write(command.ExpressionSpan.Text, command.Line.CreateFilePosition(command.ExpressionSpan));

            var expressionPosition = command.Line.CreateFilePosition(command.ExpressionSpan);
            this.TextRenderer.WriteLine($", {this.X(expressionPosition)});TargetRenderer.PopPartIndentation();", command.Line.CreateFilePosition(command.EndSpan));
            this.TextRenderer.WriteLine();
        }

        protected override void Generate(OriginText command)
        {
            if (command.TextSpan.IsEmpty)
            {
                return;
            }

            this.TextRenderer.Write("TargetRenderer.Write(() => \"", command.Line.CreateFilePosition(command.TextSpan));

            //TODO: Interpolation 1:1
            var escapedText = this.EscapeString(command.TextSpan.Text);
            var textPosition = command.Line.CreateFilePosition(command.TextSpan);
            this.TextRenderer.Write(escapedText, textPosition);

            this.TextRenderer.WriteLine($"\", {this.X(textPosition)});", command.Line.CreateFilePosition(command.EndSpan));
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
            this.TextRenderer.WriteLine(command.PragmaSpan, command.Line.CreateFilePosition(command.PragmaSpan));
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

        private string X(TextFilePosition position)
        {
            return $"new TextFilePosition(\"{position.SourceName}\", new TextPosition({position.Line}, {position.Column}))";
        }
    }
}