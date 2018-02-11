using System.Reflection;

namespace Msv.Licensing.Client
{
    internal interface ILicenseVerifier
    {
        [Obfuscation(Exclude = true)]
        dynamic VerifyAndDerive(dynamic appName, dynamic filename);

        [Obfuscation(Exclude = true)]
        void Verify(dynamic appName, dynamic filename);
    }
}