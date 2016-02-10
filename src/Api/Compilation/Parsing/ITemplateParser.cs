using System.Collections.Generic;
using Twofold.Api.Compilation.Generation;

namespace Twofold.Api.Compilation.Parsing
{
    public interface ITemplateParser
    {
        List<AsbtractCodeFragment> Parse(string sourceName, string text);
    }
}
