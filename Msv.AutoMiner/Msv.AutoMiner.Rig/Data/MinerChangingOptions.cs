using System;

namespace Msv.AutoMiner.Rig.Data
{
    public class MinerChangingOptions
    {
        public TimeSpan Interval { get; set; }
        public TimeSpan Dispersion { get; set; }
        public double ThresholdRatio { get; set; }
        public double LowestAverageGpuUsage { get; set; }
        public TimeSpan LowestGpuUsageSwitchInterval { get; set; }
    }
}
