using System;
using System.Linq;
using Msv.AutoMiner.Commons.Data;

namespace Msv.AutoMiner.Service.Infrastructure.Data
{
    public class CoinProfitabilityData
    {
        public long Id => Coins.Aggregate(0L, (x, y) => (x << 10) | (long)y.Coin.Id);
        public MiningMode Mode { get; set; }
        public SingleCoinProfitability[] Coins { get; set; }
        public double BtcPerDay { get; set; }
        public decimal UsdPerDay { get; set; }
        public double PowerUsage { get; set; }
        public decimal ElectricityCost { get; set; }
        public decimal UsdPerDayTotal => UsdPerDay - ElectricityCost;
        public Miner Miner { get; set; }

        public string ToCoinString()
            => CoinDataToString(x => x.Coin.CurrencySymbol);
        public string ToCoinNameString()
            => CoinDataToString(x => x.Coin.Name);
        public string ToCoinsPerDayString() 
            => CoinDataToString(x => x.CoinsPerDay.ToString("F4")).PadLeft(15);

        private string CoinDataToString(Func<SingleCoinProfitability, string> getter)
        {
            switch (Mode)
            {
                case MiningMode.Single:
                    return getter.Invoke(Coins[0]);
                case MiningMode.Double:
                case MiningMode.Merged:
                    return string.Join("+", Coins.Select(getter));
                default:
                    throw new ArgumentException("Invalid mining mode");
            }
        }
    }
}
