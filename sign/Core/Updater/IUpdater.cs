using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Sign.Core
{
    public interface IUpdater
    {
        void Update( StrongNameKeyPair snk, HashSet<IAssemblyInfo> notSigned, IEnumerable<IAssemblyInfo> allAssemblies );
    }
}
