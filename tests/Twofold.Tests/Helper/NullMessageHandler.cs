namespace HicknHack.Twofold.Tests.Helper
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using HicknHack.Twofold.Abstractions;
    using HicknHack.Twofold.Abstractions.SourceMapping;


    public class NullMessageHandler : IMessageHandler
    {
        public void Message(TraceLevel level, string text, string source, TextPosition position)
        {
        }

        public void StackTrace(List<HicknHack.Twofold.Abstractions.StackFrame> frames)
        {
        }
    }
}
