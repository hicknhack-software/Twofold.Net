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
    using Extensions;
    using Interface.Compilation;
    using Interface.SourceMapping;
    using System.Collections.Generic;
    using System.IO;
    using TextRendering;

    internal sealed class CSharpGenerator : AbstractCodeGenerator
    {
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
            var escapedText = command.IndentationSpan.Text.Escape();
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
            var escapedText = command.IndentationSpan.Text.Escape();
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
            var escapedText = command.TextSpan.Text.Escape();
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
            this.TextRenderer.WriteLine(command.PragmaSpan.Text, command.Line.CreateFilePosition(command.PragmaSpan));
        }

        protected override void PreGeneration(string templatePath, string text)
        {
            this.TextRenderer.WriteLine($"// {templatePath}");

            foreach (string targetCodeUsing in Constants.TargetCodeUsings)
            {
                this.TextRenderer.WriteLine(targetCodeUsing);
            }
            var escapedTemplatePath = templatePath.Escape();
            this.TextRenderer.WriteLine($"#line 1 \"{escapedTemplatePath}\"");
        }

        private TextPosition TextRendererPosition() => new TextPosition(this.TextRenderer.Line, this.TextRenderer.Column);

        private string X(TextFilePosition position)
        {
            var escapedSourceName = position.SourceName.Escape();
            return $"new TextFilePosition(\"{escapedSourceName}\", new TextPosition({position.Line}, {position.Column}))";
        }
    }
}