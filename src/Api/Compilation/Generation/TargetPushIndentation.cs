namespace Twofold.Api.Compilation.Generation
{
    public class TargetPushIndentation : AsbtractCodeFragment
    {
        public TargetPushIndentation(FileLine line, TextSpan span)
            : base(CodeFragmentTypes.TargetPushIndentation, line, span)
        { }
    }
}
