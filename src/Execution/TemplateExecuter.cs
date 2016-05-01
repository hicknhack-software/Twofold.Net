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

namespace Twofold.Execution
{
    using Interface;
    using Interface.SourceMapping;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using TextRendering;

    internal class TemplateExecuter
    {
        private readonly IMessageHandler MessageHandler;

        public TemplateExecuter(IMessageHandler messageHandler)
        {
            this.MessageHandler = messageHandler;
        }

        public Target Execute<T>(CompiledTemplate compiledTemplate, T input)
        {
            if (compiledTemplate == null)
            {
                throw new ArgumentNullException(nameof(compiledTemplate));
            }

            Assembly assembly = compiledTemplate.Assembly;
            Type mainType = assembly.GetType(compiledTemplate.MainTypeName);
            if (mainType == null)
            {
                this.MessageHandler.Message(TraceLevel.Error, $"Can't find main type '{compiledTemplate.MainTypeName}'.", compiledTemplate.SourceName, new TextPosition());
                return null;
            }

            MethodInfo mainMethod = mainType.GetMethod(Constants.EntryMethodName, BindingFlags.Public | BindingFlags.Static);
            if (mainMethod == null)
            {
                this.MessageHandler.Message(TraceLevel.Error, $"Can't find main method '{Constants.EntryMethodName}'.", compiledTemplate.SourceName, new TextPosition());
                return null;
            }

            // Validate parameters of main method
            ParameterInfo[] parameters = mainMethod.GetParameters();
            bool parameterCountInvalid = (parameters.Length != 1);
            bool parameterInvalid = false;
            if (parameterCountInvalid == false)
            {
                ParameterInfo param = parameters[0];
                parameterInvalid |= param.HasDefaultValue;
                parameterInvalid |= param.IsIn;
                parameterInvalid |= param.IsLcid;
                parameterInvalid |= param.IsOptional;
                parameterInvalid |= param.IsOut;
                parameterInvalid |= param.IsRetval;
                parameterInvalid |= (param.ParameterType.IsAssignableFrom(typeof(T)) == false);
            }

            if (parameterCountInvalid || parameterInvalid)
            {
                this.MessageHandler.Message(TraceLevel.Error, $"Template main method has invalid signature. Expected 'public static {Constants.EntryMethodName}({typeof(T)})'.", compiledTemplate.SourceName, new TextPosition());
                return null;
            }

            // Invoke main method
            var textWriter = new StringWriter();
            var sourceMap = new SourceMap();
            try
            {
                TargetRenderer.SetTextWriter(textWriter, sourceMap);
                mainMethod.Invoke(null, new object[] { input });
            }
            catch (Exception ex)
            {
                this.MessageHandler.Message(TraceLevel.Error, ex.ToString(), compiledTemplate.SourceName, new TextPosition());
            }

            var target = new Target(compiledTemplate.SourceName, textWriter.ToString(), sourceMap);
            textWriter.Dispose();
            return target;
        }
    }
}