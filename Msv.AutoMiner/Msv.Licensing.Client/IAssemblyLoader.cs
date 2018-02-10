using System.IO;
using System.Reflection;

namespace Msv.Licensing.Client
{
    public interface IAssemblyLoader
    {
        [Obfuscation(Exclude = true)]
        dynamic Load(MemoryStream[] streams);
    }
}
