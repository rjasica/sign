using System.Collections.Generic;

using Mono.Cecil;

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
