using System;

namespace Msv.AutoMiner.Common.Models.ControlCenterService
{
    public class MiningWorkModel
    {
        public Guid CoinId { get; set; }

        public string CoinName { get; set; }

        public string CoinSymbol { get; set; }

        public Guid CoinAlgorithmId { get; set; }

        public PoolDataModel[] Pools { get; set; }
    }
}
