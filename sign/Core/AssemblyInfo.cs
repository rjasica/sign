using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace Sign
{
    public class AssemblyInfo : IAssemblyInfo
    {
        public AssemblyInfo(string fullPath, AssemblyDefinition assembly)
        {
            FullPath = fullPath;
            Assembly = assembly;
        }

        public string FullPath { get; private set; }

        public AssemblyDefinition Assembly { get; private set; }
    }
}
