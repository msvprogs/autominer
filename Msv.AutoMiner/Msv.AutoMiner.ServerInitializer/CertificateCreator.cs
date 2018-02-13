using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.X509.Extension;
using X509Certificate = Org.BouncyCastle.X509.X509Certificate;

namespace Msv.AutoMiner.ServerInitializer
{
    public class CertificateCreator
    {        
        private const string KeyAlgorithm = "SHA512withRSA";
        private const int KeyStrength = 2048;

        private static readonly TimeSpan M_CertificateValidityPeriod = TimeSpan.FromDays(365 * 15);

        public X509Certificate2 CreateRoot(string commonName)
        {
            if (commonName == null) 
                throw new ArgumentNullException(nameof(commonName));

            return CreateCertificate(null, null, commonName);
        }

        public X509Certificate2 CreateDerived(X509Certificate2 parent, string commonName)
        {
            if (parent == null) 
                throw new ArgumentNullException(nameof(parent));
            if (commonName == null) 
                throw new ArgumentNullException(nameof(commonName));

            var parentCert = DotNetUtilities.FromX509Certificate(parent);
            var parentKeyPair = DotNetUtilities.GetKeyPair(parent.PrivateKey);

            return CreateCertificate(parentCert, parentKeyPair.Private, commonName);
        }

        private static X509Certificate2 CreateCertificate(
            X509Certificate parentCert, AsymmetricKeyParameter parentPrivateKey, string commonName)
        {
            var random = new SecureRandom(new CryptoApiRandomGenerator());
            var generator = new X509V3CertificateGenerator();
            generator.SetSerialNumber(BigIntegers.CreateRandomInRange(
                BigInteger.One, BigInteger.ValueOf(long.MaxValue), random));
            var subjectName = new X509Name(new[] {X509Name.CN}, new[] {commonName});
            generator.SetIssuerDN(parentCert?.IssuerDN ?? subjectName);
            generator.SetSubjectDN(subjectName);
            generator.SetNotBefore(DateTime.UtcNow);
            generator.SetNotAfter(DateTime.UtcNow + M_CertificateValidityPeriod);

            var keyPairGenerator = new RsaKeyPairGenerator();
            keyPairGenerator.Init(new KeyGenerationParameters(random, KeyStrength));

            var subjectKeyPair = keyPairGenerator.GenerateKeyPair();
            generator.SetPublicKey(subjectKeyPair.Public);
            generator.AddExtension(X509Extensions.SubjectKeyIdentifier, false,
                new SubjectKeyIdentifierStructure(subjectKeyPair.Public));
            if (parentCert != null)
                generator.AddExtension(X509Extensions.AuthorityKeyIdentifier, false,
                    new AuthorityKeyIdentifierStructure(parentCert));

            var certificate = generator.Generate(
                new Asn1SignatureFactory(KeyAlgorithm, parentPrivateKey ?? subjectKeyPair.Private));

            var store = new Pkcs12Store();
            var certificateEntry = new X509CertificateEntry(certificate);
            var friendlyName = certificate.SubjectDN.ToString();
            store.SetCertificateEntry(friendlyName, certificateEntry);
            store.SetKeyEntry(friendlyName, new AsymmetricKeyEntry(subjectKeyPair.Private), new[] {certificateEntry});
            using (var stream = new MemoryStream())
            {
                const string tempPassword = "123456";
                store.Save(stream, tempPassword.ToCharArray(), random);
                return new X509Certificate2(
                    stream.ToArray(), tempPassword, X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet);
            }
        }
    }
}