using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Xml;
using Msv.Licensing.Common;

namespace Msv.Licensing.Client
{
    internal class LicenseVerifier : ILicenseVerifier
    {
        private readonly IEncryptionKeyDeriver m_Deriver;
        private readonly IPublicKeyProvider m_PublicKeyProvider;

        public LicenseVerifier(IEncryptionKeyDeriver deriver, IPublicKeyProvider publicKeyProvider)
        {
            m_Deriver = deriver;
            m_PublicKeyProvider = publicKeyProvider;
        }

        [Obfuscation(Exclude = true)]
        public dynamic VerifyAndDerive(dynamic appName, dynamic filename)
        {
            Verify(appName, filename);
            return m_Deriver.Derive(m_PublicKeyProvider.Provide());
        }

        public void Verify(dynamic appName, dynamic filename)
        {
            dynamic xmlDocument = new XmlDocument();
            using (dynamic fileReader = new StreamReader(filename))
            using (dynamic reader = new XmlTextReader(fileReader))
                xmlDocument.Load(reader);

            var signatureNodes = xmlDocument.GetElementsByTagName("Signature");
            if (signatureNodes.Count != 1)
                throw new LicenseCorruptException();

            dynamic signedXml = new SignedXml(xmlDocument);
            signedXml.LoadXml(signatureNodes.Item(0));

            using (dynamic rsa = new RSACryptoServiceProvider())
            {
                rsa.ImportCspBlob(m_PublicKeyProvider.Provide());
                if (!signedXml.CheckSignature(rsa))
                    throw new LicenseCorruptException();
            }

            var licenseData = LicenseData.Serializer.Deserialize((string)xmlDocument.InnerXml);
            if (licenseData.ApplicationName != appName)
                throw new LicenseIsForDifferentApplicationException();

            var now = ((dynamic)typeof(DateTime))
                .GetProperty(nameof(DateTime.Now), BindingFlags.Static | BindingFlags.Public)
                .GetGetMethod()
                .Invoke(null, null);

            if (licenseData.Expires != null && licenseData.Expires < now)
                throw new LicenseExpiredException();
        }
    }
}
