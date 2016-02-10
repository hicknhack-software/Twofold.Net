namespace Twofold.Api.Compilation.Generation
{
    public class TargetNewLine : AsbtractCodeFragment
    {
        public TargetNewLine(FileLine line, TextSpan span)
            : base(CodeFragmentTypes.TargetNewLine, line, span)
        { }
    }
}
