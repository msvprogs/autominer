using System;
using Msv.AutoMiner.Common.Enums;
using Msv.AutoMiner.FrontEnd.Models.Coins;

namespace Msv.AutoMiner.FrontEnd.Models.Wallets
{
    public class WalletDisplayModel : WalletBaseModel
    {
        public ActivityState Activity { get; set; }

        public CoinBaseModel Coin { get; set; }

        public bool IsMiningTarget { get; set; }

        public double CoinBtcPrice { get; set; }

        public double? LastDayVolume { get; set; }

        public bool IsInactive { get; set; }

        public double Available { get; set; }

        public double Blocked { get; set; }

        public double Unconfirmed { get; set; }

        public DateTime? LastUpdated { get; set; }
    }
}
