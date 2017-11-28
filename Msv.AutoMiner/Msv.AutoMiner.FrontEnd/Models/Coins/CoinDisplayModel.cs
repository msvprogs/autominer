using System;
using Msv.AutoMiner.Common.Enums;
using Msv.AutoMiner.FrontEnd.Data;

namespace Msv.AutoMiner.FrontEnd.Models.Coins
{
    public class CoinDisplayModel : CoinBaseModel
    {
        public ActivityState Activity { get; set; }

        public double Difficulty { get; set; }

        public double NetHashRate { get; set; }

        public double BlockReward { get; set; }

        public CoinExchangePrice[] ExchangePrices { get; set; }

        public long Height { get; set; }

        public DateTime? LastUpdated { get; set; }
    }
}
