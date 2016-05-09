using Example.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Twofold;
using Twofold.Interface;
using Twofold.Interface.SourceMapping;

namespace Example
{
    public class ArgumentDescriptor
    {
        public readonly string Type;
        public readonly string Name;

        public ArgumentDescriptor(string type, string name)
        {
            Type = type;
            Name = name;
        }
    }

    public class MethodDescriptor
    {
        public readonly string Name;
        public readonly List<ArgumentDescriptor> Arguments;
        public readonly string ReturnType;
        public readonly string Body;

        public MethodDescriptor(string returnType, string name, List<ArgumentDescriptor> arguments, string body)
        {
            ReturnType = returnType;
            Name = name;
            Arguments = arguments;
            Body = body;
        }
    }

    public class ClassDescriptor
    {
        public string s;
        public readonly string Name;
        public readonly List<MethodDescriptor> Methods;

        public ClassDescriptor(string name, List<MethodDescriptor> methods)
        {
            Name = name;
            Methods = methods;
        }
    }

    class Program : ITemplateLoader, IMessageHandler
    {
        static void Main(string[] args)
        {
            Program app = new Program();
            Engine engine = new Engine(app, app, Assembly.GetExecutingAssembly().Location);

            CompiledTemplate compiledTemplate = engine.Compile("Main");

            if (Directory.Exists("Generated"))
            {
                Directory.Delete("Generated", true);
            }
            Directory.CreateDirectory("Generated");

            foreach (var generatedCode in compiledTemplate.GeneratedCodes)
            {
                string filename = generatedCode.TemplatePath;
                int i = filename.LastIndexOf("\\", StringComparison.Ordinal);
                if (i > 0)
                {
                    filename = filename.Substring(i + 1);
                }

                using (var fileStream = new FileStream($"Generated\\{filename}", FileMode.CreateNew))
                using (var writer = new StreamWriter(fileStream))
                {
                    writer.Write(generatedCode.Code);
                }

                using (var fileStream = new FileStream($"Generated\\{filename}.map", FileMode.CreateNew))
                using (var writer = new StreamWriter(fileStream))
                {
                    writer.Write(generatedCode.SourceMap);
                }
            }

            if (compiledTemplate.IsValid)
            {
                var classDescriptor = new ClassDescriptor("TwofoldGenerated", new List<MethodDescriptor>
                {
                    new MethodDescriptor("void", "hello", new List<ArgumentDescriptor>
                    {
                        new ArgumentDescriptor("string", "greeted")
                    }, "Console.WriteLine(\"Hello \" + greeted);"),
                });


                Target target = engine.Run(compiledTemplate, classDescriptor);
                File.Delete("Generated\\Output.cs");
                File.Delete("Generated\\Output.cs.map");
                if (target != null)
                {
                    using (var fileStream = new FileStream("Generated\\Output.cs", FileMode.CreateNew))
                    using (var writer = new StreamWriter(fileStream))
                    {
                        writer.Write(target.GeneratedText);
                    }

                    using (var fileStream = new FileStream("Generated\\Output.cs.map", FileMode.CreateNew))
                    using (var writer = new StreamWriter(fileStream))
                    {
                        writer.Write(target.SourceMap);
                    }
                }

                var pos = new TextPosition(7, 34);
                Debug.WriteLine($"Callstack for {pos}:");
                foreach (var e in target.SourceMap.CallerStack(pos))
                {
                    Debug.WriteLine($"{e.SourceName} ({e.Line}, {e.Column})");
                }
            }

            Console.ReadKey();
        }

        #region ITemplateLoader
        public Template Load(string name)
        {
            var text = Resources.ResourceManager.GetString(name);
            if (text == null)
            {
                throw new FileNotFoundException("", name);
            }

            Template template = new Template($"C:\\Projects\\Sourcecode\\{name}.cs", text);
            return template;
        }
        #endregion

        #region IMessageHandler
        public void Message(TraceLevel level, string text, string source, TextPosition position)
        {
            if (string.IsNullOrEmpty(source))
            {
                Console.WriteLine($"Twofold: {level.ToString()}: {text}");
                Trace.WriteLine($"Twofold: {level.ToString()}: {text}");
                return;
            }

            string positionText = "";
            if (position.IsValid)
            {
                positionText = position.ToString();
            }
            Console.WriteLine($"{source}{positionText}: {level.ToString()}: {text}");
            Trace.WriteLine($"{source}{positionText}: {level.ToString()}: {text}");
        }
        #endregion
    }
}
