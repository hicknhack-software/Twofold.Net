using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Twofold
{
    public interface IMessageHandler
    {
        void Message(TraceLevel level, string text);
        void TemplateMessage(TraceLevel level, string text);
        void CSharpMessage(TraceLevel level, string text);
    };

    public struct TextLoaderResult
    {
        public string Name { get; private set; }
        public string Text { get; private set; }

        public TextLoaderResult(string name, string text)
        {
            Name = name;
            Text = text;
        }
    };

    public interface ITextLoader
    {
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="IOException"></exception>
        TextLoaderResult Load(string name);
    }

    public class Template
    {
        public bool IsValid
        {
            get { return Assembly != null; }
        }
        public Assembly @Assembly { get; private set; }

        public Template() { }

        public Template(Assembly assembly)
        {
            Assembly = assembly;
        }
    }

    public class TemplateCompiler
    {
        ITextLoader textLoader;
        IMessageHandler messageHandler;
        StringCollection referencedAssemblies;

        public TemplateCompiler(ITextLoader textLoader, IMessageHandler messageHandler, StringCollection referencedAssemblies)
        {
            this.textLoader = textLoader;
            this.messageHandler = messageHandler;
            this.referencedAssemblies = referencedAssemblies;
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

    public class Target
    { }

    public class Engine
    {
        ITextLoader templateLoader;
        IMessageHandler messageHandler;
        StringCollection referencedAssemblies;

        public Engine(ITextLoader templateLoader, IMessageHandler messageHandler)
            : this(templateLoader, messageHandler, new StringCollection())
        { }

        public Engine(ITextLoader templateLoader, IMessageHandler messageHandler, StringCollection referencedAssemblies)
        {
            this.templateLoader = templateLoader;
            this.messageHandler = messageHandler;
            this.referencedAssemblies = referencedAssemblies;
        }

        public Template Compile(string templateName)
        {
            Template template = new Template();
            try {
                var templateCompiler = this.CreateTemplateCompiler();
                template = templateCompiler.Compile(templateName);
            }
            catch (FileNotFoundException) {
                messageHandler.Message(TraceLevel.Error, $"Template '{templateName}' not found");
            }
            catch (IOException) {
                messageHandler.Message(TraceLevel.Error, $"IO error while reading template '{templateName}'");
            }
            catch (Exception ex) {
                messageHandler.Message(TraceLevel.Error, ex.ToString());
            }
            return template;
        }

        public Target Run<T>(Template template, T input, params Assembly[] assemblies) where T : class
        {
            return new Target();
        }

        TemplateCompiler CreateTemplateCompiler(params Assembly[] assemblies)
        {
            var templateCompiler = new TemplateCompiler(templateLoader, messageHandler, referencedAssemblies);
            return templateCompiler;
        }
    }
}
