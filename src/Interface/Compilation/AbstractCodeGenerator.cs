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

namespace Twofold.Interface.Compilation
{
    using System;
    using System.Collections.Generic;

    public abstract class AbstractCodeGenerator
    {
        private readonly ITemplateParser Parser;

        public AbstractCodeGenerator(ITemplateParser parser)
        {
            this.Parser = parser;
        }

        public void Generate(string templatePath, string text)
        {
            this.PreGeneration(templatePath, text);

            List<AsbtractRenderCommand> commands = this.Parser.Parse(templatePath, text);
            foreach (var command in commands)
            {
                switch (command.Type)
                {
                    case RenderCommands.Expression:
                        this.Generate((ExpressionCommand)command);
                        break;

                    case RenderCommands.Pragma:
                        this.Generate((PragmaCommand)command);
                        break;

                    case RenderCommands.Script:
                        this.Generate((ScriptCommand)command);
                        break;

                    case RenderCommands.Text:
                        this.Generate((TextCommand)command);
                        break;

                    case RenderCommands.LocalIndentation:
                        this.Generate((LocalIndentationCommand)command);
                        break;

                    case RenderCommands.NewLine:
                        this.Generate((NewLineCommand)command);
                        break;

                    case RenderCommands.PopIndentation:
                        this.Generate((PopIndentationCommand)command);
                        break;

                    case RenderCommands.PushIndentation:
                        this.Generate((PushIndentationCommand)command);
                        break;

                    case RenderCommands.Statement:
                        this.Generate((StatementCommand)command);
                        break;

                    default:
                        throw new NotSupportedException($"CodeFragmentType '{command.Type.ToString()}' is not supported.");
                }
            }

            this.PostGeneration(templatePath, text);
        }

        protected virtual void PreGeneration(string templatePath, string text)
        {
        }

        protected virtual void PostGeneration(string templatePath, string text)
        {
        }

        protected abstract void Generate(TextCommand command);

        protected abstract void Generate(ExpressionCommand command);

        protected abstract void Generate(PragmaCommand command);

        protected abstract void Generate(ScriptCommand command);

        protected abstract void Generate(NewLineCommand command);

        protected abstract void Generate(LocalIndentationCommand command);

        protected abstract void Generate(PushIndentationCommand command);

        protected abstract void Generate(PopIndentationCommand command);

        protected abstract void Generate(StatementCommand command);
    }
}