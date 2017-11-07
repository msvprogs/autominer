using System;
using Msv.AutoMiner.Common.Enums;

namespace Msv.AutoMiner.FrontEnd.Models.Coins
{
    public class CoinDisplayModel : CoinBaseModel
    {
        public ActivityState Activity { get; set; }

        public double Difficulty { get; set; }

        public double NetHashRate { get; set; }

        public double BlockReward { get; set; }

        public double BlockTimeSecs { get; set; }

        public long Height { get; set; }

        public DateTime? LastUpdated { get; set; }
    }
}
