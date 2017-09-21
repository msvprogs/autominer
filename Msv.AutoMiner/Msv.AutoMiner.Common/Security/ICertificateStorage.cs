using System.Security.Cryptography.X509Certificates;

namespace Msv.AutoMiner.Common.Security
{
    public interface ICertificateStorage
    {
        void InstallRootCertificateIfNotExist();
        void Store(X509Certificate2 certificate, StoreName storeName);
        X509Certificate2 FindByThumbprint(StoreName storeName, string thumbprint);
    }
}
