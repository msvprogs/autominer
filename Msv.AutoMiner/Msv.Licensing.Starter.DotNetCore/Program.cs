﻿using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Msv.Licensing.Client;
using Msv.Licensing.Client.Data;

namespace Msv.Licensing.Starter.DotNetCore
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

            dynamic loader = new LicensedApplicationLoader(new AssemblyLoader(), new string[0]);

            ApplicationLoadResult result = loader.Load(
                Path.GetFileNameWithoutExtension(appFile.Name), LicenseFile.GetNameOfNewest(currentDirectory));
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
