﻿using System;

namespace Msv.AutoMiner.Data
{
    public class CoinNetworkInfo
    {
        public Guid CoinId { get; set; }

        public virtual Coin Coin { get; set; }

        public DateTime Created { get; set; }

        public double Difficulty { get; set; }

        public long NetHashRate { get; set; }

        public double BlockReward { get; set; }

        public double BlockTimeSeconds { get; set; }

        public long Height { get; set; }
    }
}
