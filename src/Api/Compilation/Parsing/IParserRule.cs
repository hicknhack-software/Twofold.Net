using System.Collections.Generic;
using Twofold.Api.Compilation.Generation;

namespace Twofold.Api.Compilation.Parsing
{
    public interface IParserRule
    {
        List<AsbtractCodeFragment> Parse(FileLine line, IMessageHandler messageHandler);
    }
}
