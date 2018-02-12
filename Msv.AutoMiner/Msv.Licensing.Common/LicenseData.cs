using System;
using System.Xml.Serialization;

namespace Msv.Licensing.Common
{
    [XmlRoot(nameof(LicenseData))]
    public class LicenseData
    {
        [XmlElement(nameof(ApplicationName))]
        public string ApplicationName { get; set; }

        [XmlElement(nameof(HardwareId))]
        public string HardwareId { get; set; }

        [XmlElement(nameof(LicenseId))]
        public string LicenseId { get; set; }

        [XmlElement(nameof(Owner))]
        public string Owner { get; set; }

        [XmlElement(nameof(Issued))]
        public DateTime Issued { get; set; }

        [XmlElement(nameof(Expires))]
        public DateTime? Expires { get; set; }

        [XmlElement(nameof(SkipHardwareIdValidation))]
        public bool SkipHardwareIdValidation { get; set; }

        public static ISerializer<LicenseData> Serializer 
            => new CorrectXmlSerializer<LicenseData>();
    }
}
