using System;
using System.IO;
using System.Text;
using Msv.AutoMiner.Service.System.Contracts;
using NLog;

namespace Msv.AutoMiner.Service.System.Unix
{
    public class SoftwareWatchdog : IWatchdog
    {
        private const string WatchDogFile = "/dev/watchdog";

        private static readonly ILogger M_Logger = LogManager.GetLogger("SoftwareWatchdog");
        private static readonly byte[] M_Food = Encoding.ASCII.GetBytes("food");
        private static readonly byte[] M_StopString = Encoding.ASCII.GetBytes("V");

        public void Feed()
        {
            try
            {
                if (WriteToWatchdog(M_Food))
                    M_Logger.Info("Fed the watchdog");
                else
                    M_Logger.Warn("Watchdog file not found, feeding failed");
            }
            catch (Exception ex)
            {
                M_Logger.Error(ex, "Watchdog feeding error");
            }
        }

        public void Dispose()
        {
            WriteToWatchdog(M_StopString);
            M_Logger.Info("Watchdog was disarmed");
        }

        private static bool WriteToWatchdog(byte[] bytes)
        {
            if (!File.Exists(WatchDogFile))
                return false;
            using (var fileStream = new FileStream(WatchDogFile, FileMode.Open, FileAccess.Write, FileShare.ReadWrite))
                fileStream.Write(bytes, 0, bytes.Length);
            return true;
        }
    }
}
