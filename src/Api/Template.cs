using System.Reflection;

namespace Twofold.Api
{
    public class Template
    {
        public readonly Assembly @Assembly;
        public readonly string Name;

        public Template(string name, Assembly assembly)
        {
            Name = name;
            Assembly = assembly;
        }
    }
}
