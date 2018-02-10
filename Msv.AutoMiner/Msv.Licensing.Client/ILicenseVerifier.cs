using System.Reflection;

namespace Msv.Licensing.Client
{
    internal interface ILicenseVerifier
    {
        [Obfuscation(Exclude = true)]
        dynamic Verify(string appName, string filename);
    }
}