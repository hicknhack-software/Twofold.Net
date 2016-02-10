namespace Twofold.Api.Compilation.Generation
{
    public class OriginText : AsbtractCodeFragment
    {
        public OriginText(FileLine line, TextSpan span)
            : base(CodeFragmentTypes.OriginText, line, span)
        { }
    }
}
