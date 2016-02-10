namespace Twofold.Api.Compilation.Generation
{
    public class OriginScript : AsbtractCodeFragment
    {
        public OriginScript(FileLine line, TextSpan span)
            : base(CodeFragmentTypes.OriginScript, line, span)
        { }
    }
}
