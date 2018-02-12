using System.IO;
using System.Linq;
using System.Reflection;

namespace Msv.Licensing.Client
{
    public static class LicenseFile
    {
        public static string GetNameOfNewest()
            => new DirectoryInfo(
                    Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)
                    ?? Directory.GetCurrentDirectory())
                .GetFiles("*.msvlic")
                .OrderByDescending(x => x.CreationTime)
                .FirstOrDefault()
                ?.FullName;
    }
}