using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Msv.AutoMiner.Common.Helpers;
using Msv.AutoMiner.ControlCenterService.Security.Contracts;
using Msv.AutoMiner.ControlCenterService.Storage.Contracts;
using Msv.AutoMiner.Data;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.X509.Extension;

namespace Msv.AutoMiner.ControlCenterService.Security
{
    public class CertificateService : ICertificateService
    {
        private static readonly TimeSpan M_CertificateValidityPeriod = TimeSpan.FromDays(365 * 5);

        private readonly ICertificateServiceStorage m_Storage;

        public CertificateService(ICertificateServiceStorage storage)
        {
            m_Storage = storage ?? throw new ArgumentNullException(nameof(storage));
        }

        public Task<X509Certificate2> CreateCertificate(Rig rig, X509Certificate2 serverCertificate, byte[] certificateRequest)
        {
            if (rig == null)
                throw new ArgumentNullException(nameof(rig));
            if (serverCertificate == null)
                throw new ArgumentNullException(nameof(serverCertificate));
            if (certificateRequest == null)
                throw new ArgumentNullException(nameof(certificateRequest));

            var request = new Pkcs10CertificationRequest(certificateRequest);
            if (!request.Verify())
                return null;

            var requestInfo = request.GetCertificationRequestInfo();
            var cnList = requestInfo.Subject.GetValueList(X509Name.CN);
            if (cnList.Count != 1 || cnList.Cast<string>().Single() != rig.Name)
                return null;

            var generator = new X509V3CertificateGenerator();
            generator.SetSerialNumber(new BigInteger(128, new SecureRandom()));
            generator.SetIssuerDN(new X509Name(serverCertificate.SubjectName.Format(false)));
            generator.SetSubjectDN(requestInfo.Subject);
            generator.SetNotBefore(DateTime.UtcNow);
            generator.SetNotAfter(DateTime.UtcNow + M_CertificateValidityPeriod);
            var clientPublicKey = PublicKeyFactory.CreateKey(requestInfo.SubjectPublicKeyInfo);
            generator.SetPublicKey(clientPublicKey);
            generator.AddExtension(X509Extensions.AuthorityKeyIdentifier, false,
                new AuthorityKeyIdentifierStructure(DotNetUtilities.FromX509Certificate(serverCertificate)));
            generator.AddExtension(X509Extensions.SubjectKeyIdentifier, false,
                new SubjectKeyIdentifierStructure(clientPublicKey));

            var caKeyPair = DotNetUtilities.GetKeyPair(serverCertificate.PrivateKey);
            var bouncyCert = generator.Generate(
                new Asn1SignatureFactory(request.SignatureAlgorithm.Algorithm.Id, caKeyPair.Private));
            return Task.FromResult(new X509Certificate2(DotNetUtilities.ToX509Certificate(bouncyCert)));
        }

        public async Task<Rig> AuthenticateRig(X509Certificate2 serverCertificate, X509Certificate2 clientCertificate)
        {
            if (serverCertificate == null)
                throw new ArgumentNullException(nameof(serverCertificate));
            if (clientCertificate == null)
                throw new ArgumentNullException(nameof(clientCertificate));

            if (clientCertificate.IssuerName.Name != serverCertificate.SubjectName.Name)
                return null;

            var rig = await m_Storage.GetRigByName(clientCertificate.GetNameInfo(X509NameType.SimpleName, false));
            if (rig?.ClientCertificateSerial == null
                || !rig.ClientCertificateSerial.SequenceEqual(clientCertificate.GetSerialNumber()))
                return null;
            if (rig.ClientCertificateThumbprint == null
                || !rig.ClientCertificateThumbprint.SequenceEqual(HexHelper.FromHex(clientCertificate.Thumbprint)))
                return null;

            return rig;
        }
    }
}
