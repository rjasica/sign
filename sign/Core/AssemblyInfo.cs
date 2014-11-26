using Mono.Cecil;

namespace Sign.Core
{
    public class AssemblyInfo : IAssemblyInfo
    {
        public AssemblyInfo(string fullPath, AssemblyDefinition assembly)
        {
            this.FullPath = fullPath;
            this.Assembly = assembly;
        }

        public string FullPath { get; private set; }

        public AssemblyDefinition Assembly { get; private set; }
    }
}
