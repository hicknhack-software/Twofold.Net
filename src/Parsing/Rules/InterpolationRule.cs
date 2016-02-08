using System;
using Twofold.Api;

namespace Twofold.Parsing.Rules
{
    class TextSpan
    {
        public TextSpan(int begin, int end)
        { }
    }

    class IndentTargetPart
    {
        public IndentTargetPart(FileLine line, TextSpan span)
        { }
    }

    class OriginTarget
    {
        public OriginTarget(FileLine line, TextSpan span)
        { }
    }

    public class InterpolationRule : IParserRule
    {
        public void Parse(FileLine line, ICodeGenerator codeGenerator, IMessageHandler messageHandler)
        {
            //var beginIndexIndent = line.BeginIndexNonSpace + 1; //skip matched character
            //var beginIndex = line.Text.IndexOfNot(beginIndexIndent, line.EndIndex, CharExtensions.IsSpace);
            //if (beginIndex == -1) {
            //    beginIndex = line.EndIndex;
            //}

            //codeGenerator.Append(new IndentTargetPart(line, new TextSpan(beginIndexIndent, beginIndex)));
            //var endIndex = beginIndex;
            //while (beginIndex != line.EndIndex) {
            //    endIndex = line.Text.IndexOf(beginIndex, line.EndIndex, (ch) => ch == '#');
            //    if (endIndex == -1) { // reached line end
            //        endIndex = line.EndIndex;
            //        break;
            //    }

            //    var expressionBeginIndex = endIndex + 1; //skip #
            //    if (expressionBeginIndex == line.EndIndex) { // reached line end
            //        break;
            //    }
            //    switch (line.Text[expressionBeginIndex]) {
            //        // Escaped # as "##"
            //        //      |    ##{blaa}
            //        //begin      ^
            //        //end        ^
            //        //exprBegin   ^
            //        case '#':
            //            codeGenerator.Append(new OriginTarget(line, new TextSpan(beginIndex, expressionBeginIndex)));
            //            beginIndex = endIndex = expressionBeginIndex + 1;
            //            continue;

            //        case '{':
            //            codeGenerator.Append(new OriginTarget(line, new TextSpan(beginIndex, endIndex)));
            //            continue;

            //        // Move on
            //        default:
            //            endIndex = expressionBeginIndex + 1;
            //            continue;
            //    }
            //}
            //codeGenerator.Append(new OriginTarget(line, new TextSpan(beginIndex, line.EndIndex)));
            throw new NotImplementedException();
        }
    }
}
