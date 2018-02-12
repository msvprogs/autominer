using Msv.Licensing.Client.Data;

namespace Msv.Licensing.Client.Contracts
{
    public interface ILicensedApplicationLoader
    {
        ApplicationLoadResult Load(string applicationName, string licenseFileName);
    }
}
