using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Common.Logging;
using Mono.Cecil;
using NullGuard;

namespace Sign.Core
{
    public class DefaultAssemblyInfoProvider : IAssemblyInfoProvider
    {
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();
        private readonly string[] files;
        private readonly string[] dirs;
        private readonly string pattern;
        private readonly ReaderParameters readerParameters;

        public DefaultAssemblyInfoProvider( [AllowNull]string[] files, [AllowNull]string[] dirs, string pattern )
        {
            if( files == null && dirs == null )
            {
                throw new AppException( "Provide directory or file name", ErrorCodes.ParserError );
            }

            this.files = files ?? new string[ 0 ];
            this.dirs = dirs ?? new string[ 0 ];
            this.pattern = pattern;

            this.readerParameters = CreateReaderParameters( this.dirs );
        }

        public IEnumerable<IAssemblyInfo> GetAssemblies()
        {
            var regex = new Regex( this.pattern );

            var paths = new List<string>( this.files );

            paths.AddRange( this.dirs.SelectMany( Directory.EnumerateFiles ).Where( x => regex.IsMatch( x ) ) );

            Log.Info( "Assembly to analyze:" );
            foreach( var path in paths )
            {
                Log.Info( path );
            }

            var assemblies = new List<IAssemblyInfo>();

            foreach( var path in paths )
            {
                try
                {
                    assemblies.Add( new AssemblyInfo( path, AssemblyDefinition.ReadAssembly( path, this.readerParameters ) ) );
                }
                catch( Exception ex )
                {
                    throw new AppException( string.Format( "Can not read assembly: {0}", path ), ex, ErrorCodes.AssemblyReadError );
                }
            }

            return assemblies;
        }

        private static ReaderParameters CreateReaderParameters( IEnumerable<string> dirs )
        {
            var resolver = new DefaultAssemblyResolver();
            foreach( var dir in dirs )
            {
                resolver.AddSearchDirectory( dir );
            }

            return new ReaderParameters
            {
                AssemblyResolver = resolver,
            };
        }
    }
}
