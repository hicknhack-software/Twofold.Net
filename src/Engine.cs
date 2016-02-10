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
using System.Collections.Specialized;
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
        readonly StringCollection referencedAssemblies = new StringCollection();

        public Engine(ITextLoader templateLoader, IMessageHandler messageHandler, params string[] referencedAssemblies)
        {
            this.templateLoader = templateLoader;
            this.messageHandler = messageHandler;
            this.referencedAssemblies.AddRange(referencedAssemblies);
        }

        public Template Compile(string templateName)
        {
            Template template = null;
            try {
                var templateCompiler = new TemplateCompiler(templateLoader, messageHandler, referencedAssemblies);
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

        public Target Run<T>(Template template, T input) where T : class
        {
            Assembly assembly = template.Assembly;
            return null;
        }
    }
}
