﻿using System;
using Msv.AutoMiner.Common.Data.Enums;

namespace Msv.AutoMiner.FrontEnd.Models.Rigs
{
    public class RigDisplayModel : RigEditModel
    {
        public ActivityState Activity { get; set; }
        
        public string CertificateSerial { get; set; }

        public DateTime? LastHeartbeat { get; set; }

        public string RemoteAddress { get;set; }
    }
}
