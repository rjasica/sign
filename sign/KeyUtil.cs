using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Common.Logging;

namespace Sign
{
    internal static class KeyUtil
    {
        private readonly static ILog Log = LogManager.GetCurrentClassLogger();

        public static StrongNameKeyPair ReadKey(string key)
        {
            StrongNameKeyPair snk;
            try
            {
                Log.InfoFormat("Key file: {0}", key);
                snk = new StrongNameKeyPair(File.ReadAllBytes(key));
            }
            catch (Exception ex)
            {
                throw new AppException("Error during key reading", ex, ErrorCodes.KeyError);
            }
            return snk;
        }
    }
}
