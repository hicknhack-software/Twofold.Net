using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Twofold.Api
{
    public interface ITextLoader
    {
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="IOException"></exception>
        TextLoaderResult Load(string name);
    }

}
