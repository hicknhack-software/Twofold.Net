using System;
using System.Diagnostics;
using Twofold.Interface;
using Twofold.Interface.SourceMapping;

namespace UnitTests.Helper
{
    class NullMessageHandler : IMessageHandler
    {
        public void Message(TraceLevel level, string text, string source, TextPosition position)
        {
        }
    }
}
