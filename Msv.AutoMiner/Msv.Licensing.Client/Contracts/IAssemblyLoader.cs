using System.IO;
using System.Reflection;

namespace Msv.Licensing.Client.Contracts
{
    public interface IAssemblyLoader
    {
        [Obfuscation(Exclude = true)]
        dynamic Load(MemoryStream[] streams);

        [Obfuscation(Exclude = true)]
        dynamic CreateResolver(dynamic assemblies);
    }
}
