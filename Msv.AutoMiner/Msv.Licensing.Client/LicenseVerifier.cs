using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Xml;
using Msv.Licensing.Client.Contracts;
using Msv.Licensing.Common;
using NLog;

namespace Msv.Licensing.Client
{
    internal class LicenseVerifier : ILicenseVerifier
    {
        private static readonly ILogger M_Logger = LogManager.GetCurrentClassLogger();

        private readonly dynamic m_Deriver;
        private readonly dynamic m_PublicKeyProvider;
        private readonly dynamic m_HardwareIdProvider;

        public LicenseVerifier(
            IEncryptionKeyDeriver deriver, 
            IPublicKeyProvider publicKeyProvider,
            IHardwareIdProvider hardwareIdProvider)
        {
            m_Deriver = deriver;
            m_PublicKeyProvider = publicKeyProvider;
            m_HardwareIdProvider = hardwareIdProvider;
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
            {
                M_Logger.Warn("License is corrupt! Invalid license file");
                throw new LicenseCorruptException();
            }

            dynamic signedXml = new SignedXml(xmlDocument);
            signedXml.LoadXml(signatureNodes.Item(0));

            using (dynamic rsa = new RSACryptoServiceProvider())
            {
                rsa.ImportCspBlob(m_PublicKeyProvider.Provide());
                if (!signedXml.CheckSignature(rsa))
                {
                    M_Logger.Warn("License is corrupt! Invalid license file");
                    throw new LicenseCorruptException();
                }
            }

            dynamic licenseData = LicenseData.Serializer.Deserialize((string)xmlDocument.InnerXml);
            if (licenseData.ApplicationName != appName)
            {
                M_Logger.Warn("License file cannot be used, it has been issued for the other application: " + licenseData.ApplicationName);
                throw new LicenseIsForDifferentApplicationException();
            }

            var hardwareId = m_HardwareIdProvider.GetHardwareId();
            if (!licenseData.SkipHardwareIdValidation
                && licenseData.HardwareId != hardwareId)
            {
                M_Logger.Warn("Hardware ID is different, please update the license");
                throw new LicenseCorruptException();
            }

            var now = ((dynamic)typeof(DateTime))
                .GetProperty(nameof(DateTime.UtcNow), BindingFlags.Static | BindingFlags.Public)
                .GetGetMethod()
                .Invoke(null, null);

            if (licenseData.Expires != null && licenseData.Expires < now)
            {
                M_Logger.Warn($"License expired on {licenseData.Expires.Value.ToLongDateString()} GMT");
                throw new LicenseExpiredException();
            }

            ((dynamic)typeof(Environment))
                .GetMethod(nameof(Environment.SetEnvironmentVariable),
                    BindingFlags.Static | BindingFlags.Public, 
                    null, 
                    new[] {typeof(string), typeof(string)}, 
                    null)
                .Invoke(null, new object[] {"MSVAUTOMINER_LICENSE_QJCBLF", licenseData});

            M_Logger.Info($"License verified. {licenseData.ApplicationName}, licensed to {licenseData.Owner}, " 
                          + $"expires on {(licenseData.Expires != null ? licenseData.Expires.Value.ToLongDateString() + " GMT" : "<never>")}");
        }
    }
}
