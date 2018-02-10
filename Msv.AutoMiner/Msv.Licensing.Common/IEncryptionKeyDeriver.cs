using System.Reflection;

namespace Msv.Licensing.Common
{
    public interface IEncryptionKeyDeriver
    {
        [Obfuscation(Exclude = true)]
        dynamic Derive(dynamic publicKey);
    }
}
