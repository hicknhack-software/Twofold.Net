/* Twofold.Net
 * (C) Copyright 2016 HicknHack Software GmbH
 *
 * The original code can be found at:
 *     https://github.com/hicknhack-software/Twofold.Net
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Twofold.Interface;
using Twofold.Interface.Compilation;
using Twofold.Interface.SourceMapping;

namespace Twofold.Compilation
{
    public sealed class TemplateCompiler
    {
        readonly ITemplateLoader textLoader;
        readonly IMessageHandler messageHandler;
        readonly List<string> referencedAssemblies;
        readonly TemplateParser templateParser;

        /// <summary>
        /// Constructs a TemplateCompiler.
        /// </summary>
        /// <param name="templateLoader">ITemplateLoader to load templates by name.</param>
        /// <param name="messageHandler">IMessageHandler to report messges to the user.</param>
        /// <param name="referencedAssemblies">Additional assemblied to include in template compilation.</param>
        public TemplateCompiler(ITemplateLoader templateLoader, IMessageHandler messageHandler, List<string> referencedAssemblies)
        {
            if (templateLoader == null) {
                throw new ArgumentNullException("templateLoader");
            }
            if (messageHandler == null) {
                throw new ArgumentNullException("messageHandler");
            }

            this.textLoader = templateLoader;
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

        /// <summary>
        /// Loads the given template and compiles it.
        /// </summary>
        /// <param name="templateName">Name of template which will be resolved by ITemplateLoader.</param>
        /// <returns>Compiled template or null in case of an error.</returns>
        /// <exception cref="ArgumentException">If templateName is null or empty.</exception>
        public CompiledTemplate Compile(string templateName)
        {
            if (string.IsNullOrEmpty(templateName)) {
                throw new ArgumentException("Can't be null or empty.", "templateName");
            }

            var templateNames = new Queue<string>();
            templateNames.Enqueue(templateName);

            var generatedTwofoldSources = new List<string>();

            string mainTemplateFilename = null;
            bool mainTemplateFilenameSet = false;

            while (templateNames.Count > 0) {

                // Load template
                string loadTemplateName = templateNames.Dequeue();
                Template template = null;
                try {
                    template = textLoader.Load(loadTemplateName);
                    if (mainTemplateFilenameSet == false) {
                        mainTemplateFilename = template.SourceName;
                        mainTemplateFilenameSet = true;
                    }
                }
                catch (FileNotFoundException) {
                    messageHandler.Message(TraceLevel.Error, $"Can't find template '{templateName}'.");
                    throw;
                }
                catch (IOException) {
                    messageHandler.Message(TraceLevel.Error, $"IO error while reading template '{templateName}'.");
                    throw;
                }
                catch (Exception) {
                    continue;
                }

                // Generate Twofold enhanced CSharp code from template
                List<string> includedFiles;
                string generatedCode = this.GenerateCode(template.SourceName, template.Text, out includedFiles);
                if (generatedCode != null) {
                    generatedTwofoldSources.Add(generatedCode);
                    foreach (var includedFile in includedFiles) {
                        templateNames.Enqueue(includedFile);
                    }
                }
            }

            // Compile CSharp code
            Assembly assembly = this.CompileCode(generatedTwofoldSources);
            if (assembly == null) {
                return null;
            }

            // Check Twofold CSharp assembly for entry points/types
            string mainTypeName = this.DetectMainType(mainTemplateFilename, assembly);
            if (string.IsNullOrEmpty(mainTypeName)) {
                return null;
            }

            return new CompiledTemplate(mainTemplateFilename, assembly, mainTypeName, string.Join(Environment.NewLine, generatedTwofoldSources));
        }

        string GenerateCode(string sourceName, string sourceText, out List<string> includedFiles)
        {
            if (sourceName == null) {
                throw new ArgumentNullException("sourceName");
            }
            if (sourceText == null) {
                throw new ArgumentNullException("text");
            }

            TextWriter codeWriter = null;
            includedFiles = new List<string>();
            string generatedCode = null;

            try {
                codeWriter = new StringWriter();
                var csharpGenerator = new CSharpGenerator(templateParser, codeWriter, includedFiles);
                csharpGenerator.Generate(sourceName, sourceText);
                generatedCode = codeWriter.ToString();
            }
            finally {
                codeWriter.Dispose();
            }

            return generatedCode;
        }

        Assembly CompileCode(List<string> sources)
        {
            // Prepare compiler
            var parameters = new CompilerParameters();
            parameters.GenerateInMemory = true;
            parameters.TreatWarningsAsErrors = true;
            parameters.IncludeDebugInformation = false;
            parameters.CompilerOptions = string.Join(" ", Constants.CompilerOptions);
            parameters.ReferencedAssemblies.AddRange(Constants.CompilerAssemblies);
            parameters.ReferencedAssemblies.AddRange(referencedAssemblies.ToArray());

            // Compile
            var codeProvider = new CSharpCodeProvider();
            CompilerResults compilerResults = codeProvider.CompileAssemblyFromSource(parameters, sources.ToArray());

            // Report errors
            foreach (CompilerError compilerError in compilerResults.Errors) {
                var textPosition = new TextPosition();
                if (compilerError.Line != 0 && compilerError.Column != 0) {
                    textPosition = new TextPosition(compilerError.Line, compilerError.Column);
                }

                TraceLevel traceLevel = compilerError.IsWarning ? TraceLevel.Warning : TraceLevel.Error;
                var errorPosition = new TextFilePosition(compilerError.FileName, textPosition);
                messageHandler.CSharpMessage(traceLevel, errorPosition, $"{compilerError.ErrorNumber}: {compilerError.ErrorText}");
            }

            if (compilerResults.Errors.Count > 0) {
                return null;
            }
            return compilerResults.CompiledAssembly;
        }

        string DetectMainType(string sourceName, Assembly assembly)
        {
            Type mainType = null;
            MethodInfo mainMethod = null;

            Type[] exportedTypes = assembly.GetExportedTypes();
            foreach (var exportedType in exportedTypes) {
                mainMethod = exportedType.GetMethod(Constants.EntryMethodName, BindingFlags.Public | BindingFlags.Static);
                if (mainMethod != null) {
                    mainType = exportedType;
                    break;
                }
            }

            if (mainMethod == null) {
                messageHandler.Message(TraceLevel.Error, $"Can't find static template entry method '{Constants.EntryMethodName}' in '{sourceName}'.");
                return null;
            }

            return mainType.FullName;
        }
    }
}
