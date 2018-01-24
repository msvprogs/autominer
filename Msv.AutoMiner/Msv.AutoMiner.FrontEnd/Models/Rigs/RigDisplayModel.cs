using System;
using Msv.AutoMiner.Common.Enums;

namespace Msv.AutoMiner.FrontEnd.Models.Rigs
{
    public class RigDisplayModel : RigBaseModel
    {
        public ActivityState Activity { get; set; }
        
        public string CertificateSerial { get; set; }

        public DateTime? LastHeartbeat { get; set; }

        public string RemoteAddress { get;set; }
    }
}
