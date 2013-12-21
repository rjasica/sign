using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sign.Core
{
    public interface IAssemblyInfoProvider
    {
        IEnumerable<IAssemblyInfo> GetAssemblies();
    }
}
