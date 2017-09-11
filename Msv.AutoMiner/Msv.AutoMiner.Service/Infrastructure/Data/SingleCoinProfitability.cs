using System;
using Msv.AutoMiner.Commons.Data;

namespace Msv.AutoMiner.Service.Infrastructure.Data
{
    public class SingleCoinProfitability
    {
        public Coin Coin { get; }
        public double CoinsPerDay { get; }

        public SingleCoinProfitability(Coin coin, double coinsPerDay)
        {
            if (coin == null)
                throw new ArgumentNullException(nameof(coin));

            Coin = coin;
            CoinsPerDay = coinsPerDay;
        }
    }
}
