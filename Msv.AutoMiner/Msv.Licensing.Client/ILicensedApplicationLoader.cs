namespace Msv.Licensing.Client
{
    public interface ILicensedApplicationLoader
    {
        ApplicationLoadResult Load(string applicationName, string licenseFileName);
    }
}
