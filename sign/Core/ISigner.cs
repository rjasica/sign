using System.Collections.Generic;
using System.Reflection;

namespace Sign.Core
{
    public interface ISigner
    {
        void Sign(StrongNameKeyPair snk, IAssemblyInfoProvider assemblyInfoProvider);
    }
}