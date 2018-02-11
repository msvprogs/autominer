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
                    using (var reader = new DependencyContextJsonReader())
                    using (var depsJsonStream = x.OpenRead())
                        return reader.Read(depsJsonStream);
                })
                .Aggregate((x, y) => x.Merge(y));

            if (targetContext == null)
            {
                Console.WriteLine("No .deps.json files found, exiting");
                return;
            }

            var targetFileName = Path.Combine(args[0], args[1] + ".deps.json");
            using (var targetMemoryStream = new MemoryStream())
            {
                var writer = new DependencyContextWriter();
                writer.Write(targetContext, targetMemoryStream);
                var json = JsonConvert.DeserializeObject<JObject>(
                    Encoding.UTF8.GetString(targetMemoryStream.ToArray()));
                ((JObject)json["libraries"])
                    .Properties()
                    .Where(y => y.Name.StartsWith("Msv."))
                    .ToList()
                    .ForEach(y => y.Remove());
                File.WriteAllText(targetFileName, JsonConvert.SerializeObject(json));
            }

            Console.WriteLine("Dependencies merged, target file: " + targetFileName);
        }
    }
}
