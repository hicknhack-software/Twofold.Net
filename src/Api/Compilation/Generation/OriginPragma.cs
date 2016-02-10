namespace Twofold.Api.Compilation.Generation
{
    public class OriginPragma : AsbtractCodeFragment
    {
        public readonly string Name;
        public readonly string Argument;

        public OriginPragma(string name, string argument, FileLine line, TextSpan span)
            : base(CodeFragmentTypes.OriginPragma, line, span)
        {
            Name = name;
            Argument = argument;
        }
    }
}
