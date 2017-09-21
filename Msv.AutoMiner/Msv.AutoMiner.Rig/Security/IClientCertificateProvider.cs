using System.Security.Cryptography.X509Certificates;
using Org.BouncyCastle.Pkcs;

namespace Msv.AutoMiner.Rig.Security
{
    public interface IClientCertificateProvider
    {
        X509Certificate2 GetCertificate();
        Pkcs10CertificationRequest CreateNewRequest(string commonName);
        void StoreNewCertificate(X509Certificate2 certificate);
    }
}
