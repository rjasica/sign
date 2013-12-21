using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Common.Logging;
using Common.Logging.Factory;
using Common.Logging.Simple;

namespace Sign
{
    public class LogFactory : AbstractCachingLoggerFactoryAdapter
    {
        public static bool IsVerbose { get; set; } 

        protected override ILog CreateLogger(string name)
        {
            return new ConsoleOutLogger(
                name, 
                IsVerbose ? LogLevel.All : LogLevel.Error,
                false, 
                false, 
                false, 
                CultureInfo.InvariantCulture.DateTimeFormat.FullDateTimePattern);
        }
    }
}
