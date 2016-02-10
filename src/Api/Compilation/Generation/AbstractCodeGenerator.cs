using System;
using System.Collections.Generic;
using Twofold.Api.Compilation.Parsing;

namespace Twofold.Api.Compilation.Generation
{
    public abstract class AbstractCodeGenerator
    {
        readonly ITemplateParser parser;

        public AbstractCodeGenerator(ITemplateParser parser)
        {
            this.parser = parser;
        }

        public void Generate(string sourceName, string text)
        {
            this.PreGeneration(sourceName, text);

            List<AsbtractCodeFragment> fragments = parser.Parse(sourceName, text);
            foreach (var codeFragment in fragments) {
                switch (codeFragment.Type) {
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
                        throw new NotSupportedException($"CodeFragmentType '{codeFragment.Type.ToString()} is not supported.'");
                }
            }

            this.PostGeneration(sourceName, text);
        }

        protected abstract void PreGeneration(string sourceName, string text);
        protected abstract void PostGeneration(string sourceName, string text);

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
