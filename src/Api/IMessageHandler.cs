using System.Diagnostics;

namespace Twofold.Api
{
    public interface IMessageHandler
    {
        void Message(TraceLevel level, string text);
        void TemplateMessage(TraceLevel level, string text);
        void CSharpMessage(TraceLevel level, string text);
    };
}
