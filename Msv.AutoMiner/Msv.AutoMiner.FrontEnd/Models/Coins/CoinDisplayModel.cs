﻿using System;
using Msv.AutoMiner.Common.Enums;
using Msv.AutoMiner.Data.Logic;

namespace Msv.AutoMiner.FrontEnd.Models.Coins
{
    public class CoinDisplayModel : CoinBaseModel
    {
        public ActivityState Activity { get; set; }

        public double Difficulty { get; set; }

        public double DifficultyDelta { get; set; }

        public double NetHashRate { get; set; }

        public double BlockReward { get; set; }

        public CoinExchangePrice[] ExchangePrices { get; set; }

        public long Height { get; set; }

        public DateTime? LastBlockTime { get; set; }

        public DateTime? LastUpdated { get; set; }

        public bool HasLocalNode { get; set; }

        public ExchangeType? MiningTargetExchange { get; set; }

        public int? MasternodeCount { get; set; }

        public double? TotalSupply { get; set; }
    }
}
