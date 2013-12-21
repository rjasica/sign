using System;
using System.Reflection;
using CommandLine;
using Common.Logging;
using Sign.Core;

namespace Sign
{
    internal class Program
    {
        private static ILog log;

        private static ILog Log
        {
            get { return log ?? (log = LogManager.GetCurrentClassLogger()); }
        }

        private static int Main(string[] args)
        {
            var options = new Options();
            if (Parser.Default.ParseArguments(args, options))
            {
                LogFactory.IsVerbose = options.Verbose;

                try
                {
                    StrongNameKeyPair snk = KeyUtil.ReadKey(options.Key);

                    IAssemblyInfoProvider assemblyInfoProvider = new DefaultAssemblyInfoProvider(
                        options.Files,
                        options.Dirs,
                        options.Pattern);
                    ISigner signer = new Signer();

                    signer.Sign(snk, assemblyInfoProvider);
                }
                catch (AppException ex)
                {
                    Log.Error(ex.Message);
                    Log.Info("Error occurs. Run sign -help to get more info.", ex);
                    return ex.ErrorCode;
                }
                catch (Exception ex)
                {
                    Log.Error("Internal error. Run with -verbose to see more details");
                    Log.Info(ex.Message, ex);
                    return ErrorCodes.InternalError;
                }

                return ErrorCodes.Ok;
            }
            return ErrorCodes.ParserError;
        }
    }
}