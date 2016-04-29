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

            TemplateCompilerResult compilerResult = engine.Compile("Main");

            File.Delete("TwofoldRenderCode.cs");
            using (var fileStream = new FileStream("TwofoldRenderCode.cs", FileMode.CreateNew))
            using (var writer = new StreamWriter(fileStream))
                foreach (var nameSourceTuple in compilerResult.TargetCodes)
                {
                    writer.WriteLine($"//{nameSourceTuple.Item1}");
                    writer.WriteLine("//////////////////////////////////////////////////////");
                    writer.WriteLine($"{nameSourceTuple.Item2}");
                    writer.WriteLine();
                }

            if (compilerResult.CompiledTemplate != null)
            {

                var classDescriptor = new ClassDescriptor("TwofoldGenerated", new List<MethodDescriptor>
                {
                    new MethodDescriptor("void", "hello", new List<ArgumentDescriptor>
                    {
                        new ArgumentDescriptor("string", "greeted")
                    }, "Console.WriteLine(\"Hello \" + greeted);"),
                });


                Target target = engine.Run(compilerResult.CompiledTemplate, classDescriptor);
                Console.WriteLine("Generated code...");
                Console.WriteLine("-------");
                if (target != null)
                {
                    Console.WriteLine(target.GeneratedText);
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

            Template template = new Template(name, text);
            return template;
        }
        #endregion

        #region IMessageHandler
        public void Message(TraceLevel level, string text, string source, TextPosition position)
        {
            if (string.IsNullOrEmpty(source))
            {
                Trace.WriteLine($"Twofold: {level.ToString()}: {text}");
                return;
            }

            string positionText = "";
            if (position.IsValid)
            {
                positionText = position.ToString();
            }
            Trace.WriteLine($"{source}{positionText}: {level.ToString()}: {text}");
        }
        #endregion
    }
}
