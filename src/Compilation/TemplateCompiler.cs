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
        readonly List<string> referencedAssemblies;
        readonly TemplateParser templateParser;

        public TemplateCompiler(ITextLoader textLoader, IMessageHandler messageHandler, List<string> referencedAssemblies)
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

        public CompiledTemplate Compile(string templateName)
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
                foreach (var includedFile in includedFiles) {
                    templateNames.Enqueue(includedFile);
                }
            }

            // Compile CSharp code
            Assembly assembly = this.CompileCode(generatedTwofoldSources);

            // Check Twofold CSharp assembly for entry points/types
            string mainTypeName = this.DetectMainType(mainTemplateFilename, assembly);
            if (string.IsNullOrEmpty(mainTypeName)) {
                return null;
            }

            return new CompiledTemplate(mainTemplateFilename, assembly, mainTypeName);
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
            parameters.ReferencedAssemblies.Add("System.dll");
            parameters.ReferencedAssemblies.Add("System.Core.dll");
            parameters.ReferencedAssemblies.AddRange(referencedAssemblies.ToArray());

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

            return mainType.AssemblyQualifiedName;
        }
    }
}
