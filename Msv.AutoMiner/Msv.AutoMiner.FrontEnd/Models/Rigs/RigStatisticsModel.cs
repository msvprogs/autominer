using System;
using Msv.AutoMiner.Common.Models.ControlCenterService;
using Msv.AutoMiner.Data;

namespace Msv.AutoMiner.FrontEnd.Models.Rigs
{
    public class RigStatisticsModel : RigBaseModel
    {
        public Heartbeat LastHeartbeat { get; set; }

        public DateTime? ProfitabilityTableTime { get; set; }

        public CoinProfitabilityInfo[] LastProfitabilityTable { get; set; }

        public CoinMiningDuration[] LastDayActivity { get; set; }

        public CoinAlgorithm[] Algorithms { get; set; }

        public class CoinProfitabilityInfo
        {
            public Guid CoinId { get; set; }

            public int PoolId { get; set; }

            public string CoinName { get; set; }

            public string CoinSymbol { get; set; }

            public string PoolName { get; set; }

            public double BtcPerDay { get; set; }

            public double UsdPerDay { get; set; }
        }

        public class CoinMiningDuration
        {
            public DateTime Time { get; set; }

            public string CoinName { get; set; }

            public string CoinSymbol { get; set; }

            public TimeSpan Duration { get; set; }
        }
    }
}
