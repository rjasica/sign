using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using Mono.Cecil;

namespace Sign.Core
{
    public class Signer : ISigner
    {
        private static readonly string KnowTypeAttributeName = typeof(KnownTypeAttribute).FullName;
        private static readonly string TypeFullName = typeof(Type).FullName;

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
            UpdateStrongNameKey( snk, modified );
            UpdateKnowTypeAttribute( snk, modified );

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

        private static void UpdateStrongNameKey(StrongNameKeyPair snk, HashSet<IAssemblyInfo> modified)
        {
            foreach (var assemblyInfo in modified)
            {
                var name = assemblyInfo.Assembly.Name;
                name.HashAlgorithm = AssemblyHashAlgorithm.SHA1;
                name.PublicKey = snk.PublicKey;
                name.HasPublicKey = true;
                name.Attributes |= AssemblyAttributes.PublicKey;
            }
        }

        private static void UpdateKnowTypeAttribute( StrongNameKeyPair snk, HashSet<IAssemblyInfo> modified )
        {
            foreach (var assemblyInfo in modified)
            {
                UpdateKnowTypeAttribute( snk, assemblyInfo.Assembly );
            }
        }

        private static void UpdateKnowTypeAttribute( StrongNameKeyPair snk, AssemblyDefinition assemby )
        {
            foreach( var type in assemby.MainModule.GetTypes().Where(t => t.HasCustomAttributes ) )
            {
                var knownTypeAttribute = type.CustomAttributes.FirstOrDefault( t => t.AttributeType.FullName == KnowTypeAttributeName);
                if( knownTypeAttribute == null )
                {
                    continue;
                }

                var argument = knownTypeAttribute.ConstructorArguments.FirstOrDefault( t => t.Type.FullName == TypeFullName );

                if( argument.Type == null || argument.Value == null )
                {
                    continue;
                }

                var typeReference = argument.Value as TypeReference;
                if( typeReference == null )
                {
                    continue;
                }

                var assemblyReference = typeReference.Scope as AssemblyNameReference;
                if( assemblyReference == null )
                {
                    continue;
                }

                assemblyReference.PublicKey = snk.PublicKey;
            }
        }

        private static void WriteModifiedAssemblies(StrongNameKeyPair snk, HashSet<IAssemblyInfo> modified)
        {
            foreach (var assemblyInfo in modified)
            {
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
