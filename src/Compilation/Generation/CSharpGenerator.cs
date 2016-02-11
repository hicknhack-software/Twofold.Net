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
using System.Collections.Generic;
using System.IO;
using Twofold.Api;
using Twofold.Api.Compilation.Generation;
using Twofold.Compilation.Parsing;

namespace Twofold.Compilation.Generation
{
    public class CSharpGenerator : AbstractCodeGenerator
    {
        readonly TextWriterController textWriterController;
        readonly TextWriter textWriter;
        readonly List<string> includedFiles;

        public CSharpGenerator(TemplateParser templateParser, TextWriter textWriter, List<string> includedFiles)
            : base(templateParser)
        {
            this.textWriterController = new TextWriterController();
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
            textWriterController.Append(fragment.Span, textWriter);
            textWriterController.AppendNewLine(textWriter);
        }

        protected override void Generate(OriginScript fragment)
        {
            textWriterController.Append(fragment.Span, textWriter);
            textWriterController.AppendNewLine(textWriter);
        }

        protected override void Generate(OriginExpression fragment)
        {
            textWriterController.Append(fragment.Span, textWriter);
            textWriterController.AppendNewLine(textWriter);
        }

        protected override void Generate(OriginText fragment)
        {
            textWriterController.Append(fragment.Span, textWriter);
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
        }

        protected override void PreGeneration(string sourceName, string text)
        {
            var lineDirective = $"#line 1 \"{sourceName}\"";
            textWriterController.Append(new TextSpan(lineDirective), textWriter);
            textWriterController.AppendNewLine(textWriter);
        }
    }
}
