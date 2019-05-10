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
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Emit;
    using Rules;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    internal sealed class TemplateCompiler
    {
        private readonly ITemplateLoader TemplateLoader;
        private readonly IMessageHandler MessageHandler;
        private readonly IEnumerable<Assembly> UserAssemblies;
        private readonly TemplateParser TemplateParser;

        /// <summary>
        /// Constructs a TemplateCompiler.
        /// </summary>
        /// <param name="templateLoader">ITemplateLoader to load templates by name.</param>
        /// <param name="messageHandler">IMessageHandler to report messges to the user.</param>
        /// <param name="userAssemblies">Additional assemblied to include in template compilation.</param>
        public TemplateCompiler(ITemplateLoader templateLoader, IMessageHandler messageHandler, IEnumerable<Assembly> userAssemblies)
        {
            this.TemplateLoader = templateLoader ?? throw new ArgumentNullException(nameof(templateLoader));
            this.MessageHandler = messageHandler ?? throw new ArgumentNullException(nameof(messageHandler));
            this.UserAssemblies = userAssemblies;

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
                GeneratedCode generatedCode = this.GenerateCode(template.Path, template.Text, out List<string> includedFiles);
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
            Assembly assembly = this.CompileCode(templateName, generatedTargetCodes);
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

        private Assembly CompileCode(string templateName, List<GeneratedCode> generatedCodes)
        {
            var parseOptions = new CSharpParseOptions(LanguageVersion.CSharp7);

            var syntraxTrees = generatedCodes
                .Select(generatedCode => SyntaxFactory.ParseSyntaxTree(generatedCode.Code, parseOptions, generatedCode.TemplatePath))
                .ToList();

            var referencedTypes = new Type[] { typeof(object), typeof(TemplateCompiler) };
            var systemAssemblies = referencedTypes.Select(type => type.GetTypeInfo().Assembly).ToList();
            var allAssemblies = systemAssemblies.Union(this.UserAssemblies).ToList();
            var references = allAssemblies.Select(assembly => MetadataReference.CreateFromFile(assembly.Location)).ToList();

            // NOTE(Lathan): Suppress diagnostic warning for unknown #pragma include
            var hiddenDiagnostics = new Dictionary<string, ReportDiagnostic> { { "CS1633", ReportDiagnostic.Hidden } };

            var compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                .WithGeneralDiagnosticOption(ReportDiagnostic.Error)
                .WithSpecificDiagnosticOptions(hiddenDiagnostics);
            var compilation = CSharpCompilation.Create(templateName)
                .WithOptions(compilationOptions)
                .AddReferences(references)
                .AddSyntaxTrees(syntraxTrees);

            using (var memoryStream = new MemoryStream())
            {
                EmitResult result = compilation.Emit(memoryStream);

                if (result.Success)
                {
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    return Assembly.Load(memoryStream.ToArray());
                }
                else
                {
                    IEnumerable<Diagnostic> issues = result.Diagnostics
                        .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Warning ||
                        diagnostic.Severity == DiagnosticSeverity.Error);

                    foreach (Diagnostic issue in issues)
                    {
                        FileLinePositionSpan lineSpan = issue.Location.GetMappedLineSpan();
                        int line = lineSpan.StartLinePosition.Line + 1;
                        int column = lineSpan.StartLinePosition.Character + 1;

                        string filepath = lineSpan.Path;
                        var textPosition = new TextPosition();
                        if (lineSpan.IsValid)
                        {
                            GeneratedCode generatedCode = generatedCodes.FirstOrDefault(gcode => string.Compare(gcode.TemplatePath, filepath, StringComparison.OrdinalIgnoreCase) == 0);
                            var errorPosition = new TextPosition(line, column);
                            if (generatedCode != null)
                            {
                                TextFilePosition original = generatedCode.SourceMap.FindSourceByGenerated(errorPosition);
                                if (original.IsValid)
                                {
                                    textPosition = new TextPosition(original.Line, original.Column);
                                }
                            }
                            else
                            {
                                this.MessageHandler.Message(TraceLevel.Error, $"Couldn't lookup source position for {filepath} {errorPosition} in source map. Using compiler reported position.", null, new TextPosition());
                                textPosition = new TextPosition(line, column);
                            }

                        }

                        TraceLevel traceLevel = issue.Severity == DiagnosticSeverity.Warning ? TraceLevel.Warning : TraceLevel.Error;
                        this.MessageHandler.Message(traceLevel, $"{issue.Id}: {issue.GetMessage()}", filepath, textPosition);
                    }

                    return null;
                }
            }
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