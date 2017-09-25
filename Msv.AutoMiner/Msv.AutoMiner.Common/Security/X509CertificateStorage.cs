using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace Msv.AutoMiner.Common.Security
{
    public class X509CertificateStorage : ICertificateStorage
    {
        private readonly StoreLocation m_StoreLocation;
        private readonly X509Certificate2 m_RootCertificate;

        public X509CertificateStorage(StoreLocation storeLocation, X509Certificate2 rootCertificate)
        {
            m_StoreLocation = storeLocation;
            m_RootCertificate = rootCertificate ?? throw new ArgumentNullException(nameof(rootCertificate));
        }

        public void InstallRootCertificateIfNotExist()
        {
            using (var store = new X509Store(StoreName.Root, m_StoreLocation))
            {
                store.Open(OpenFlags.ReadOnly);
                if (store.Certificates.Contains(m_RootCertificate))
                    return;
                store.Open(OpenFlags.ReadWrite);
                store.Add(m_RootCertificate);
            }
        }

        public virtual void Store(X509Certificate2 certificate, StoreName storeName)
        {
            if (certificate == null)
                throw new ArgumentNullException(nameof(certificate));

            using (var store = new X509Store(storeName, m_StoreLocation))
            {
                store.Open(OpenFlags.ReadWrite);
                store.Add(certificate);
            }
        }

        public virtual X509Certificate2 FindByThumbprint(StoreName storeName, string thumbprint)
        {
            if (string.IsNullOrEmpty(thumbprint))
                throw new ArgumentException("Value cannot be null or empty.", nameof(thumbprint));

            using (var store = new X509Store(storeName, m_StoreLocation))
            {
                store.Open(OpenFlags.ReadOnly);
                return store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false)
                    .Cast<X509Certificate2>()
                    .FirstOrDefault();
            }
        }
    }
}
