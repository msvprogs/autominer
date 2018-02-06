using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Msv.AutoMiner.ControlCenterService.Configuration
{
    public class RigStatusLimitsElement
    {
        public int SamplesCount { get; set; }
        public int MinVideoUsage { get; set; }
        public int MaxVideoTemperature { get; set; }
        public int MaxInvalidSharesRate { get; set; }
        public int MaxHashrateDifference { get; set; }
    }
}
