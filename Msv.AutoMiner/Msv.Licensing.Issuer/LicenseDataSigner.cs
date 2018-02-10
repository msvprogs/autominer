using System;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;
using Msv.Licensing.Common;

namespace Msv.Licensing.Issuer
{
    public class LicenseDataSigner
    {
        private const string PrivateKey =
            "BwIAAACkAABSU0EyAAQAAAEAAQDZdm20wFjURUZCUBa6ooSe8zbNqUG79tTyLdXQ5ElI1VGEPDPJrFInoRGzWM0jNdVhmx2Tr/O5wV7eBIx3o5K47EuSpqEGkOogM4kGQZXo41i44tjmeTdoylPFh/w0DuPcz/+1wXVXYHkIscyYJSnt/yvomHtsxG7gqU3+6Dbl0EMBj65H8LbGscSQoqkgxa4xeRUudYHqN0LLzRsp28CcwW9+39do/y1NFimXVJ8z1wBtosdMTOwOo8d+58pPPtezRzJq7L/TsdwuU92ctpAu2ut6ZAZFM/ZLMbpN4+ypIG/TS8om+nhYOUODaVCC/K4FvdNHK+1SB8we8JFPMHP4ZZFk2vIlsyAetjnrMbhLXW9FpR8KhCt+djjTCOuwlZ1nUR9VcBZy4Fnji4n6wgtxeNap0fyjTLBthn39U+CMveHpFkvXvz6Xu8/5JwBCI/9rTQgMM7JNd/Yai0Up95chP2A+CYz7OLvDcnQIvSVh7YLPgSV+YmbGEcDgzq/lDbFVgJoVS1zhNsozs8M1GE4vTmDzvtGo23+9Yfsvb6kEDDOQjfW0pGqtnml5vSPlv/KCvVsWxpejiWltQN4ujbw/6b1U28YOrY5qTnXF8QdOoy4SPMIJpIoamgQAd2iPHqqNAitzkzxsCaVnsWdab6OmGxwsDxZbyfdtO0gzuq9Z5SYSDan2YfPZwZvVPqf+9bOfyYmro71e79uEDSaOYb41MJ0o0bw3l3pB8nHrnjBj177OC1IGHoT3SDyV16i8HMY=";

        public byte[] Sign(LicenseData licenseData)
        {
            if (licenseData == null)
                throw new ArgumentNullException(nameof(licenseData));

            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.ImportCspBlob(Convert.FromBase64String(PrivateKey));

                var xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(LicenseData.Serializer.Serialize(licenseData));
                var declaration = xmlDocument.ChildNodes
                    .OfType<XmlNode>()
                    .FirstOrDefault(x => x.NodeType == XmlNodeType.XmlDeclaration);
                if (declaration != null)
                    xmlDocument.RemoveChild(declaration);

                var signedXml = new SignedXml(xmlDocument) { SigningKey = rsa };
                var reference = new Reference(string.Empty);
                reference.AddTransform(new XmlDsigEnvelopedSignatureTransform());
                signedXml.AddReference(reference);
                signedXml.ComputeSignature();

                if (xmlDocument.DocumentElement == null)
                    throw new InvalidOperationException("XML document element is null");
                xmlDocument.DocumentElement.AppendChild(xmlDocument.ImportNode(signedXml.GetXml(), true));
                return Encoding.UTF8.GetBytes(xmlDocument.InnerXml);
            }
        }
    }
}
