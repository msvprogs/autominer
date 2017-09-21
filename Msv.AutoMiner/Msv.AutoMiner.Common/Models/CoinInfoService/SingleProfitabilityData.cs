using System;

namespace Msv.AutoMiner.Common.Models.CoinInfoService
{
    public class SingleProfitabilityData
    {
        public Guid CoinId { get; set; }

        public string CoinName { get; set; }

        public string CoinSymbol { get; set; }

        public Guid AlgorithmId { get; set; }

        public double Difficulty { get; set; }

        public long NetHashRate { get; set; }

        public double BlockReward { get; set; }

        public long Height { get; set; }

        public DateTime LastUpdatedUtc { get; set; }

        public double CoinsPerDay { get; set; }

        public double ElectricityCostPerDay { get; set; }

        public MarketPriceData[] MarketPrices { get; set; }
    }
}