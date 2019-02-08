﻿/* Twofold.Net
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

namespace HicknHack.Twofold.Execution
{
    using Compilation;
    using Abstractions;
    using Abstractions.SourceMapping;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using TextRendering;

    internal class TemplateExecuter
    {
        private readonly IMessageHandler MessageHandler;

        public TemplateExecuter(IMessageHandler messageHandler)
        {
            this.MessageHandler = messageHandler;
        }

        public Target Execute(CompiledTemplate compiledTemplate, params object[] arguments)
        {
            if (compiledTemplate == null)
            {
                throw new ArgumentNullException(nameof(compiledTemplate));
            }

            if (compiledTemplate.IsValid == false)
            {
                throw new ArgumentException("Not valid", nameof(compiledTemplate));
            }

            Assembly assembly = compiledTemplate.Assembly;
            Type mainType = assembly.GetType(compiledTemplate.MainTypeName);
            if (mainType == null)
            {
                this.MessageHandler.Message(TraceLevel.Error, $"Can't find main type '{compiledTemplate.MainTypeName}'.", compiledTemplate.OriginalName, new TextPosition());
                return null;
            }

            MethodInfo mainMethod = mainType.GetMethod(Constants.EntryMethodName, BindingFlags.Public | BindingFlags.Static);
            if (mainMethod == null)
            {
                this.MessageHandler.Message(TraceLevel.Error, $"Can't find main method '{Constants.EntryMethodName}'.", compiledTemplate.OriginalName, new TextPosition());
                return null;
            }

            // Validate parameters of main method
            ParameterInfo[] paramInfos = mainMethod.GetParameters();
            bool argumentCountInvalid = (paramInfos.Length != arguments.Length);
            bool argumentsInvalid = false;
            if (argumentCountInvalid == false)
            {
                int argumentIndex = 0;
                foreach (ParameterInfo paramInfo in paramInfos)
                {
                    argumentsInvalid |= paramInfo.HasDefaultValue;
                    argumentsInvalid |= paramInfo.IsIn;
                    argumentsInvalid |= paramInfo.IsLcid;
                    argumentsInvalid |= paramInfo.IsOptional;
                    argumentsInvalid |= paramInfo.IsOut;
                    argumentsInvalid |= paramInfo.IsRetval;

                    object argument = arguments[argumentIndex++];
                    argumentsInvalid |= (paramInfo.ParameterType.IsAssignableFrom(argument.GetType()) == false);
                    if (argumentsInvalid)
                    {
                        break;
                    }
                }
            }

            if (argumentCountInvalid || argumentsInvalid)
            {
                var paramTypes = new List<string>();
                foreach (ParameterInfo param in paramInfos)
                {
                    paramTypes.Add(param.ParameterType.ToString());
                }
                string paramSignature = string.Join(", ", paramTypes);
                this.MessageHandler.Message(TraceLevel.Error, $"Template main method has invalid signature. Expected 'public static void {Constants.EntryMethodName}({paramSignature})'.", compiledTemplate.OriginalName, new TextPosition());
                return null;
            }

            // Invoke main method
            var textWriter = new StringWriter();
            var mapping = new Mapping();
            try
            {
                TemplateRenderer.SetTextWriter(textWriter, mapping);
                mainMethod.Invoke(null, arguments);
            }
            catch (Exception ex)
            {
                Exception templateException = ex.InnerException;
                this.PrintException(templateException, compiledTemplate.GeneratedCodes);
            }

            var target = new Target(compiledTemplate.OriginalName, textWriter.ToString(), mapping);
            textWriter.Dispose();
            return target;
        }

        private void PrintException(Exception ex, List<GeneratedCode> generatedCodes)
        {
            var frames = new List<Abstractions.StackFrame>();

            var stackTrace = new StackTrace(ex, true);
            for (int i = 0; i < stackTrace.FrameCount; ++i)
            {
                System.Diagnostics.StackFrame diagnosticFrame = stackTrace.GetFrame(i);

                // Extract method name
                MethodBase method = diagnosticFrame.GetMethod();
                // Note(Maik): Skip internal render methods in case of a stack trace.
                if (Attribute.IsDefined(method, typeof(HideStackTraceAttribute)))
                {
                    continue;
                }
                Type declaringType = method.DeclaringType;
                var methodSb = new StringBuilder();
                if (declaringType != null)
                {
                    methodSb.Append(declaringType.FullName).Append(".");
                }
                methodSb.Append(method.Name);

                // Extract original filename, line and column
                int? line = null;
                if (diagnosticFrame.GetFileLineNumber() != 0)
                {
                    line = diagnosticFrame.GetFileLineNumber();
                }

                int? column = null;
                if (diagnosticFrame.GetFileColumnNumber() != 0)
                {
                    column = diagnosticFrame.GetFileColumnNumber();
                }

                string filename = diagnosticFrame.GetFileName();
                GeneratedCode generatedCode = generatedCodes.FirstOrDefault(gcode => string.Compare(gcode.TemplatePath, filename, StringComparison.OrdinalIgnoreCase) == 0);
                if ((generatedCode != null) && (line != null))
                {
                    var position = new TextPosition(line.Value, column ?? 1);
                    TextFilePosition original = generatedCode.SourceMap.FindSourceByGenerated(position);
                    if (original.IsValid)
                    {
                        filename = original.Name;
                        line = original.Line;
                        column = original.Column;
                    }
                }

                var msgFrame = new Abstractions.StackFrame(methodSb.ToString(), filename, line, column);
                frames.Add(msgFrame);
            } // for

            this.MessageHandler.Message(TraceLevel.Error, $"{ex.GetType().FullName}: {ex.Message}", string.Empty, new TextPosition());
            this.MessageHandler.StackTrace(frames);
        }

    }
}