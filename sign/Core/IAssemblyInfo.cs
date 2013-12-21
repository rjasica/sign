using Mono.Cecil;

namespace Sign
{
    public interface IAssemblyInfo
    {
        string FullPath { get; }
        AssemblyDefinition Assembly { get; }
    }
}