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
            => streams
                .Cast<dynamic>()
                .Select(x => Assembly.Load(x.ToArray()))
                .ToArray();

        [Obfuscation(Exclude = true)]
        public dynamic CreateResolver(dynamic assemblies)
            => new AssemblyResolver(assemblies);
    }
}