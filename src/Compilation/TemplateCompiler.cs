using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Twofold.Api;
using Twofold.Api.Compilation.Parsing;
using Twofold.Api.SourceMapping;
using Twofold.Compilation.Generation;
using Twofold.Compilation.Parsing;

namespace Twofold.Compilation
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

            var parserRules = new Dictionary<char, IParserRule>
            {
                {'\\', new InterpolationRule() },
                {'|', new InterpolateLineRule() },
                {'=', new CallRule() },
            };
            this.templateParser = new TemplateParser(parserRules, new PassThroughRule(), this.messageHandler);
        }

        public Template Compile(string templateName)
        {
            // Load template and convert to CSharp
            TextLoaderResult textLoaderResult = textLoader.Load(templateName);

            // Generate Twofold enhanced CSharp code
            string twofoldCSharpCode = this.GenerateCode(textLoaderResult.Name, textLoaderResult.Text);

            // Compile CSharp code
            Assembly twofoldAssembly = this.CompileCode(textLoaderResult.Name, twofoldCSharpCode);

            // Check Twofold CSharp assembly for entry points/types
            bool testResult = this.CheckAssembly(textLoaderResult.Name, twofoldAssembly);
            if (testResult == false) {
                return null;
            }

            return new Template(twofoldAssembly);
        }

        string GenerateCode(string name, string text)
        {
            TextWriter codeWriter = new StringWriter();
            var csharpGenerator = new CSharpGenerator(templateParser, codeWriter);
            csharpGenerator.Generate(name, text);
            return codeWriter.ToString();
        }

        Assembly CompileCode(string name, string text)
        {
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
            CompilerResults compilerResults = codeProvider.CompileAssemblyFromSource(parameters, text);
            if (compilerResults.Errors.Count > 0) {
                foreach (CompilerError compilerError in compilerResults.Errors) {
                    TraceLevel traceLevel = compilerError.IsWarning ? TraceLevel.Warning : TraceLevel.Error;
                    var errorPosition = new TextFilePosition(name, new TextPosition(compilerError.Line, compilerError.Column));
                    messageHandler.CSharpMessage(traceLevel, errorPosition, compilerError.ToString());
                }
            }

            return compilerResults.CompiledAssembly;
        }

        bool CheckAssembly(string name, Assembly assembly)
        {
            // Check for entry type and main method
            var entryType = assembly.GetType("Twofold.Entry");
            if (entryType == null) {
                messageHandler.Message(TraceLevel.Error, $"Can't find Twofold entry class 'Twofold.Entry' in template '{name}'.");
                return false;
            }

            var mainMethod = entryType.GetMethod("Main", BindingFlags.Public | BindingFlags.Static);
            if (mainMethod == null) {
                messageHandler.Message(TraceLevel.Error, $"Can't find Twofold entry method 'Main' in template '{name}'.");
                return false;
            }

            return true;
        }
    }
}
