using System;

namespace Msv.AutoMiner.Service.Infrastructure.Data
{
    public class MinerChangingOptions
    {
        public TimeSpan Interval { get; set; }
        public TimeSpan Dispersion { get; set; }
        public double ThresholdRatio { get; set; }
    }
}
