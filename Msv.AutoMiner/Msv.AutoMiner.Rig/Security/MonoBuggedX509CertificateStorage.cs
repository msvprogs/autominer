using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Msv.AutoMiner.Common.Security;

namespace Msv.AutoMiner.Rig.Security
{
    //Mono's X509Store implementation... can't store private keys... :(
    public class MonoBuggedX509CertificateStorage : X509CertificateStorage
    {
        private const string PrivateKeyStoragePath = "certs";
        private const int AesKeySize = 256;
        private static readonly Encoding M_Encoding = Encoding.UTF8;

        public MonoBuggedX509CertificateStorage(StoreLocation storeLocation, X509Certificate2 rootCertificate)
            : base(storeLocation, rootCertificate)
        {
            if (!Directory.Exists(PrivateKeyStoragePath))
                Directory.CreateDirectory(PrivateKeyStoragePath);
        }

        public override void Store(X509Certificate2 certificate, StoreName storeName)
        {
            base.Store(certificate, storeName);
            if (!certificate.HasPrivateKey)
                return;
            using (var aes = CreateAesCryptoServiceProvider(certificate))
            using (var encryptor = aes.CreateEncryptor())
            {
                var key = M_Encoding.GetBytes(certificate.PrivateKey.ToXmlString(true));
                File.WriteAllBytes(GetPrivateKeyPath(certificate), encryptor.TransformFinalBlock(key, 0, key.Length));
            }
        }

        public override X509Certificate2 FindByThumbprint(StoreName storeName, string thumbprint)
        {
            var certificate = base.FindByThumbprint(storeName, thumbprint);
            if (certificate == null)
                return null;
            if (!File.Exists(GetPrivateKeyPath(certificate)))
                return certificate;
            var rsa = RSA.Create();
            using (var aes = CreateAesCryptoServiceProvider(certificate))
            using (var decryptor = aes.CreateDecryptor())
            {
                var keyBytes = File.ReadAllBytes(GetPrivateKeyPath(certificate));
                rsa.FromXmlString(M_Encoding.GetString(decryptor.TransformFinalBlock(keyBytes, 0, keyBytes.Length)));
            }
            certificate.PrivateKey = rsa;
            return certificate;
        }

        private static string GetPrivateKeyPath(X509Certificate2 certificate)
            => Path.Combine(PrivateKeyStoragePath, certificate.Thumbprint + ".bin");

        private static AesCryptoServiceProvider CreateAesCryptoServiceProvider(X509Certificate2 certificate)
        {
            using (var deriveKey = new Rfc2898DeriveBytes(
                certificate.Thumbprint + certificate.IssuerName.Name + certificate.Thumbprint,
                M_Encoding.GetBytes("2xgin2motvbtvjsfdnv.,zsef  ksv")))
            {
                var aes = new AesCryptoServiceProvider
                {
                    KeySize = AesKeySize,
                    Key = deriveKey.GetBytes(AesKeySize / 8),
                    Mode = CipherMode.CBC
                };
                aes.IV = deriveKey.GetBytes(aes.BlockSize / 8);
                return aes;
            }
        }
    }
}
