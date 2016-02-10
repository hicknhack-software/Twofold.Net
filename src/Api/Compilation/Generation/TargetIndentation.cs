namespace Twofold.Api.Compilation.Generation
{
    public class TargetIndentation : AsbtractCodeFragment
    {
        public TargetIndentation(FileLine line, TextSpan span)
            : base(CodeFragmentTypes.TargetIndentation, line, span)
        { }
    }
}
