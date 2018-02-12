using System;
using System.Runtime.Serialization;
using Msv.AutoMiner.Common.Data;
using Msv.AutoMiner.Common.Infrastructure;

namespace Msv.AutoMiner.Common.Licensing
{
    [DataContract(Namespace = "")]
    public class LicenseData
    {
        [DataMember(Order = 0)]
        public string ApplicationName { get; set; }

        [DataMember(Order = 1)]
        public string HardwareId { get; set; }

        [DataMember(Order = 2)]
        public string LicenseId { get; set; }

        [DataMember(Order = 3)]
        public string Owner { get; set; }

        [DataMember(Order = 4)]
        public DateTime Issued { get; set; }

        [DataMember(Order = 5)]
        public DateTime? Expires { get; set; }

        [DataMember(Order = 6)]
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