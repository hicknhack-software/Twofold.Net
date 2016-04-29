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
        private readonly ITemplateParser parser;

        public AbstractCodeGenerator(ITemplateParser parser)
        {
            this.parser = parser;
        }

        public void Generate(string sourceName, string text)
        {
            this.PreGeneration(sourceName, text);

            List<AsbtractCodeFragment> fragments = parser.Parse(sourceName, text);
            foreach (var codeFragment in fragments)
            {
                switch (codeFragment.Type)
                {
                    case CodeFragmentTypes.OriginExpression:
                        this.Generate((OriginExpression)codeFragment);
                        break;

                    case CodeFragmentTypes.OriginPragma:
                        this.Generate((OriginPragma)codeFragment);
                        break;

                    case CodeFragmentTypes.OriginScript:
                        this.Generate((OriginScript)codeFragment);
                        break;

                    case CodeFragmentTypes.OriginText:
                        this.Generate((OriginText)codeFragment);
                        break;

                    case CodeFragmentTypes.TargetIndentation:
                        this.Generate((TargetIndentation)codeFragment);
                        break;

                    case CodeFragmentTypes.TargetNewLine:
                        this.Generate((TargetNewLine)codeFragment);
                        break;

                    case CodeFragmentTypes.TargetPopIndentation:
                        this.Generate((TargetPopIndentation)codeFragment);
                        break;

                    case CodeFragmentTypes.TargetPushIndentation:
                        this.Generate((TargetPushIndentation)codeFragment);
                        break;

                    default:
                        throw new NotSupportedException($"CodeFragmentType '{codeFragment.Type.ToString()}' is not supported.");
                }
            }

            this.PostGeneration(sourceName, text);
        }

        protected virtual void PreGeneration(string sourceName, string text)
        {
        }

        protected virtual void PostGeneration(string sourceName, string text)
        {
        }

        protected abstract void Generate(OriginText fragment);

        protected abstract void Generate(OriginExpression fragment);

        protected abstract void Generate(OriginPragma fragment);

        protected abstract void Generate(OriginScript fragment);

        protected abstract void Generate(TargetNewLine fragment);

        protected abstract void Generate(TargetIndentation fragment);

        protected abstract void Generate(TargetPushIndentation fragment);

        protected abstract void Generate(TargetPopIndentation fragment);
    }
}