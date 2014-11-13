﻿using System;
using Mono.Cecil;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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

        private void UpdateAttributesInAssembly( StrongNameKeyPair snk, AssemblyDefinition assemby )
        {
            foreach( var type in assemby.MainModule.GetTypes().Where( t => t.HasCustomAttributes ) )
            {
                this.UpdateAttributesOnType( type, snk );
            }
        }

        private void UpdateAttributesOnType( TypeDefinition type, StrongNameKeyPair snk )
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
