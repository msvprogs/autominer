using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Msv.Licensing.Client.Contracts;

namespace Msv.Licensing.Starter.DotNetCore
{
    public class AssemblyLoader : IAssemblyLoader
    {
        [Obfuscation(Exclude = true)]
        public dynamic Load(MemoryStream[] streams)
            => streams
                .Cast<dynamic>()
                .Select(x => AssemblyLoadContext.Default.LoadFromStream(x))
                .ToArray();

        [Obfuscation(Exclude = true)]
        public dynamic CreateResolver(dynamic assemblies)
            => new AssemblyResolver(assemblies);
    }
}