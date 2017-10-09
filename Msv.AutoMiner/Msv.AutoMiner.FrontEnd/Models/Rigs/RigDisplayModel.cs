using System;

namespace Msv.AutoMiner.FrontEnd.Models.Rigs
{
    public class RigDisplayModel : RigBaseModel
    {
        public bool IsActive { get; set; }
        
        public string CertificateSerial { get; set; }

        public DateTime? LastHeartbeat { get; set; }
    }
}
