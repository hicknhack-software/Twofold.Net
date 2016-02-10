using System.Diagnostics;
using Twofold.Api.SourceMapping;

namespace Twofold.Api
{
    public interface IMessageHandler
    {
        void Message(TraceLevel level, string text);
        void TemplateMessage(TraceLevel level, TextFilePosition position, string text);
        void CSharpMessage(TraceLevel level, TextFilePosition position, string text);
    };
}
