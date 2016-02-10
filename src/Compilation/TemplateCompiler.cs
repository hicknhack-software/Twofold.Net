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
                {'#', new PreprocessorRule() },
            };
            this.templateParser = new TemplateParser(parserRules, new PassThroughRule(), this.messageHandler);
        }

        public Template Compile(string templateName)
        {
            var templateNames = new Queue<string>();
            templateNames.Enqueue(templateName);

            var generatedTwofoldSources = new List<string>();

            string mainTemplateFilename = null;
            bool mainTemplateFilenameSet = false;

            while (templateNames.Count > 0) {
                // Load template
                TextLoaderResult textLoaderResult = textLoader.Load(templateName);
                if (mainTemplateFilenameSet == false) {
                    mainTemplateFilename = textLoaderResult.Name;
                    mainTemplateFilenameSet = true;
                }

                // Generate Twofold enhanced CSharp code from template
                List<string> includedFiles;
                string twofoldCSharpCode = this.GenerateCode(textLoaderResult.Name, textLoaderResult.Text, out includedFiles);
                generatedTwofoldSources.Add(twofoldCSharpCode);
            }

            // Compile CSharp code
            Assembly twofoldAssembly = this.CompileCode(generatedTwofoldSources);

            // Check Twofold CSharp assembly for entry points/types
            bool testResult = this.CheckAssembly(mainTemplateFilename, twofoldAssembly);
            if (testResult == false) {
                return null;
            }

            return new Template(mainTemplateFilename, twofoldAssembly);
        }

        string GenerateCode(string sourceName, string text, out List<string> includedFiles)
        {
            TextWriter codeWriter = new StringWriter();
            includedFiles = new List<string>();

            var csharpGenerator = new CSharpGenerator(templateParser, codeWriter, includedFiles);
            csharpGenerator.Generate(sourceName, text);

            return codeWriter.ToString();
        }

        Assembly CompileCode(List<string> sources)
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
            CompilerResults compilerResults = codeProvider.CompileAssemblyFromSource(parameters, sources.ToArray());
            if (compilerResults.Errors.Count > 0) {
                foreach (CompilerError compilerError in compilerResults.Errors) {
                    TraceLevel traceLevel = compilerError.IsWarning ? TraceLevel.Warning : TraceLevel.Error;
                    var errorPosition = new TextFilePosition(compilerError.FileName, new TextPosition(compilerError.Line, compilerError.Column));
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
