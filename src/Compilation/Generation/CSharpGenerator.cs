using System.IO;
using Twofold.Api.Compilation.Generation;
using Twofold.Compilation.Parsing;

namespace Twofold.Compilation.Generation
{
    public class CSharpGenerator : AbstractCodeGenerator
    {
        readonly TextWriterController textWriterController;
        readonly TextWriter textWriter;

        public CSharpGenerator(TemplateParser templateParser, TextWriter textWriter)
            : base(templateParser)
        {
            this.textWriterController = new TextWriterController();
            this.textWriter = textWriter;
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
    }
}
