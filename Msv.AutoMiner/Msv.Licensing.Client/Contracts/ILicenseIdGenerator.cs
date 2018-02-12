using System.Reflection;

namespace Msv.Licensing.Client.Contracts
{
    internal interface ILicenseIdGenerator
    {
        [Obfuscation(Exclude = true)]
        string Generate();
    }
}
