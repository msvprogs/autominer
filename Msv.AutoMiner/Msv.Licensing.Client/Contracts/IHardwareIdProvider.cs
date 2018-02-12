using System.Reflection;

namespace Msv.Licensing.Client.Contracts
{
    internal interface IHardwareIdProvider
    {
        [Obfuscation(Exclude = true)]
        dynamic GetHardwareId();
    }
}
