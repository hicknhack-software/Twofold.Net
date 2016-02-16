using System.Diagnostics;
using Twofold.Interface;
using Twofold.Interface.SourceMapping;

namespace UnitTests.Helper
{
    class NullMessageHandler : IMessageHandler
    {
        public void CSharpMessage(TraceLevel level, TextFilePosition position, string text)
        {
        }

        public void Message(TraceLevel level, string text)
        {
        }

        public void TemplateMessage(TraceLevel level, TextFilePosition position, string text)
        {
        }
    }
}
