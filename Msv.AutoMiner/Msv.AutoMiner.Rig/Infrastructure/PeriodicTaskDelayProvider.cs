using System;
using Msv.AutoMiner.Rig.Infrastructure.Contracts;
using Msv.AutoMiner.Rig.Security;

namespace Msv.AutoMiner.Rig.Infrastructure
{
    public class PeriodicTaskDelayProvider : IPeriodicTaskDelayProvider
    {
        private const int MinDelaySecs = 0;
        private const int MaxDelaySecs = 30;

        private readonly IClientCertificateProvider m_CertificateProvider;

        public PeriodicTaskDelayProvider(IClientCertificateProvider certificateProvider)
        {
            m_CertificateProvider = certificateProvider ?? throw new ArgumentNullException(nameof(certificateProvider));
        }

        public TimeSpan GetDelay<T>()
        {
            var certificate = m_CertificateProvider.GetCertificate();
            var seed = certificate != null
                ? BitConverter.ToInt32(certificate.GetSerialNumber(), 0)
                : 0;
            var random = new Random(seed ^ typeof(T).GetHashCode());
            return TimeSpan.FromSeconds(random.Next(MinDelaySecs, MaxDelaySecs));
        }
    }
}
