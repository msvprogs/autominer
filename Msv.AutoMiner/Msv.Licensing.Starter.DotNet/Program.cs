using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Msv.Licensing.Client;

namespace Msv.Licensing.Starter.DotNet
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var currentDirectory =
#if DEBUG
                args.ElementAtOrDefault(0) ??
#endif
                Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)
                ?? Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(currentDirectory);

            var appFile = new DirectoryInfo(currentDirectory)
                .GetFiles("*.msvenc")
                .FirstOrDefault();
            if (appFile == null)
            {
                Console.WriteLine("No suitable application for run, press any key to exit...");
                Console.ReadKey();
                return;
            }

            var loader = new LicensedApplicationLoader(new AssemblyLoader(),
            #if DEBUG
                new string[0]
            #else 
                args
            #endif
            );

            var result = loader.Load(Path.GetFileNameWithoutExtension(appFile.Name), "license.dat");
            if (result.Status == ApplicationLoadStatus.Success)
                return;
            
            Console.WriteLine(result.Message);
            if (result.Error != null)
                Console.WriteLine(result.Error);
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();           
        }
    }
}
