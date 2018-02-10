using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Msv.Licensing.Client;

namespace Msv.Licensing.Starter.DotNet
{
    public class AssemblyLoader : IAssemblyLoader
    {
        [Obfuscation(Exclude = true)]
        public dynamic Load(MemoryStream[] streams)
        {
            var assemblies = streams
                .Cast<dynamic>()
                .Select(x => Assembly.Load(x.ToArray()))
                .ToArray();
            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;

            return assemblies.Single(x => x.EntryPoint != null).EntryPoint;

            Assembly OnAssemblyResolve(object sender, dynamic args) 
                => Array.Find(assemblies, x => x.GetName().FullName == args.Name);
        }
    }
}