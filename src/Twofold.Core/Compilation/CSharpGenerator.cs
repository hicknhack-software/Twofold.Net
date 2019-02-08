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

namespace HicknHack.Twofold.Compilation
{
    using Extensions;
    using Abstractions.Compilation;
    using Abstractions.SourceMapping;
    using System.Collections.Generic;
    using System.IO;
    using TextRendering;

    internal sealed class CSharpGenerator : AbstractCodeGenerator
    {
        private readonly TextRenderer TextRenderer;
        private readonly List<string> IncludedFiles;

        public CSharpGenerator(TemplateParser templateParser, TextWriter textWriter, Mapping mapping, List<string> includedFiles)
            : base(templateParser)
        {
            this.TextRenderer = new TextRenderer(textWriter, mapping);
            this.IncludedFiles = includedFiles;
        }

        protected override void Generate(PushIndentationCommand cmd)
        {
            var escapedText = cmd.Indentation.Text.Escape();
            this.TextRenderer.Write("_Template.PushIndentation(\"", cmd.Begin.Position);
            this.TextRenderer.Write(escapedText, cmd.Indentation.Position, EntryFeatures.ColumnInterpolation);
            this.TextRenderer.Write($"\", {this.SourceExpression(cmd.Indentation.Position)}, {this.FeatureExpression(EntryFeatures.ColumnInterpolation)});", cmd.End.Position);
            this.TextRenderer.WriteLine(cmd.End.Position);
        }

        protected override void Generate(PopIndentationCommand cmd)
        {
            this.TextRenderer.Write("_Template.PopIndentation();", cmd.Pop.Position);
            this.TextRenderer.WriteLine(cmd.Pop.Position);
        }

        protected override void Generate(NewLineCommand cmd)
        {
            this.TextRenderer.Write($"_Template.WriteLine({this.SourceExpression(cmd.NewLine.Position)});", cmd.NewLine.Position);
            this.TextRenderer.WriteLine(cmd.NewLine.Position);
        }

        protected override void Generate(StatementCommand cmd)
        {
            this.TextRenderer.Write($"_Template.PushCaller({this.SourceExpression(cmd.Statement.Position)});", cmd.Statement.Position);
            this.TextRenderer.Write(cmd.Statement.Text, cmd.Statement.Position, EntryFeatures.ColumnInterpolation);
            this.TextRenderer.Write($"_Template.PopCaller();", cmd.End.Position);
            this.TextRenderer.WriteLine(cmd.End.Position);
        }

        protected override void Generate(ScriptCommand cmd)
        {
            this.TextRenderer.Write(cmd.Script.Text, cmd.Script.Position, EntryFeatures.ColumnInterpolation);
            this.TextRenderer.WriteLine(cmd.End.Position);
        }

        protected override void Generate(ExpressionCommand cmd)
        {
            this.TextRenderer.Write("_Template.Write(() => ", cmd.Begin.Position);
            this.TextRenderer.Write(cmd.Expression.Text, cmd.Expression.Position, EntryFeatures.ColumnInterpolation);
            this.TextRenderer.Write($", {this.SourceExpression(cmd.Expression.Position)}, {this.FeatureExpression(EntryFeatures.None)});", cmd.End.Position);
            this.TextRenderer.WriteLine(cmd.End.Position);
        }

        protected override void Generate(TextCommand cmd)
        {
            var escapedText = cmd.Text.Text.Escape();
            this.TextRenderer.Write("_Template.Write(() => \"", cmd.Text.Position);
            this.TextRenderer.Write(escapedText, cmd.Text.Position, EntryFeatures.ColumnInterpolation);
            this.TextRenderer.Write($"\", {this.SourceExpression(cmd.Text.Position)}, {this.FeatureExpression(EntryFeatures.ColumnInterpolation)});", cmd.End.Position);
            this.TextRenderer.WriteLine(cmd.End.Position);
        }

        protected override void Generate(PragmaCommand cmd)
        {
            switch (cmd.Name)
            {
                case "include":
                    {
                        if (string.IsNullOrEmpty(cmd.Argument) == false)
                        {
                            this.IncludedFiles.Add(cmd.Argument);
                        }
                    }
                    break;

                default:
                    break;
            }

            this.TextRenderer.Write(cmd.Pragma.Text, cmd.Pragma.Position, EntryFeatures.ColumnInterpolation);
            this.TextRenderer.WriteLine(cmd.End.Position);
        }

        protected override void PreGeneration(string templatePath, string text)
        {
            this.TextRenderer.Write($"// {templatePath}", new TextFilePosition());
            this.TextRenderer.WriteLine(new TextFilePosition());

            foreach (string targetCodeUsing in Constants.TargetCodeUsings)
            {
                this.TextRenderer.Write(targetCodeUsing, new TextFilePosition());
                this.TextRenderer.WriteLine(new TextFilePosition());
            }
            var escapedTemplatePath = templatePath.Escape();
            this.TextRenderer.Write($"#line 1 \"{escapedTemplatePath}\"", new TextFilePosition());
            this.TextRenderer.WriteLine(new TextFilePosition());

            this.TextRenderer.ResetPosition();
        }

        private string SourceExpression(TextFilePosition position)
        {
            var escapedSourceName = position.Name.Escape();
            return $"new _Source(\"{escapedSourceName}\", {position.Line}, {position.Column})";
        }

        private string FeatureExpression(EntryFeatures features)
        {
            return $"_Features.{features}";
        }
    }
}