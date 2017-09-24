using System.Security.Cryptography.X509Certificates;
using Msv.AutoMiner.Rig.Data;
using Org.BouncyCastle.Crypto;

namespace Msv.AutoMiner.Rig.Security
{
    public interface IClientCertificateProvider
    {
        X509Certificate2 GetCertificate();
        CertificateRequestWithKeys CreateNewKeys(string commonName);
        void StoreCaCertificate(X509Certificate2 certificate);
        void StoreClientCertificate(X509Certificate2 certificate, AsymmetricCipherKeyPair keyPair);
    }
}
