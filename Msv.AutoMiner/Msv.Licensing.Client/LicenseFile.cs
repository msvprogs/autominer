using System.IO;
using System.Linq;

namespace Msv.Licensing.Client
{
    public static class LicenseFile
    {
        public static string GetNameOfNewest(string path)
            => new DirectoryInfo(path)
                .GetFiles("*.msvlic")
                .OrderByDescending(x => x.CreationTime)
                .FirstOrDefault()
                ?.FullName;
    }
}