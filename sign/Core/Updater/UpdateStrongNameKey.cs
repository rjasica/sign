using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sign.Core.Updater
{
    public class UpdateStrongNameKey : IUpdater
    {
        public void Update(System.Reflection.StrongNameKeyPair snk, HashSet<IAssemblyInfo> notSigned, IEnumerable<IAssemblyInfo> allAssemblies)
        {
            foreach (var assemblyInfo in notSigned)
            {
                var name = assemblyInfo.Assembly.Name;
                name.HashAlgorithm = AssemblyHashAlgorithm.SHA1;
                name.PublicKey = snk.PublicKey;
                name.HasPublicKey = true;
                name.Attributes |= AssemblyAttributes.PublicKey;
            }
        }
    }
}
