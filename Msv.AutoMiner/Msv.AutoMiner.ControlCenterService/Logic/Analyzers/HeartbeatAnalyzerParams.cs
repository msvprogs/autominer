namespace Msv.AutoMiner.ControlCenterService.Logic.Analyzers
{
    public class HeartbeatAnalyzerParams
    {
        public int SamplesCount { get; set; }

        public int MinVideoUsage { get; set; }
        public int MaxVideoTemperature { get; set; }
        public int MaxInvalidSharesRate { get; set; }
        public int MaxHashrateDifference { get; set; }
    }
}
