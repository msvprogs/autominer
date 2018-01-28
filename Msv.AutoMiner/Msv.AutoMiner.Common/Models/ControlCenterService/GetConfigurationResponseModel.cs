namespace Msv.AutoMiner.Common.Models.ControlCenterService
{
    public class GetConfigurationResponseModel
    {
        public MinerModel[] Miners { get; set; }

        public AlgorithmMinerModel[] Algorithms { get; set; }
    }
}
