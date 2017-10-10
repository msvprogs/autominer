using System;
using Msv.AutoMiner.Common.Models.ControlCenterService;

namespace Msv.AutoMiner.FrontEnd.Models.Rigs
{
    public class RigStatisticsModel : RigBaseModel
    {
        public Heartbeat LastHeartbeat { get; set; }

        public CoinProfitabilityInfo[] LastProfitabilityTable { get; set; }

        public CoinMiningDuration[] LastDayActivity { get; set; }

        public class CoinProfitabilityInfo
        {
            public Guid CoinId { get; set; }

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
