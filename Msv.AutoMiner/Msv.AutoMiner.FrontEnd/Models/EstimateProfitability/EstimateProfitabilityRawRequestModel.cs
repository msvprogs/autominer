namespace Msv.AutoMiner.FrontEnd.Models.EstimateProfitability
{
    public class EstimateProfitabilityRawRequestModel
    {
        public string Difficulty { get; set; }

        public string BlockReward { get; set; }

        public string MaxTarget { get; set; }

        public string BtcPrice { get; set; }

        public string HashRate { get; set; }

        public string ClientPowerUsage { get; set; }

        public string ElectricityCostUsd { get; set; }

        public string KnownAlgorithm { get; set; }
    }
}
