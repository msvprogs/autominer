using System;
using System.Runtime.Serialization;
using Msv.AutoMiner.Common.Data;
using Msv.AutoMiner.Common.Infrastructure;

namespace Msv.AutoMiner.Common.Licensing
{
    [DataContract(Name = nameof(LicenseData), Namespace = "")]
    public class LicenseData
    {
        [DataMember(Name = nameof(ApplicationName), Order = 0)]
        public string ApplicationName { get; set; }

        [DataMember(Name = nameof(HardwareId), Order = 1)]
        public string HardwareId { get; set; }

        [DataMember(Name = nameof(LicenseId), Order = 2)]
        public string LicenseId { get; set; }

        [DataMember(Name = nameof(Owner), Order = 3)]
        public string Owner { get; set; }

        [DataMember(Name = nameof(Issued), Order = 4)]
        public DateTime Issued { get; set; }

        [DataMember(Name = nameof(Expires), Order = 5)]
        public DateTime? Expires { get; set; }

        [DataMember(Name = nameof(SkipHardwareIdValidation), Order = 6)]
        public bool SkipHardwareIdValidation { get; set; }

        [IgnoreDataMember]
        public bool IsEmpty => LicenseId == null;

        public static ISerializer<LicenseData> Serializer 
            => new CorrectXmlSerializer<LicenseData>();

        public static LicenseData Current
            => Environment.GetEnvironmentVariable(Constants.LicenseEnvVariableName) != null
                ? Serializer.Deserialize(Environment.GetEnvironmentVariable(Constants.LicenseEnvVariableName))
                : new LicenseData();
    }
}