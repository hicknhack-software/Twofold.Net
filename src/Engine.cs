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

namespace Twofold
{
    using Compilation;
    using Execution;
    using Interface;
    using System.Collections.Generic;

    public class Engine
    {
        private readonly ITemplateLoader TemplateLoader;
        private readonly IMessageHandler MessageHandler;
        private readonly List<string> ReferencedAssemblies;

        public Engine(ITemplateLoader templateLoader, IMessageHandler messageHandler, params string[] referencedAssemblies)
        {
            this.TemplateLoader = templateLoader;
            this.MessageHandler = messageHandler;
            this.ReferencedAssemblies = new List<string>();
            this.ReferencedAssemblies.AddRange(referencedAssemblies);
        }

        /// <summary>
        /// Compiles a Twofold template into internal representation.
        /// </summary>
        /// <param name="templateName">Name of Twofold template.</param>
        /// <returns>The compiled template or null if an error occured.</returns>
        public CompiledTemplate Compile(string templateName)
        {
            var templateCompiler = new TemplateCompiler(this.TemplateLoader, this.MessageHandler, this.ReferencedAssemblies);
            return templateCompiler.Compile(templateName);
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
            var templateExecuter = new TemplateExecuter(this.MessageHandler);
            Target target = templateExecuter.Execute(compiledTemplate, input);
            return target;
        }
    }
}