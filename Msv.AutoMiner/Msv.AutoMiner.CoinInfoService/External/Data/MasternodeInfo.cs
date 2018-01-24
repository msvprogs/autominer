using System;

namespace Msv.AutoMiner.CoinInfoService.External.Data
{
    public class MasternodeInfo
    {
        public string CurrencySymbol { get; set; }
        public int MasternodesCount { get; set; }
        public double TotalSupply { get; set; }
        public DateTime Updated { get; set; }
    }
}
