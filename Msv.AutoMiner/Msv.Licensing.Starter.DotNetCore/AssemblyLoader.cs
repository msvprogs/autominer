using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Msv.Licensing.Client;

namespace Msv.Licensing.Starter.DotNetCore
{
    public class AssemblyLoader : IAssemblyLoader
    {
        [Obfuscation(Exclude = true)]
        public dynamic Load(MemoryStream[] streams)
        {
            var assemblies = streams
                .Cast<dynamic>()
                .Select(x =>  AssemblyLoadContext.Default.LoadFromStream(x))
                .ToArray();

            AssemblyLoadContext.Default.Resolving += OnAssemblyResolve;

            return assemblies.Single(x => x.EntryPoint != null).EntryPoint;

            Assembly OnAssemblyResolve(AssemblyLoadContext assemblyLoadContext, AssemblyName assemblyName)
            {
                Console.WriteLine(assemblyName.Name);
                return Array.Find(assemblies, x => x.GetName().FullName == assemblyName.ToString())
                       /*?? (File.Exists($"{assemblyName.Name}.dll") ? Assembly.LoadFrom($"{assemblyName.Name}.dll") : null)*/;
            }
        }
    }
}