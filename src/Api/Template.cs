using System.Reflection;

namespace Twofold.Api
{
    public class Template
    {
        public Assembly @Assembly { get; private set; }

        public Template(Assembly assembly)
        {
            Assembly = assembly;
        }
    }
}
