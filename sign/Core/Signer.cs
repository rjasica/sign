using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Mono.Cecil;

namespace Sign.Core
{
    public class Signer : ISigner
    {
        private static bool IsSigned(AssemblyDefinition assemblyDefinition)
        {
            var name = assemblyDefinition.Name;
            return (name.PublicKey != null && name.PublicKey.Length != 0)
                   || (name.PublicKeyToken != null && name.PublicKeyToken.Length != 0);
        }

        public void Sign(StrongNameKeyPair snk, IAssemblyInfoProvider assemblyInfoProvider)
        {
            var allAssemblies = assemblyInfoProvider.GetAssemblies();
            var modified = GetNotSignedAssemblies(allAssemblies);

            UpdateReference(snk, modified, allAssemblies);

            WriteModifiedAssemblies(snk, modified);
        }

        private static void UpdateReference(StrongNameKeyPair snk, HashSet<IAssemblyInfo> modified, IEnumerable<IAssemblyInfo> allAssemblies)
        {
            int startState = 0;
            while (startState != modified.Count)
            {
                startState = modified.Count;
                var set = new HashSet<string>(modified.Select(x => x.Assembly.Name.Name));

                foreach (var assembly in allAssemblies)
                {
                    foreach (AssemblyNameReference reference in assembly.Assembly.MainModule.AssemblyReferences)
                    {
                        if (set.Contains(reference.Name))
                        {
                            reference.PublicKey = snk.PublicKey;
                            modified.Add(assembly);
                        }
                    }
                }
            }
        }

        private static void WriteModifiedAssemblies(StrongNameKeyPair snk, HashSet<IAssemblyInfo> modified)
        {
            foreach (var assemblyInfo in modified)
            {
                var name = assemblyInfo.Assembly.Name;
                name.HashAlgorithm = AssemblyHashAlgorithm.SHA1;
                name.PublicKey = snk.PublicKey;
                name.HasPublicKey = true;
                name.Attributes |= AssemblyAttributes.PublicKey;
                assemblyInfo.Assembly.Write(assemblyInfo.FullPath, new WriterParameters {StrongNameKeyPair = snk});
            }
        }

        private static HashSet<IAssemblyInfo> GetNotSignedAssemblies(IEnumerable<IAssemblyInfo> allAssemblies)
        {
            var modified = new HashSet<IAssemblyInfo>();

            foreach (var assemblyInfo in allAssemblies.Where(x => !IsSigned(x.Assembly)))
            {
                modified.Add(assemblyInfo);
            }
            return modified;
        }
    }
}
