using System;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Pkcs;

namespace Msv.AutoMiner.Rig.Data
{
    public class CertificateRequestWithKeys
    {
        public Pkcs10CertificationRequest CertificationRequest { get; }
        public AsymmetricCipherKeyPair KeyPair { get; }

        public CertificateRequestWithKeys(Pkcs10CertificationRequest certificationRequest, AsymmetricCipherKeyPair keyPair)
        {
            CertificationRequest = certificationRequest ?? throw new ArgumentNullException(nameof(certificationRequest));
            KeyPair = keyPair ?? throw new ArgumentNullException(nameof(keyPair));
        }
    }
}
