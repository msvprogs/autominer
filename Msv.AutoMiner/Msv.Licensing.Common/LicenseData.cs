using System;
using System.Runtime.Serialization;

namespace Msv.Licensing.Common
{
    [DataContract(Name = nameof(LicenseData), Namespace = "")]
    public class LicenseData
    {
        [DataMember(Name = nameof(ApplicationName))]
        public string ApplicationName { get; set; }

        [DataMember(Name = nameof(HardwareId))]
        public string HardwareId { get; set; }

        [DataMember(Name = nameof(LicenseId))]
        public string LicenseId { get; set; }

        [DataMember(Name = nameof(Owner))]
        public string Owner { get; set; }

        [DataMember(Name = nameof(Issued))]
        public DateTime Issued { get; set; }

        [DataMember(Name = nameof(Expires))]
        public DateTime? Expires { get; set; }

        [DataMember(Name = nameof(SkipHardwareIdValidation))]
        public bool SkipHardwareIdValidation { get; set; }

        public static ISerializer<LicenseData> Serializer 
            => new CorrectXmlSerializer<LicenseData>();
    }
}
