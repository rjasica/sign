using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Mono.Cecil;

namespace Sign.Core.Updater
{
    public class UpdateTypeAttributeArguments : IUpdater
    {
        private static readonly string TypeFullName = typeof( Type ).FullName;

        public void Update( StrongNameKeyPair snk, HashSet<IAssemblyInfo> notSigned, IEnumerable<IAssemblyInfo> allAssemblies )
        {
            foreach( var assemblyInfo in notSigned )
            {
                UpdateAttributesInAssembly( snk, assemblyInfo.Assembly );
            }
        }

        private static void UpdateAttributesOnType( TypeDefinition type, StrongNameKeyPair snk )
        {
            foreach( var attribute in type.CustomAttributes )
            {
                var assemblyNameReferences = attribute.ConstructorArguments.Where( t => t.Type.FullName == TypeFullName );

                foreach( var argument in assemblyNameReferences
                    .Select( s => s.Value as TypeReference )
                    .Where( s => s != null )
                    .Select( s => s.Scope as AssemblyNameReference )
                    .Where( a => a != null )
                    .Where( a => !a.HasPublicKey )
                    .Where( a => a.PublicKeyToken == null || a.PublicKeyToken.Length == 0 ) )
                {
                    argument.PublicKey = snk.PublicKey;
                }
            }
        }

        private static void UpdateAttributesInAssembly( StrongNameKeyPair snk, AssemblyDefinition assemby )
        {
            foreach( var type in assemby.MainModule.GetTypes().Where( t => t.HasCustomAttributes ) )
            {
                UpdateAttributesOnType( type, snk );
            }
        }
    }
}
