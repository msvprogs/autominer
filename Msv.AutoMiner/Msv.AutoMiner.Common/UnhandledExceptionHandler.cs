using System;
using System.Threading.Tasks;
using NLog;

namespace Msv.AutoMiner.Common
{
    public static class UnhandledExceptionHandler
    {
        public static void RegisterLogger(ILogger logger)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            AppDomain.CurrentDomain.UnhandledException +=
                (s, e) => logger.Fatal(e.ExceptionObject as Exception, "Unhandled exception");
            TaskScheduler.UnobservedTaskException +=
                (s, e) => logger.Fatal(e.Exception, "Unobserved exception");
        }
    }
}
