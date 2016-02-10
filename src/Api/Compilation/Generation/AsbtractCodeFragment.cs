namespace Twofold.Api.Compilation.Generation
{
    public abstract class AsbtractCodeFragment
    {
        public readonly CodeFragmentTypes Type;
        public readonly FileLine Line;
        public readonly TextSpan Span;

        public AsbtractCodeFragment(CodeFragmentTypes type, FileLine line, TextSpan span)
        {
            Type = type;
            Line = line;
            Span = span;
        }
    }
}
