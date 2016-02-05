using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Twofold.Api
{
    public interface IParserRule
    {
        void Parse(string value, int beginIndex, int nonSpaceBeginIndex, int endIndex);
    }
}
