using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace Sign.Core.Updater
{
    public class UpdateKnowTypeAttribute : IUpdater
    {
        private static readonly string KnowTypeAttributeName = typeof(KnownTypeAttribute).FullName;

        private static readonly string TypeFullName = typeof(Type).FullName;

        public void Update(System.Reflection.StrongNameKeyPair snk, HashSet<IAssemblyInfo> modified, IEnumerable<IAssemblyInfo> allAssemblies)
        {
            foreach (var assemblyInfo in modified)
            {
                UpdateAttribute(snk, assemblyInfo.Assembly);
            }
        }

        private void UpdateAttribute(StrongNameKeyPair snk, AssemblyDefinition assemby)
        {
            foreach (var type in assemby.MainModule.GetTypes().Where(t => t.HasCustomAttributes))
            {
                foreach (var knownTypeAttribute in type.CustomAttributes.Where(t => t.AttributeType.FullName == KnowTypeAttributeName))
                {
                    if (knownTypeAttribute == null)
                    {
                        continue;
                    }

                    var argument = knownTypeAttribute.ConstructorArguments.FirstOrDefault(t => t.Type.FullName == TypeFullName);

                    if (argument.Type == null || argument.Value == null)
                    {
                        continue;
                    }

                    var typeReference = argument.Value as TypeReference;
                    if (typeReference == null)
                    {
                        continue;
                    }

                    var assemblyReference = typeReference.Scope as AssemblyNameReference;
                    if (assemblyReference == null)
                    {
                        continue;
                    }

                    assemblyReference.PublicKey = snk.PublicKey;
                }
            }
        }
    }
}
