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

namespace HicknHack.Twofold.Compilation
{
    using Abstractions;
    using Abstractions.Compilation;
    using Abstractions.SourceMapping;
    using Microsoft.CSharp;
    using Rules;
    using System;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    internal sealed class TemplateCompiler
    {
        private readonly ITemplateLoader TemplateLoader;
        private readonly IMessageHandler MessageHandler;
        private readonly List<string> ReferencedAssemblies;
        private readonly TemplateParser TemplateParser;

        /// <summary>
        /// Constructs a TemplateCompiler.
        /// </summary>
        /// <param name="templateLoader">ITemplateLoader to load templates by name.</param>
        /// <param name="messageHandler">IMessageHandler to report messges to the user.</param>
        /// <param name="referencedAssemblies">Additional assemblied to include in template compilation.</param>
        public TemplateCompiler(ITemplateLoader templateLoader, IMessageHandler messageHandler, List<string> referencedAssemblies)
        {
            if (templateLoader == null)
            {
                throw new ArgumentNullException(nameof(templateLoader));
            }
            if (messageHandler == null)
            {
                throw new ArgumentNullException(nameof(messageHandler));
            }

            this.TemplateLoader = templateLoader;
            this.MessageHandler = messageHandler;
            this.ReferencedAssemblies = referencedAssemblies;

            var parserRules = new Dictionary<char, IParserRule>
            {
                {'\\', new InterpolationRule() },
                {'|', new InterpolationLineRule() },
                {'=', new CallRule() },
                {'#', new PreprocessorRule() },
            };
            this.TemplateParser = new TemplateParser(parserRules, new PassThroughRule(), this.MessageHandler);
        }

        /// <summary>
        /// Loads the given template and compiles it.
        /// </summary>
        /// <param name="templateName">Name of template which will be resolved by ITemplateLoader.</param>
        /// <returns>Compiled template or null in case of an error.</returns>
        /// <exception cref="ArgumentException">If templateName is null or empty.</exception>
        public CompiledTemplate Compile(string templateName)
        {
            if (string.IsNullOrEmpty(templateName))
            {
                throw new ArgumentException("Can't be null or empty.", nameof(templateName));
            }

            var templateNames = new Queue<string>();
            templateNames.Enqueue(templateName);

            var generatedTargetCodes = new List<GeneratedCode>();

            string mainTemplatePath = null;
            bool mainTemplateFilenameSet = false;
            var generatedFiles = new HashSet<string>();

            while (templateNames.Count > 0)
            {
                // Load template
                string loadTemplateName = templateNames.Dequeue();
                Template template = null;
                try
                {
                    template = this.TemplateLoader.Load(loadTemplateName);
                    if (mainTemplateFilenameSet == false)
                    {
                        mainTemplatePath = template.Path;
                        mainTemplateFilenameSet = true;
                    }
                }
                catch (FileNotFoundException)
                {
                    this.MessageHandler.Message(TraceLevel.Error, $"Can't find template '{templateName}'.", string.Empty, new TextPosition());
                    throw;
                }
                catch (IOException)
                {
                    this.MessageHandler.Message(TraceLevel.Error, $"IO error while reading template '{templateName}'.", string.Empty, new TextPosition());
                    throw;
                }
                catch (Exception)
                {
                    continue;
                }

                // Generate Twofold enhanced CSharp code from template
                List<string> includedFiles;
                GeneratedCode generatedCode = this.GenerateCode(template.Path, template.Text, out includedFiles);
                if (generatedCode != null)
                {
                    generatedFiles.Add(loadTemplateName);
                    generatedTargetCodes.Add(generatedCode);
                    foreach (var includedFile in includedFiles)
                    {
                        if (generatedFiles.Contains(includedFile))
                        {
                            continue;
                        }
                        templateNames.Enqueue(includedFile);
                    }
                }
            }

            // Compile CSharp code
            Assembly assembly = this.CompileCode(generatedTargetCodes);
            if (assembly == null)
            {
                return new CompiledTemplate(mainTemplatePath, generatedTargetCodes);
            }

            // Check Twofold CSharp assembly for entry points/types
            string mainTypeName = this.DetectMainType(mainTemplatePath, assembly);
            if (string.IsNullOrEmpty(mainTypeName))
            {
                return new CompiledTemplate(mainTemplatePath, generatedTargetCodes);
            }

            return new CompiledTemplate(mainTemplatePath, assembly, mainTypeName, generatedTargetCodes);
        }

        private GeneratedCode GenerateCode(string templatePath, string originalText, out List<string> includedFiles)
        {
            if (templatePath == null)
            {
                throw new ArgumentNullException(nameof(templatePath));
            }
            if (originalText == null)
            {
                throw new ArgumentNullException(nameof(originalText), "text");
            }

            TextWriter codeWriter = new StringWriter();
            includedFiles = new List<string>();
            string generatedCode = string.Empty;
            var mapping = new Mapping();

            try
            {
                var csharpGenerator = new CSharpGenerator(this.TemplateParser, codeWriter, mapping, includedFiles);
                csharpGenerator.Generate(templatePath, originalText);
                generatedCode = codeWriter.ToString();
            }
            finally
            {
                codeWriter.Dispose();
            }

            return new GeneratedCode(templatePath, generatedCode, mapping);
        }

        private Assembly CompileCode(List<GeneratedCode> generatedCodes)
        {
            // Prepare compiler
            var parameters = new CompilerParameters();
            parameters.GenerateInMemory = true;
            parameters.TreatWarningsAsErrors = true;
            parameters.IncludeDebugInformation = true;
            parameters.CompilerOptions = string.Join(" ", Constants.CompilerOptions);
            parameters.ReferencedAssemblies.AddRange(Constants.CompilerAssemblies);
            parameters.ReferencedAssemblies.AddRange(this.ReferencedAssemblies.ToArray());
            parameters.ReferencedAssemblies.Add(typeof(TemplateCompiler).Assembly.Location);

            // Compile
            CompilerResults compilerResults;
            using (var codeProvider = new CSharpCodeProvider())
            {
                var codes = generatedCodes.Select(generatedCode => generatedCode.Code).ToArray();
                compilerResults = codeProvider.CompileAssemblyFromSource(parameters, codes);
            }

            // Report errors
            foreach (CompilerError compilerError in compilerResults.Errors)
            {
                string filename = compilerError.FileName;
                var textPosition = new TextPosition();
                if (compilerError.Line != 0 && compilerError.Column != 0)
                {
                    GeneratedCode generatedCode = generatedCodes.FirstOrDefault(gcode => string.Compare(gcode.TemplatePath, compilerError.FileName, StringComparison.OrdinalIgnoreCase) == 0);
                    if (generatedCode != null)
                    {
                        var errorPosition = new TextPosition(compilerError.Line, compilerError.Column);
                        TextFilePosition original = generatedCode.SourceMap.FindSourceByGenerated(errorPosition);
                        if (original.IsValid)
                        {
                            textPosition = new TextPosition(original.Line, original.Column);
                        }
                    }
                    else
                    {
                        textPosition = new TextPosition(compilerError.Line, compilerError.Column);
                    }

                }

                TraceLevel traceLevel = compilerError.IsWarning ? TraceLevel.Warning : TraceLevel.Error;
                this.MessageHandler.Message(traceLevel, $"{compilerError.ErrorNumber}: {compilerError.ErrorText}", filename, textPosition);
            }

            if (compilerResults.Errors.Count > 0)
            {
                return null;
            }
            return compilerResults.CompiledAssembly;
        }

        private string DetectMainType(string templatePath, Assembly assembly)
        {
            Type mainType = null;
            MethodInfo mainMethod = null;

            Type[] exportedTypes = assembly.GetExportedTypes();
            foreach (var exportedType in exportedTypes)
            {
                mainMethod = exportedType.GetMethod(Constants.EntryMethodName, BindingFlags.Public | BindingFlags.Static);
                if (mainMethod != null)
                {
                    mainType = exportedType;
                    break;
                }
            }

            if (mainMethod == null)
            {
                this.MessageHandler.Message(TraceLevel.Error, $"Can't find static template entry method '{Constants.EntryMethodName}'.", templatePath, new TextPosition());
                return null;
            }

            return mainType.FullName;
        }
    }
}