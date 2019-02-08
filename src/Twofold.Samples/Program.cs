namespace HicknHack.Twofold.Samples
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using Properties;
    using HicknHack.Twofold;
    using HicknHack.Twofold.Abstractions;
    using HicknHack.Twofold.Abstractions.SourceMapping;

    public interface ILogger
    {
        void Log(string text);
    }

    public class Logger : ILogger
    {
        public void Log(string text)
        {
            Console.WriteLine(text);
        }
    }

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

    internal class Program : ITemplateLoader, IMessageHandler
    {
        private Engine engine;

        private Program()
        {
            engine = new Engine(this, this, Assembly.GetExecutingAssembly().Location);
        }

        private static void Main(string[] args)
        {
            var app = new Program();

            // Example 1
            if(false)
            {
                var classDescriptor = new ClassDescriptor("TwofoldGenerated", new List<MethodDescriptor>
                {
                    new MethodDescriptor("void", "hello", new List<ArgumentDescriptor>
                    {
                        new ArgumentDescriptor("string", "greeted")
                    }, "Console.WriteLine(\"Hello \" + greeted);"),
                });
                var logger = new Logger();
                app.CompileAndRun("Main", classDescriptor);
            }

            // Example 2
            if(true)
            {
                app.CompileAndRun("Recursion", "Hello, world!", 5);
            }

            Console.WriteLine("Ready");
            Console.ReadKey();
        }

        private void CompileAndRun(string template, params object[] arguments)
        {
            CompiledTemplate compiledTemplate = engine.Compile(template);

            if (Directory.Exists("Generated"))
            {
                Directory.Delete("Generated", true);
            }
            Directory.CreateDirectory("Generated");

            string executablePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            if (compiledTemplate.IsValid)
            {
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
                    {
                        string combinedFilePath = Path.Combine(executablePath, "Generated", filename);
                        generatedCode.SourceMap.Write(fileStream, combinedFilePath);
                    }
                }

                Target target = engine.Run(compiledTemplate, arguments);
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
                    {
                        string combinedFilePath = Path.Combine(executablePath, "Generated", "Output.cs");
                        target.SourceMap.Write(fileStream, combinedFilePath);
                    }

                    var pos = new TextPosition(7, 34);
                    Debug.WriteLine($"Callstack for {pos}:");
                    foreach (var e in target.SourceMap.CallerStack(pos))
                    {
                        Debug.WriteLine($"{e.Name} ({e.Line}, {e.Column})");
                    }
                }
            }
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

        #endregion ITemplateLoader

        #region IMessageHandler

        public void Message(TraceLevel level, string text, string source, TextPosition position)
        {
            if (string.IsNullOrEmpty(source))
            {
                Console.Error.WriteLine($"Twofold: {level.ToString()}: {text}");
                Trace.WriteLine($"Twofold: {level.ToString()}: {text}");
                return;
            }

            string positionText = "";
            if (position.IsValid)
            {
                positionText = position.ToString();
            }
            Console.Error.WriteLine($"{source}{positionText}: {level.ToString()}: {text}");
            Trace.WriteLine($"{source}{positionText}: {level.ToString()}: {text}");
        }

        public void StackTrace(List<HicknHack.Twofold.Abstractions.StackFrame> frames)
        {
            var sb = new StringBuilder();
            foreach (var frame in frames)
            {
                sb.Clear();
                sb.Append(' ', 4).Append("at ").Append(frame.Method ?? string.Empty);
                if (string.IsNullOrEmpty(frame.File) == false)
                {
                    sb.Append(" in ").Append(frame.File);
                    if (frame.Line.HasValue)
                    {
                        sb.Append(" (").Append(frame.Line);
                        if (frame.Column.HasValue)
                        {
                            sb.Append(",").Append(frame.Column);
                        }

                        sb.Append(")");
                    }
                }

                var line = sb.ToString();
                Console.Error.WriteLine(line);
                Trace.WriteLine(line);
            }
        }

        #endregion IMessageHandler
    }
}