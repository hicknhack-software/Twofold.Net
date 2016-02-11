using Example.Properties;
using System;
using System.Diagnostics;
using System.IO;
using Twofold;
using Twofold.Api;
using Twofold.Api.SourceMapping;

namespace Example
{
    class Program : ITemplateLoader, IMessageHandler
    {
        static void Main(string[] args)
        {
            Program app = new Program();
            Engine engine = new Engine(app, app);

            CompiledTemplate compiledTemplate = engine.Compile("ExampleMain");
            Target target = engine.Run(compiledTemplate, "HicknHack Software GmbH");

            Console.ReadKey();
        }

        public Template Load(string name)
        {
            var text = Resources.ResourceManager.GetString(name);
            if (text == null) {
                throw new FileNotFoundException("", name);
            }

            Template template = new Template(name, text);
            return template;
        }

        #region IMessageHandler
        public void CSharpMessage(TraceLevel level, TextFilePosition position, string text)
        {
            string positionText = "";
            if (position.IsValid) {
                positionText = $"({ position.Line},{ position.Column})";
            }

            Trace.WriteLine($"{position.SourceName}{positionText}: {level.ToString()}: {text}");
        }

        public void Message(TraceLevel level, string text)
        {
            Trace.WriteLine($"Twofold: {level.ToString()}: {text}");
        }

        public void TemplateMessage(TraceLevel level, TextFilePosition position, string text)
        {
            string positionText = "";
            if (position.IsValid) {
                positionText = $"({ position.Line},{ position.Column})";
            }

            Trace.WriteLine($"{position.SourceName}{positionText}: {level.ToString()}: {text}");
        }
        #endregion
    }
}
