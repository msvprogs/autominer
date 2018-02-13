using System;
using Msv.AutoMiner.Common.Data.Enums;

namespace Msv.AutoMiner.FrontEnd.Models.Miners
{
    public class MinerDisplayModel : MinerBaseModel
    {
        public ActivityState Activity { get; set; }

        public string CurrentWindowsVersion { get; set; }

        public string CurrentLinuxVersion { get; set; }

        public DateTime? LastWindowsUpdated { get; set; }

        public DateTime? LastLinuxUpdated { get; set; }
    }
}