using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Mono.Cecil;

using ICustomAttributeProvider = Mono.Cecil.ICustomAttributeProvider;

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

        private static void UpdateAttributesInAssembly( StrongNameKeyPair snk, AssemblyDefinition assemby )
        {
            foreach( var type in assemby.MainModule.GetTypes() )
            {
                IEnumerable<ICustomAttributeProvider> types = new[] { type };
                IEnumerable<ICustomAttributeProvider> fields = type.Fields;
                IEnumerable<ICustomAttributeProvider> properties = type.Properties;
                IEnumerable<ICustomAttributeProvider> events = type.Events;
                IEnumerable<ICustomAttributeProvider> methods = type.Methods;

                foreach( var attrProvider 
                    in types.Concat( fields ).Concat( properties ).Concat( events ).Concat( methods ) )
                {
                    UpdateAttributesOnType( attrProvider, snk );
                }
            }
        }

        private static void UpdateAttributesOnType( ICustomAttributeProvider type, StrongNameKeyPair snk )
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
    }
}
