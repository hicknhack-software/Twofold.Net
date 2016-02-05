using System.Reflection;

namespace Twofold
{
    public class Template
    {
        public bool IsValid
        {
            get { return Assembly != null; }
        }
        public Assembly @Assembly { get; private set; }

        public Template() { }

        public Template(Assembly assembly)
        {
            Assembly = assembly;
        }
    }
}
