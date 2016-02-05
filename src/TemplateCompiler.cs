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
        readonly CSharpGenerator csharpGenerator;

        public TemplateCompiler(ITextLoader textLoader, IMessageHandler messageHandler, StringCollection referencedAssemblies)
        {
            this.textLoader = textLoader;
            this.messageHandler = messageHandler;
            this.referencedAssemblies = referencedAssemblies;
            this.csharpGenerator = new CSharpGenerator();
            this.templateParser = new TemplateParser(new Dictionary<char, IParserRule>
            {
                {'\\', new InterpolationRule(messageHandler, csharpGenerator) },
                {'|', new InterpolateLineRule(messageHandler, csharpGenerator) },
                {'=', new CallRule(messageHandler, csharpGenerator) },
                {'#', new CommandRule(messageHandler, csharpGenerator) },
            }, new PassThroughRule(messageHandler, csharpGenerator));
        }

        public Template Compile(string templateName)
        {
            // Load text
            TextLoaderResult textLoaderResult = textLoader.Load(templateName);

            // Line processing

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
            CompilerResults compilerResults = codeProvider.CompileAssemblyFromSource(parameters, textLoaderResult.Text);
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
