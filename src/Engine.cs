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
