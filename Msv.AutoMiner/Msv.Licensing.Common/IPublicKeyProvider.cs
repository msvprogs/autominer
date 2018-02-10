using System.Reflection;

namespace Msv.Licensing.Common
{
    public interface IPublicKeyProvider
    {
        [Obfuscation(Exclude = true)]
        dynamic Provide();
    }
}
