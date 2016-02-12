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
using Twofold.Execution;

namespace Twofold
{
    public class Engine
    {
        readonly ITemplateLoader templateLoader;
        readonly IMessageHandler messageHandler;
        readonly List<string> referencedAssemblies = new List<string>();

        public Engine(ITemplateLoader templateLoader, IMessageHandler messageHandler, params string[] referencedAssemblies)
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
            var templateCompiler = new TemplateCompiler(templateLoader, messageHandler, referencedAssemblies);
            CompiledTemplate compiledTemplate = templateCompiler.Compile(templateName);
            return compiledTemplate;
        }

        /// <summary>
        /// Executed a compiled Twofold template.
        /// </summary>
        /// <typeparam name="T">The argument type of the template main method.</typeparam>
        /// <param name="compiledTemplate">The compiled Twofold template.</param>
        /// <param name="input">The parameter which is given to the template main method.</param>
        /// <returns>The generated target text or null if an error occured.</returns>
        /// <exception cref="ArgumentNullException">If compiledTemplate is null.</exception>
        public Target Run<T>(CompiledTemplate compiledTemplate, T input)
        {
            var templateExecuter = new TemplateExecuter(messageHandler);
            Target target = templateExecuter.Execute(compiledTemplate, input);
            return target;
        }
    }
}
