using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Common.Logging;
using Mono.Cecil;

namespace Sign.Core.Updater
{
    public class UpdateInternalVisibleToAttribute : IUpdater
    {
        private const int ExpectedArgumentsNumber = 1;

        private static readonly string AttributeFullName = typeof( InternalsVisibleToAttribute ).FullName;

        private static ILog log;

        private static ILog Log
        {
            get { return log ?? ( log = LogManager.GetCurrentClassLogger() ); }
        }


        public void Update(
            StrongNameKeyPair snk,
            HashSet<IAssemblyInfo> notSigned,
            IEnumerable<IAssemblyInfo> allAssemblies )
        {
            foreach( var assemblyInfo in notSigned.ToArray() )
            {
                UpdateAssembly( notSigned, assemblyInfo );
            }
        }

        private static void UpdateAssembly( HashSet<IAssemblyInfo> modified, IAssemblyInfo assemblyInfo )
        {
            foreach( var info in GetInternalVisibleToAttributes( assemblyInfo ).ToArray() )
            {
                if( info.ConstructorArguments.Count != ExpectedArgumentsNumber )
                {
                    Log.WarnFormat(
                        "Constructor of InternalVisibleToAttribute has not expected number of arguments. Actual: {0}, expected: {1}.",
                        info.ConstructorArguments.Count,
                        ExpectedArgumentsNumber );
                    continue;
                }

                var argument = GetConstructorArgument( assemblyInfo, info );
                if( argument == null )
                {
                    continue;
                }

                var originalAssemblyName = ( string )argument.Value.Value;

                var signedAssembly = GetMatchingSignedAssembly( modified, originalAssemblyName );

                if( signedAssembly == null )
                {
                    Log.WarnFormat(
                        "InternalVisibleToAttribute for assembly '{0}' will be removed.",
                        originalAssemblyName );
                    assemblyInfo.Assembly.CustomAttributes.Remove( info );
                    continue;
                }

                var assemblyName = string.Format(
                    "{0}, PublicKey={1}",
                    signedAssembly.Assembly.Name.Name,
                    GetPublicKeyToken( signedAssembly ) );
                var newArgument = new CustomAttributeArgument( argument.Value.Type, assemblyName );

                info.ConstructorArguments.Clear();
                info.ConstructorArguments.Add( newArgument );
            }
        }

        private static string GetPublicKeyToken( IAssemblyInfo signedAssembly )
        {
            var publicKeyToken = signedAssembly.Assembly.Name.PublicKey.Select( x => x.ToString( "x2" ) )
                .Aggregate( ( x, y ) => x + y );
            return publicKeyToken;
        }

        private static IAssemblyInfo GetMatchingSignedAssembly(
            IEnumerable<IAssemblyInfo> modified,
            string originalAssemblyName )
        {
            var assemblyByName = modified.FirstOrDefault( s => s.Assembly.Name.Name == originalAssemblyName );
            if( assemblyByName == null )
            {
                Log.WarnFormat(
                    "Not found assembly '{0}' in signed assemblies. IternalVisibleToAttribute will be removed.",
                    originalAssemblyName );

                return null;
            }

            return assemblyByName;
        }

        private static CustomAttributeArgument? GetConstructorArgument(
            IAssemblyInfo assemblyInfo,
            CustomAttribute info )
        {
            var argumnet = info.ConstructorArguments.First();
            if( argumnet.Type != assemblyInfo.Assembly.MainModule.TypeSystem.String )
            {
                Log.WarnFormat(
                    "Argument type of constructor has invalid type> Expected string type but get '{0}.",
                    argumnet.Type.FullName );

                return null;
            }
            return argumnet;
        }

        private static IEnumerable<CustomAttribute> GetInternalVisibleToAttributes( IAssemblyInfo assemblyInfo )
        {
            var internalVisibleToAttributes = assemblyInfo.Assembly.CustomAttributes
                    .Where( t => t.AttributeType.FullName == AttributeFullName );

            return internalVisibleToAttributes;
        }
    }
}