using System;
using Twofold.Api;

namespace Twofold.Parsing.Rules
{
    public class CallRule : IParserRule
    {
        public void Parse(FileLine line, ICodeGenerator codeGenerator, IMessageHandler messageHandler)
        {
            //var beginIndex = line.BeginIndexNonSpace + 1; //skip matched character
            //var end = line.Text.IndexOfNot(beginIndex, line.EndIndex, CharExtensions.IsSpace);
            //if (end == -1) {
            //    return;
            //}
            throw new NotImplementedException();
        }
    }
}
