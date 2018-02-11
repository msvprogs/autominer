using System;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Extensions.DependencyModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Msv.DependenciesJoiner
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: Joiner.exe <deps.json_path1> ...<deps.json_pathn>  <target_library_name>");
                return;
            }

            var targetContext = args.Take(args.Length - 1)
                .Select(x => new DirectoryInfo(x))
                .SelectMany(x => x.GetFiles("*.deps.json", SearchOption.TopDirectoryOnly))
                .Select(x =>
                {
                    // Delete all encrypted libraries (Msv.*.dll) from .deps.json
                    using (var fileStream = x.OpenRead())
                    using (var reader = new StreamReader(fileStream))
                    {
                        var json = JsonConvert.DeserializeObject<JObject>(reader.ReadToEnd());
                        var msvLibs = ((JObject)json["libraries"])
                            .Properties()
                            .Where(y => y.Name.StartsWith("Msv."))
                            .ToArray();
                        foreach (var msvLib in msvLibs)
                            msvLib.Remove();
                        return new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(json)));
                    }
                })
                .Select(x =>
                {
                    using (var reader = new DependencyContextJsonReader())
                    using (x)
                        return reader.Read(x);
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
