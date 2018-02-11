using System;
using System.IO;
using NLog;

namespace Msv.AutoMiner.Rig.System.Unix
{
    public class FileReader
    {
        private static readonly ILogger M_Log = LogManager.GetCurrentClassLogger();

        public string ReadContents(string path)
        {
            if (!File.Exists(path))
            {
                M_Log.Warn($"File {path} doesn't exist");
                return string.Empty;
            }
            try
            {
                return File.ReadAllText(path);
            }
            catch (Exception ex)
            {
                M_Log.Error(ex);
                return string.Empty;
            }
        }

        public string[] ReadLines(string path)
        {
            if (!File.Exists(path))
            {
                M_Log.Warn($"File {path} doesn't exist");
                return new string[0];
            }
            try
            {
                return File.ReadAllLines(path);
            }
            catch (Exception ex)
            {
                M_Log.Error(ex);
                return new string[0];
            }
        }
    }
}
