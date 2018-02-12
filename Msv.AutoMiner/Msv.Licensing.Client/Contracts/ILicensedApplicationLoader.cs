using System.Reflection;
using Msv.Licensing.Client.Data;

namespace Msv.Licensing.Client.Contracts
{
    public interface ILicensedApplicationLoader
    {
        [Obfuscation(Exclude = true)]
        ApplicationLoadResult Load(string applicationName, string licenseFileName);
    }
}
