using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Common.Logging;
using Mono.Cecil;
using NullGuard;

namespace Sign.Core
{
    public class DefaultAssemblyInfoProvider : IAssemblyInfoProvider
    {
        private readonly static ILog Log = LogManager.GetCurrentClassLogger();
        private readonly string[] files;
        private readonly string[] dirs;
        private readonly string pattern;

        public DefaultAssemblyInfoProvider([AllowNull]string[] files, [AllowNull]string[] dirs, string pattern)
        {
            if (files == null && dirs == null)
            {
                throw new AppException("Provide directory or file name", ErrorCodes.ParserError);
            }

            this.files = files ?? new string[0];
            this.dirs = dirs ?? new string[0];
            this.pattern = pattern;
        }

        public IEnumerable<IAssemblyInfo> GetAssemblies()
        {
            var regex = new Regex(pattern);

            var paths = new List<string>(files);

            paths.AddRange(dirs.SelectMany(Directory.EnumerateFiles).Where(x => regex.IsMatch(x)));

            Log.Info("Assembly to analyze:");
            foreach (string path in paths)
            {
                Log.Info(path);
            }

            var assemblies = new List<IAssemblyInfo>();

            foreach (var path in paths)
            {
                try
                {
                    assemblies.Add(new AssemblyInfo(path,AssemblyDefinition.ReadAssembly(path)));
                }
                catch (Exception ex)
                {
                    throw new AppException(string.Format("Can not read assembly: {0}", path), ex, ErrorCodes.AssemblyReadError);
                }
            }

            return assemblies;
        }
    }
}
