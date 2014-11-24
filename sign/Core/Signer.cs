using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Sign.Core.Updater;

namespace Sign.Core
{
    public class Signer : ISigner
    {
        private readonly IUpdater[] updaters;

        public Signer()
        {
            this.updaters = new IUpdater[]
            {
                new UpdateReference(),
                new UpdateStrongNameKey(),
                new UpdateInternalVisibleToAttribute(),
                new UpdateTypeAttributeArguments(),
                new UpdateBaml()
            };
        }

        public void Sign( StrongNameKeyPair snk, IAssemblyInfoProvider assemblyInfoProvider )
        {
            var allAssemblies = assemblyInfoProvider.GetAssemblies();
            var notSignedAssemblies = GetNotSignedAssemblies( allAssemblies );

            foreach( var updater in this.updaters )
            {
                updater.Update( snk, notSignedAssemblies, allAssemblies );
            }

            WriteModifiedAssemblies( snk, notSignedAssemblies );
        }

        private static bool IsSigned( AssemblyDefinition assemblyDefinition )
        {
            var name = assemblyDefinition.Name;
            return ( name.PublicKey != null && name.PublicKey.Length != 0 )
                   || ( name.PublicKeyToken != null && name.PublicKeyToken.Length != 0 );
        }

        private static void WriteModifiedAssemblies( StrongNameKeyPair snk, IEnumerable<IAssemblyInfo> notSigned )
        {
            foreach( var assemblyInfo in notSigned )
            {
                assemblyInfo.Assembly.Write( assemblyInfo.FullPath, new WriterParameters { StrongNameKeyPair = snk } );
            }
        }

        private static HashSet<IAssemblyInfo> GetNotSignedAssemblies( IEnumerable<IAssemblyInfo> allAssemblies )
        {
            var modified = new HashSet<IAssemblyInfo>();

            foreach( var assemblyInfo in allAssemblies.Where( x => !IsSigned( x.Assembly ) ) )
            {
                modified.Add( assemblyInfo );
            }

            return modified;
        }
    }
}
