using System;
using System.Security.Cryptography.X509Certificates;
using Msv.AutoMiner.Common.Security;
using Msv.AutoMiner.Rig.System;

namespace Msv.AutoMiner.Rig.Security
{
    public class X509CertificateStorageFactory : PlatformSpecificFactoryBase<ICertificateStorage>
    {
        private readonly StoreLocation m_StoreLocation;
        private readonly X509Certificate2 m_RootCertificate;

        public X509CertificateStorageFactory(StoreLocation storeLocation, X509Certificate2 rootCertificate)
        {
            m_StoreLocation = storeLocation;
            m_RootCertificate = rootCertificate ?? throw new ArgumentNullException(nameof(rootCertificate));
        }

        protected override ICertificateStorage CreateForUnix()
            => new MonoBuggedX509CertificateStorage(m_StoreLocation, m_RootCertificate);

        protected override ICertificateStorage CreateForWindows()
            => new X509CertificateStorage(m_StoreLocation, m_RootCertificate);
    }
}
