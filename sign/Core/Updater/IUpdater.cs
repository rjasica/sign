using System.Collections.Generic;
using System.Reflection;

namespace Sign.Core.Updater
{
    public interface IUpdater
    {
        void Update( StrongNameKeyPair snk, HashSet<IAssemblyInfo> notSigned, IEnumerable<IAssemblyInfo> allAssemblies );
    }
}
