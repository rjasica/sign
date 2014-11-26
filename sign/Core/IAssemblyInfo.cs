using Mono.Cecil;

namespace Sign.Core
{
    public interface IAssemblyInfo
    {
        string FullPath { get; }

        AssemblyDefinition Assembly { get; }
    }
}