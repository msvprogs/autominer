using System;
using System.Xml.Serialization;
using Msv.AutoMiner.Common.Data;
using Msv.AutoMiner.Common.Infrastructure;

namespace Msv.AutoMiner.Common.Licensing
{
    public class LicenseData
    {
        public string ApplicationName { get; set; }
        public string HardwareId { get; set; }
        public string LicenseId { get; set; }
        public string Owner { get; set; }
        public DateTime Issued { get; set; }
        public DateTime? Expires { get; set; }
        public bool SkipHardwareIdValidation { get; set; }

        [XmlIgnore]
        public bool IsEmpty => LicenseId == null;

        public static ISerializer<LicenseData> Serializer 
            => new CorrectXmlSerializer<LicenseData>();

        public static LicenseData Current
            => Environment.GetEnvironmentVariable(Constants.LicenseEnvVariableName) != null
                ? Serializer.Deserialize(Environment.GetEnvironmentVariable(Constants.LicenseEnvVariableName))
                : new LicenseData();
    }
}