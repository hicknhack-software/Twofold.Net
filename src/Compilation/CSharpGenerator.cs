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
    using System;
    using Interface;

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

        protected override void Generate(LocalIndentationCommand command)
        {
            this.TextRenderer.Write("TargetRenderer.LocalIndentation(\"", command.Line.CreateFilePosition(command.BeginSpan));

            var escapedText = command.IndentationSpan.Text.Escape();
            var indentationPosition = command.Line.CreateFilePosition(command.IndentationSpan);
            this.TextRenderer.Write(escapedText, indentationPosition); //TODO: Interpolation 1:1

            this.TextRenderer.WriteLine($"\", {this.X(indentationPosition)});", command.Line.CreateFilePosition(command.EndSpan));

            this.TextRenderer.WriteLine();
        }

        protected override void Generate(PushIndentationCommand command)
        {
            this.TextRenderer.Write("TargetRenderer.PushIndentation(\"", command.Line.CreateFilePosition(command.BeginSpan));

            var escapedText = command.IndentationSpan.Text.Escape();
            var indentationPosition = command.Line.CreateFilePosition(command.IndentationSpan);
            this.TextRenderer.Write(escapedText, indentationPosition);  //TODO: Interpolation 1:1

            this.TextRenderer.WriteLine($"\", {this.X(indentationPosition)});", command.Line.CreateFilePosition(command.EndSpan));
            this.TextRenderer.WriteLine();
        }

        protected override void Generate(PopIndentationCommand command)
        {
            this.TextRenderer.WriteLine("TargetRenderer.PopIndentation();", command.Line.CreateFilePosition(command.IndentationSpan));
            this.TextRenderer.WriteLine();
        }

        protected override void Generate(NewLineCommand command)
        {
            this.TextRenderer.WriteLine("TargetRenderer.WriteLine();", command.Line.CreateFilePosition(command.NewLineSpan));
            this.TextRenderer.WriteLine();
        }

        protected override void Generate(StatementCommand command)
        {
            if (command.StatementSpan.IsEmpty)
            {
                return;
            }

            var scriptPosition = command.Line.CreateFilePosition(command.StatementSpan);
            this.TextRenderer.WriteLine($"TargetRenderer.PushCaller({this.X(scriptPosition)});", scriptPosition);

            this.TextRenderer.WriteLine(command.StatementSpan, scriptPosition); //TODO: Interpolation 1:1

            var endPosition = command.Line.CreateFilePosition(command.EndSpan);
            this.TextRenderer.WriteLine($"TargetRenderer.PopCaller();", endPosition);
        }

        protected override void Generate(ScriptCommand command)
        {
            if (command.ScriptSpan.IsEmpty)
            {
                return;
            }

            var scriptPosition = command.Line.CreateFilePosition(command.ScriptSpan);
            this.TextRenderer.WriteLine(command.ScriptSpan, scriptPosition); //TODO: Interpolation 1:1
        }

        protected override void Generate(ExpressionCommand command)
        {
            if (command.ExpressionSpan.IsEmpty)
            {
                return;
            }

            this.TextRenderer.Write("TargetRenderer.PushLocalIndentation();");

            this.TextRenderer.Write("TargetRenderer.Execute(() => ", command.Line.CreateFilePosition(command.BeginSpan));
            this.TextRenderer.Write(command.ExpressionSpan.Text, command.Line.CreateFilePosition(command.ExpressionSpan)); //TODO: Interpolation 1:1
            var expressionPosition = command.Line.CreateFilePosition(command.ExpressionSpan);
            this.TextRenderer.Write($", {this.X(expressionPosition)});");

            this.TextRenderer.WriteLine("TargetRenderer.PopLocalIndentation();", command.Line.CreateFilePosition(command.EndSpan));
            this.TextRenderer.WriteLine();
        }

        protected override void Generate(TextCommand command)
        {
            if (command.TextSpan.IsEmpty)
            {
                return;
            }

            this.TextRenderer.Write("TargetRenderer.Execute(() => \"", command.Line.CreateFilePosition(command.TextSpan));

            var escapedText = command.TextSpan.Text.Escape();
            var textPosition = command.Line.CreateFilePosition(command.TextSpan);
            this.TextRenderer.Write(escapedText, textPosition); //TODO: Interpolation 1:1

            this.TextRenderer.WriteLine($"\", {this.X(textPosition)});", command.Line.CreateFilePosition(command.EndSpan));
            this.TextRenderer.WriteLine();
        }

        protected override void Generate(PragmaCommand command)
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

            this.TextRenderer.WriteLine(command.PragmaSpan.Text, command.Line.CreateFilePosition(command.PragmaSpan));  //TODO: Interpolation 1:1
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