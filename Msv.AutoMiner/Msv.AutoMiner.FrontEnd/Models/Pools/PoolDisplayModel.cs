using System;
using Msv.AutoMiner.Common.Enums;
using Msv.AutoMiner.FrontEnd.Models.Coins;

namespace Msv.AutoMiner.FrontEnd.Models.Pools
{
    public class PoolDisplayModel : PoolBaseModel
    {
        public CoinBaseModel Coin { get; set; }

        public bool HasApi { get; set; }

        public ActivityState Activity { get; set; }

        public double ConfirmedBalance { get; set; }

        public double UnconfirmedBalance { get; set; }

        public double CoinBtcPrice { get; set; }

        public long PoolHashRate { get; set; }

        public int PoolWorkers { get; set; }

        public int Priority { get; set; }

        public double Fee { get; set; }

        public DateTime? LastUpdated { get; set; }
    }
}
