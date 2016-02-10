using System.Collections.Generic;
using Twofold.Api.Compilation.Generation;

namespace Twofold.Api.Compilation.Parsing
{
    public interface ITemplateParser
    {
        List<AsbtractCodeFragment> Parse(string name, string text);
    }
}
