using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Reflection;
using Twofold.Api;
using Twofold.Parsing;

namespace Twofold
{
    public class TemplateCompiler
    {
        readonly ITextLoader textLoader;
        readonly IMessageHandler messageHandler;
        readonly StringCollection referencedAssemblies;
        readonly TemplateParser templateParser;

        public TemplateCompiler(ITextLoader textLoader, IMessageHandler messageHandler, StringCollection referencedAssemblies)
        {
            this.textLoader = textLoader;
            this.messageHandler = messageHandler;
            this.referencedAssemblies = referencedAssemblies;

            this.templateParser = new TemplateParser(new Dictionary<char, IParserRule>
            {
                {'\\', new InterpolationRule(messageHandler) },
                {'|', new InterpolateLineRule(messageHandler) },
                {'=', new CallRule(messageHandler) },
                {'#', new CommandRule(messageHandler) },
            }, new PassThroughRule(messageHandler));
        }

        public Template Compile(string templateName)
        {
            // Load template and convert to CSharp
            TextLoaderResult textLoaderResult = textLoader.Load(templateName);
            var csharpGenerator = new TwofoldCSharpGenerator();
            templateParser.Parse(textLoaderResult.Name, textLoaderResult.Text, csharpGenerator);
            string generatedCode = csharpGenerator.GeneratedCode();

            // Prepare compiler
            var parameters = new CompilerParameters();
            parameters.GenerateInMemory = true;
            parameters.MainClass = "Twofold.Entry";
            parameters.ReferencedAssemblies.Add("System.dll");
            parameters.ReferencedAssemblies.Add("System.Core.dll");
            foreach (string referencedAssembly in referencedAssemblies) {
                parameters.ReferencedAssemblies.Add(referencedAssembly);
            }

            // Compile
            var codeProvider = new CSharpCodeProvider();
            CompilerResults compilerResults = codeProvider.CompileAssemblyFromSource(parameters, generatedCode);
            if (compilerResults.Errors.Count > 0) {
                foreach (CompilerError compilerError in compilerResults.Errors) {
                    TraceLevel traceLevel = compilerError.IsWarning ? TraceLevel.Warning : TraceLevel.Error;
                    messageHandler.CSharpMessage(traceLevel, compilerError.ToString());
                }

                return new Template();
            }

            // Check for entry type and main method
            Assembly compiledAssembly = compilerResults.CompiledAssembly;
            var entryType = compiledAssembly.GetType("Twofold.Entry");
            if (entryType == null) {
                messageHandler.CSharpMessage(TraceLevel.Error, "Can't find Twofold entry class 'Twofold.Entry'");
                return new Template();
            }

            var mainMethod = entryType.GetMethod("Main", BindingFlags.Public | BindingFlags.Static);
            if (mainMethod == null) {
                messageHandler.CSharpMessage(TraceLevel.Error, "Can't find Twofold entry method 'Main'");
                return new Template();
            }

            var template = new Template(compiledAssembly);
            return template;
        }
    }
}
