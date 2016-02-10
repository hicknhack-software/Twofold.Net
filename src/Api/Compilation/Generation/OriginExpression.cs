namespace Twofold.Api.Compilation.Generation
{
    public class OriginExpression : AsbtractCodeFragment
    {
        public OriginExpression(FileLine line, TextSpan span)
            : base(CodeFragmentTypes.OriginExpression, line, span)
        { }
    }
}
