using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Sign.Core.Updater
{
    public class UpdateReference : IUpdater
    {
        public void Update(StrongNameKeyPair snk, HashSet<IAssemblyInfo> notSigned, IEnumerable<IAssemblyInfo> allAssemblies)
        {
            int startState = 0;
            while (startState != notSigned.Count)
            {
                startState = notSigned.Count;
                var set = new HashSet<string>(notSigned.Select(x => x.Assembly.Name.Name));

                foreach (var assembly in allAssemblies)
                {
                    foreach (AssemblyNameReference reference in assembly.Assembly.MainModule.AssemblyReferences)
                    {
                        if (set.Contains(reference.Name))
                        {
                            reference.PublicKey = snk.PublicKey;
                            notSigned.Add(assembly);
                        }
                    }
                }
            }
        }
    }
}
