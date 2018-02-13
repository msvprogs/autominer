using System;
using System.Security.Cryptography.X509Certificates;
using Msv.AutoMiner.Common.Security;
using Msv.AutoMiner.Rig.Data;
using Msv.AutoMiner.Rig.Storage.Contracts;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;

namespace Msv.AutoMiner.Rig.Security
{
    public class ClientCertificateProvider : IClientCertificateProvider
    {
        private const string KeyAlgorithm = "SHA512withRSA";
        private const int KeyStrength = 2048;
        private const StoreName ClientCertificateStore = StoreName.My;

        private readonly ICertificateStorage m_Storage;
        private readonly IStoredSettings m_Settings;

        public ClientCertificateProvider(ICertificateStorage storage, IStoredSettings settings)
        {
            m_Storage = storage ?? throw new ArgumentNullException(nameof(storage));
            m_Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public X509Certificate2 GetCertificate()
        {
            var thumbprint = m_Settings.ClientCertificateThumbprint;
            return string.IsNullOrWhiteSpace(thumbprint)
                ? null
                : m_Storage.FindByThumbprint(ClientCertificateStore, thumbprint);
        }

        public CertificateRequestWithKeys CreateNewKeys(string commonName)
        {
            if (string.IsNullOrEmpty(commonName))
                throw new ArgumentException("Value cannot be null or empty.", nameof(commonName));

            var keyGenerator = new RsaKeyPairGenerator();
            keyGenerator.Init(new KeyGenerationParameters(new SecureRandom(), KeyStrength));
            var keyPair = keyGenerator.GenerateKeyPair();
            var request = new Pkcs10CertificationRequest(
                new Asn1SignatureFactory(KeyAlgorithm, keyPair.Private),
                new X509Name(new[] {X509Name.CN}, new[] {commonName}),
                keyPair.Public,
                null,
                keyPair.Private);
            return new CertificateRequestWithKeys(request, keyPair);
        }

        public void StoreCaCertificate(X509Certificate2 certificate)
        {
            if (certificate == null)
                throw new ArgumentNullException(nameof(certificate));

            m_Storage.Store(certificate, StoreName.Root);
        }

        public void StoreClientCertificate(X509Certificate2 certificate, AsymmetricCipherKeyPair keyPair)
        {
            if (certificate == null)
                throw new ArgumentNullException(nameof(certificate));
            if (keyPair == null)
                throw new ArgumentNullException(nameof(keyPair));

            certificate.PrivateKey = DotNetUtilities.ToRSA((RsaPrivateCrtKeyParameters)keyPair.Private);
            m_Storage.Store(certificate, ClientCertificateStore);
            m_Settings.ClientCertificateThumbprint = certificate.Thumbprint;
        }
    }
}
