using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.DependencyModel;

namespace Msv.DependenciesJoiner
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Usage: Joiner.exe <deps.json_path> <target_library_name>");
                return;
            }

            var targetContext = new DirectoryInfo(args[0])
                .GetFiles("*.deps.json", SearchOption.TopDirectoryOnly)
                .Select(x =>
                {
                    using (var reader = new DependencyContextJsonReader())
                    using (var fileStream = x.OpenRead())
                        return reader.Read(fileStream);
                })
                .Aggregate((x, y) => x.Merge(y));

            if (targetContext == null)
            {
                Console.WriteLine("No .deps.json files found, exiting");
                return;
            }

            var targetFileName = Path.Combine(args[0], args[1] + ".deps.json");
            using (var targetStream = new FileStream(targetFileName, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                var writer = new DependencyContextWriter();
                writer.Write(targetContext, targetStream);
            }

            Console.WriteLine("Dependencies merged, target file: " + targetFileName);
        }
    }
}
