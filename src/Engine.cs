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
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Twofold.Api;
using Twofold.Compilation;

namespace Twofold
{
    public class Engine
    {
        readonly ITextLoader templateLoader;
        readonly IMessageHandler messageHandler;
        readonly List<string> referencedAssemblies = new List<string>();

        public Engine(ITextLoader templateLoader, IMessageHandler messageHandler, params string[] referencedAssemblies)
        {
            this.templateLoader = templateLoader;
            this.messageHandler = messageHandler;
            this.referencedAssemblies.AddRange(referencedAssemblies);
        }

        /// <summary>
        /// Compiles a Twofold template into internal representation.
        /// </summary>
        /// <param name="templateName">Name of Twofold template.</param>
        /// <returns>The compiled template or null if an error occured.</returns>
        public CompiledTemplate Compile(string templateName)
        {
            CompiledTemplate compiledTemplate = null;
            try {
                var templateCompiler = new TemplateCompiler(templateLoader, messageHandler, referencedAssemblies);
                compiledTemplate = templateCompiler.Compile(templateName);
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
            return compiledTemplate;
        }

        /// <summary>
        /// Executed a compiled Twofold template.
        /// </summary>
        /// <typeparam name="T">The argument type of the template main method.</typeparam>
        /// <param name="compiledTemplate">The compiled Twofold template.</param>
        /// <param name="input">The parameter which is given to the template main method.</param>
        /// <returns>The generated target text or null if an error occured.</returns>
        public Target Run<T>(CompiledTemplate compiledTemplate, T input) where T : class
        {
            Assembly assembly = compiledTemplate.Assembly;
            Type mainType = assembly.GetType(compiledTemplate.MainTypeName);
            if (mainType == null) {
                messageHandler.Message(TraceLevel.Error, $"Can't find main type in '{compiledTemplate.SourceName}'.");
                return null;
            }

            MethodInfo mainMethod = mainType.GetMethod(Constants.EntryMethodName, BindingFlags.Public | BindingFlags.Static);
            if (mainMethod == null) {
                messageHandler.Message(TraceLevel.Error, $"Can't find main method in '{compiledTemplate.SourceName}'.");
                return null;
            }

            // Validate parameters of main method
            ParameterInfo[] parameters = mainMethod.GetParameters();
            bool parameterCountInvalid = (parameters.Length != 1);
            bool parameterInvalid = false;
            if (parameterCountInvalid == false) {
                ParameterInfo param = parameters[0];
                parameterInvalid |= param.HasDefaultValue;
                parameterInvalid |= (!param.IsIn);
                parameterInvalid |= param.IsLcid;
                parameterInvalid |= param.IsOptional;
                parameterInvalid |= param.IsOut;
                parameterInvalid |= param.IsRetval;
                parameterInvalid |= param.ParameterType.IsAssignableFrom(typeof(T));
            }

            if (parameterCountInvalid || parameterInvalid) {
                messageHandler.Message(TraceLevel.Error, $"Template main method in '{compiledTemplate.SourceName}' has invalid signature. Expected 'public static {Constants.EntryMethodName}({typeof(T).ToString()})'.");
                return null;
            }

            // Invoke main method
            try {
                mainMethod.Invoke(null, new object[] { input });
            }
            catch (Exception ex) {
                messageHandler.Message(TraceLevel.Error, $"An exception occured in '{compiledTemplate.SourceName}': {ex.ToString()}");
            }

            return new Target();
        }
    }
}
